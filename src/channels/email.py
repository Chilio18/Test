"""Email channel adapter using IMAP/SMTP."""

import asyncio
import email
import hashlib
import re
import smtplib
import ssl
from datetime import datetime
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText
from imaplib import IMAP4_SSL
from typing import Optional, Callable, Awaitable
import markdown

from .base import BaseChannel, ChannelMessage


class EmailChannel(BaseChannel):
    """Channel adapter for Email communication."""

    def __init__(
        self,
        smtp_host: str,
        smtp_port: int,
        smtp_user: str,
        smtp_password: str,
        imap_host: str,
        imap_port: int,
        from_email: str,
        from_name: str = "Car Sales Team",
        check_interval_seconds: int = 30,
        target_folder: str = "INBOX"
    ):
        """Initialize email channel.

        Args:
            smtp_host: SMTP server hostname.
            smtp_port: SMTP server port.
            smtp_user: SMTP username.
            smtp_password: SMTP password.
            imap_host: IMAP server hostname.
            imap_port: IMAP server port.
            from_email: Email address to send from.
            from_name: Display name for outgoing emails.
            check_interval_seconds: How often to check for new emails.
            target_folder: IMAP folder to monitor.
        """
        super().__init__("email")
        self.smtp_host = smtp_host
        self.smtp_port = smtp_port
        self.smtp_user = smtp_user
        self.smtp_password = smtp_password
        self.imap_host = imap_host
        self.imap_port = imap_port
        self.from_email = from_email
        self.from_name = from_name
        self.check_interval = check_interval_seconds
        self.target_folder = target_folder

        self._running = False
        self._check_task: Optional[asyncio.Task] = None
        self._processed_ids: set[str] = set()
        self._thread_mapping: dict[str, str] = {}  # message_id -> thread_id

    async def start(self) -> None:
        """Start checking for incoming emails."""
        self._running = True
        if self._message_handler:
            self._check_task = asyncio.create_task(self._email_check_loop())

    async def stop(self) -> None:
        """Stop checking for emails."""
        self._running = False
        if self._check_task:
            self._check_task.cancel()
            try:
                await self._check_task
            except asyncio.CancelledError:
                pass

    async def _email_check_loop(self) -> None:
        """Continuously check for new emails."""
        while self._running:
            try:
                await self._check_new_emails()
            except Exception as e:
                print(f"Error checking emails: {e}")

            await asyncio.sleep(self.check_interval)

    async def _check_new_emails(self) -> None:
        """Check for and process new emails."""
        # Run IMAP operations in executor to avoid blocking
        loop = asyncio.get_event_loop()
        messages = await loop.run_in_executor(None, self._fetch_new_emails)

        for msg in messages:
            if self._message_handler:
                response = await self._message_handler(msg)
                if response:
                    await self.send_message(
                        msg.conversation_id,
                        response,
                        subject=f"Re: {msg.metadata.get('subject', 'Your inquiry')}",
                        in_reply_to=msg.message_id,
                        references=msg.metadata.get("references", [])
                    )

    def _fetch_new_emails(self) -> list[ChannelMessage]:
        """Fetch new emails from IMAP server (blocking)."""
        messages = []

        try:
            with IMAP4_SSL(self.imap_host, self.imap_port) as imap:
                imap.login(self.smtp_user, self.smtp_password)
                imap.select(self.target_folder)

                # Search for unseen emails
                _, message_numbers = imap.search(None, "UNSEEN")

                for num in message_numbers[0].split():
                    _, msg_data = imap.fetch(num, "(RFC822)")
                    email_body = msg_data[0][1]
                    email_message = email.message_from_bytes(email_body)

                    message_id = email_message.get("Message-ID", "")
                    if message_id in self._processed_ids:
                        continue

                    # Parse email
                    channel_msg = self._parse_email(email_message)
                    if channel_msg:
                        messages.append(channel_msg)
                        self._processed_ids.add(message_id)

                        # Mark as seen
                        imap.store(num, "+FLAGS", "\\Seen")

        except Exception as e:
            print(f"IMAP error: {e}")

        return messages

    def _parse_email(self, email_message: email.message.Message) -> Optional[ChannelMessage]:
        """Parse an email message into a ChannelMessage."""
        try:
            # Get sender info
            from_header = email_message.get("From", "")
            sender_match = re.match(r'(?:"?([^"]*)"?\s)?<?([^>]+)>?', from_header)
            sender_name = sender_match.group(1) if sender_match else None
            sender_email = sender_match.group(2) if sender_match else from_header

            # Get message ID and references for threading
            message_id = email_message.get("Message-ID", "")
            in_reply_to = email_message.get("In-Reply-To", "")
            references_header = email_message.get("References", "")
            references = references_header.split() if references_header else []

            # Determine conversation/thread ID
            if in_reply_to and in_reply_to in self._thread_mapping:
                conversation_id = self._thread_mapping[in_reply_to]
            elif references:
                # Check if any reference is a known thread
                for ref in references:
                    if ref in self._thread_mapping:
                        conversation_id = self._thread_mapping[ref]
                        break
                else:
                    # New conversation based on sender email
                    conversation_id = hashlib.md5(sender_email.encode()).hexdigest()[:12]
            else:
                conversation_id = hashlib.md5(sender_email.encode()).hexdigest()[:12]

            self._thread_mapping[message_id] = conversation_id

            # Get email body
            content = ""
            if email_message.is_multipart():
                for part in email_message.walk():
                    if part.get_content_type() == "text/plain":
                        payload = part.get_payload(decode=True)
                        if payload:
                            content = payload.decode("utf-8", errors="replace")
                            break
            else:
                payload = email_message.get_payload(decode=True)
                if payload:
                    content = payload.decode("utf-8", errors="replace")

            # Clean up email content (remove quoted replies)
            content = self._clean_email_body(content)

            # Parse date
            date_str = email_message.get("Date", "")
            try:
                timestamp = email.utils.parsedate_to_datetime(date_str)
            except Exception:
                timestamp = datetime.now()

            return ChannelMessage(
                message_id=message_id,
                conversation_id=conversation_id,
                sender_id=sender_email,
                sender_name=sender_name,
                sender_email=sender_email,
                content=content,
                timestamp=timestamp,
                channel="email",
                metadata={
                    "subject": email_message.get("Subject", ""),
                    "in_reply_to": in_reply_to,
                    "references": references
                }
            )

        except Exception as e:
            print(f"Error parsing email: {e}")
            return None

    def _clean_email_body(self, content: str) -> str:
        """Remove quoted replies and signatures from email body."""
        lines = content.split("\n")
        cleaned_lines = []

        for line in lines:
            # Stop at common reply markers
            if line.startswith(">"):
                continue
            if line.strip().startswith("On ") and " wrote:" in line:
                break
            if line.strip() == "---" or line.strip() == "___":
                break
            if line.strip().lower().startswith("from:"):
                break

            cleaned_lines.append(line)

        return "\n".join(cleaned_lines).strip()

    async def send_message(
        self,
        conversation_id: str,
        message: str,
        subject: Optional[str] = None,
        in_reply_to: Optional[str] = None,
        references: Optional[list[str]] = None
    ) -> bool:
        """Send an email message.

        Args:
            conversation_id: The recipient's email address or thread ID.
            message: Message content (will be converted to HTML).
            subject: Email subject line.
            in_reply_to: Message-ID being replied to.
            references: List of Message-IDs for threading.

        Returns:
            True if sent successfully.
        """
        # If conversation_id is a hash, we need to look up the actual email
        # In production, this would come from a database
        recipient_email = conversation_id
        if "@" not in conversation_id:
            # Try to find email from thread mapping (simplified)
            for msg_id, thread_id in self._thread_mapping.items():
                if thread_id == conversation_id:
                    # Would need to lookup the original sender
                    pass

        formatted_message = self.format_message_for_channel(message)

        # Run SMTP in executor
        loop = asyncio.get_event_loop()
        return await loop.run_in_executor(
            None,
            self._send_email_sync,
            recipient_email,
            subject or "Response from Car Sales Team",
            formatted_message,
            in_reply_to,
            references
        )

    def _send_email_sync(
        self,
        to_email: str,
        subject: str,
        body: str,
        in_reply_to: Optional[str] = None,
        references: Optional[list[str]] = None
    ) -> bool:
        """Send email synchronously (blocking)."""
        try:
            msg = MIMEMultipart("alternative")
            msg["Subject"] = subject
            msg["From"] = f"{self.from_name} <{self.from_email}>"
            msg["To"] = to_email

            if in_reply_to:
                msg["In-Reply-To"] = in_reply_to
            if references:
                msg["References"] = " ".join(references + ([in_reply_to] if in_reply_to else []))

            # Plain text version
            msg.attach(MIMEText(body, "plain"))

            # HTML version
            html_body = markdown.markdown(body)
            html_content = f"""
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                {html_body}
                <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;">
                <p style="font-size: 12px; color: #666;">
                    {self.from_name}<br>
                    {self.from_email}
                </p>
            </body>
            </html>
            """
            msg.attach(MIMEText(html_content, "html"))

            # Send via SMTP
            context = ssl.create_default_context()
            with smtplib.SMTP(self.smtp_host, self.smtp_port) as server:
                server.starttls(context=context)
                server.login(self.smtp_user, self.smtp_password)
                server.sendmail(self.from_email, [to_email], msg.as_string())

            return True

        except Exception as e:
            print(f"SMTP error: {e}")
            return False

    async def send_template_message(
        self,
        conversation_id: str,
        template_name: str,
        template_params: dict
    ) -> bool:
        """Send a template email.

        Args:
            conversation_id: Recipient email address.
            template_name: Template identifier.
            template_params: Parameters including subject and body placeholders.

        Returns:
            True if sent successfully.
        """
        # Templates would typically be loaded from a database or file
        templates = {
            "appointment_confirmation": {
                "subject": "Appointment Confirmed - {appointment_type}",
                "body": """
Dear {customer_name},

Your {appointment_type} appointment has been confirmed for:

**Date & Time:** {date_time}
**Location:** {dealership_address}

{vehicle_info}

Please bring a valid driver's license and proof of insurance.

If you need to reschedule or have any questions, please reply to this email or call us at {dealership_phone}.

We look forward to seeing you!

Best regards,
{salesperson_name}
"""
            },
            "follow_up": {
                "subject": "Following up on your {vehicle_interest} inquiry",
                "body": """
Hi {customer_name},

Thank you for your interest in the {vehicle_interest}. I wanted to follow up and see if you have any questions.

{personalized_message}

Would you like to schedule a time to come in for a test drive? We have availability this week.

Best regards,
{salesperson_name}
"""
            }
        }

        template = templates.get(template_name)
        if not template:
            return False

        subject = template["subject"].format(**template_params)
        body = template["body"].format(**template_params)

        return await self.send_message(
            conversation_id,
            body,
            subject=subject
        )

    def format_message_for_channel(self, message: str) -> str:
        """Format message for email (keeps markdown for conversion)."""
        return message
