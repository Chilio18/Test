"""Lead Management System API channel adapter."""

import asyncio
from datetime import datetime
from typing import Optional, Callable, Awaitable
import aiohttp

from .base import BaseChannel, ChannelMessage


class LeadSystemChannel(BaseChannel):
    """Channel adapter for Lead Management System APIs.

    This adapter supports common CRM/Lead Management systems like:
    - Salesforce
    - HubSpot
    - DealerSocket
    - VinSolutions
    - Custom dealership systems

    The implementation is generic and can be customized via the configuration.
    """

    def __init__(
        self,
        api_url: str,
        api_key: str,
        api_secret: Optional[str] = None,
        poll_interval_seconds: int = 60,
        lead_endpoint: str = "/leads",
        message_endpoint: str = "/messages",
        auth_type: str = "bearer"  # bearer, basic, api_key
    ):
        """Initialize Lead System channel.

        Args:
            api_url: Base URL for the lead management API.
            api_key: API key or username for authentication.
            api_secret: API secret or password (for basic auth).
            poll_interval_seconds: How often to poll for new leads/messages.
            lead_endpoint: API endpoint for fetching leads.
            message_endpoint: API endpoint for sending messages.
            auth_type: Authentication type (bearer, basic, api_key).
        """
        super().__init__("lead_system")
        self.api_url = api_url.rstrip("/")
        self.api_key = api_key
        self.api_secret = api_secret
        self.poll_interval = poll_interval_seconds
        self.lead_endpoint = lead_endpoint
        self.message_endpoint = message_endpoint
        self.auth_type = auth_type

        self._session: Optional[aiohttp.ClientSession] = None
        self._running = False
        self._poll_task: Optional[asyncio.Task] = None
        self._processed_lead_ids: set[str] = set()
        self._last_check_time: Optional[datetime] = None

    def _get_auth_headers(self) -> dict:
        """Get authentication headers based on auth type."""
        if self.auth_type == "bearer":
            return {"Authorization": f"Bearer {self.api_key}"}
        elif self.auth_type == "api_key":
            return {"X-API-Key": self.api_key}
        elif self.auth_type == "basic":
            import base64
            credentials = base64.b64encode(
                f"{self.api_key}:{self.api_secret}".encode()
            ).decode()
            return {"Authorization": f"Basic {credentials}"}
        return {}

    async def start(self) -> None:
        """Start the lead system channel."""
        headers = self._get_auth_headers()
        headers["Content-Type"] = "application/json"

        self._session = aiohttp.ClientSession(headers=headers)
        self._running = True
        self._last_check_time = datetime.now()

        if self._message_handler:
            self._poll_task = asyncio.create_task(self._poll_loop())

    async def stop(self) -> None:
        """Stop the lead system channel."""
        self._running = False
        if self._poll_task:
            self._poll_task.cancel()
            try:
                await self._poll_task
            except asyncio.CancelledError:
                pass

        if self._session:
            await self._session.close()
            self._session = None

    async def _poll_loop(self) -> None:
        """Continuously poll for new leads and messages."""
        while self._running:
            try:
                await self._check_new_leads()
            except Exception as e:
                print(f"Error polling lead system: {e}")

            await asyncio.sleep(self.poll_interval)

    async def _check_new_leads(self) -> None:
        """Check for new leads from the lead management system."""
        if not self._session:
            return

        try:
            # Fetch new leads since last check
            params = {}
            if self._last_check_time:
                params["since"] = self._last_check_time.isoformat()
            params["status"] = "new,uncontacted"

            url = f"{self.api_url}{self.lead_endpoint}"
            async with self._session.get(url, params=params) as response:
                if response.status != 200:
                    return

                data = await response.json()
                leads = data.get("leads", data.get("data", []))

                for lead in leads:
                    lead_id = str(lead.get("id", lead.get("lead_id", "")))
                    if not lead_id or lead_id in self._processed_lead_ids:
                        continue

                    # Convert lead to channel message
                    channel_msg = self._parse_lead(lead)
                    if channel_msg and self._message_handler:
                        response_text = await self._message_handler(channel_msg)
                        if response_text:
                            await self.send_message(lead_id, response_text)
                            # Update lead status in the system
                            await self._update_lead_status(lead_id, "contacted")

                    self._processed_lead_ids.add(lead_id)

            self._last_check_time = datetime.now()

        except Exception as e:
            print(f"Error fetching leads: {e}")

    def _parse_lead(self, lead_data: dict) -> Optional[ChannelMessage]:
        """Parse lead data into a ChannelMessage.

        Args:
            lead_data: Lead data from the API.

        Returns:
            ChannelMessage representing the lead inquiry.
        """
        try:
            lead_id = str(lead_data.get("id", lead_data.get("lead_id", "")))

            # Extract customer info (field names vary by system)
            name_fields = ["name", "full_name", "customer_name", "contact_name"]
            name = None
            for field in name_fields:
                if lead_data.get(field):
                    name = lead_data[field]
                    break

            if not name:
                first = lead_data.get("first_name", lead_data.get("firstName", ""))
                last = lead_data.get("last_name", lead_data.get("lastName", ""))
                name = f"{first} {last}".strip() or None

            email = lead_data.get("email", lead_data.get("email_address", ""))
            phone = lead_data.get("phone", lead_data.get("phone_number", ""))

            # Build message content from lead inquiry
            content_parts = []

            # Vehicle interest
            vehicle_interest = lead_data.get("vehicle_interest", lead_data.get("vehicle", {}))
            if isinstance(vehicle_interest, dict):
                make = vehicle_interest.get("make", "")
                model = vehicle_interest.get("model", "")
                year = vehicle_interest.get("year", "")
                if make or model:
                    content_parts.append(f"Interested in: {year} {make} {model}".strip())
            elif isinstance(vehicle_interest, str) and vehicle_interest:
                content_parts.append(f"Interested in: {vehicle_interest}")

            # Customer comments/message
            comments = lead_data.get("comments", lead_data.get("message", lead_data.get("notes", "")))
            if comments:
                content_parts.append(comments)

            # Source info
            source = lead_data.get("source", lead_data.get("lead_source", ""))
            if source:
                content_parts.append(f"(Lead source: {source})")

            # Trade-in info
            trade_in = lead_data.get("trade_in", lead_data.get("tradeIn", {}))
            if trade_in and isinstance(trade_in, dict):
                trade_year = trade_in.get("year", "")
                trade_make = trade_in.get("make", "")
                trade_model = trade_in.get("model", "")
                if trade_make:
                    content_parts.append(f"Has trade-in: {trade_year} {trade_make} {trade_model}".strip())

            content = "\n".join(content_parts) if content_parts else "New lead - no specific inquiry"

            # Parse timestamp
            timestamp_str = lead_data.get("created_at", lead_data.get("createdAt", lead_data.get("timestamp", "")))
            try:
                if timestamp_str:
                    timestamp = datetime.fromisoformat(timestamp_str.replace("Z", "+00:00"))
                else:
                    timestamp = datetime.now()
            except Exception:
                timestamp = datetime.now()

            return ChannelMessage(
                message_id=lead_id,
                conversation_id=lead_id,
                sender_id=email or phone or lead_id,
                sender_name=name,
                sender_email=email,
                sender_phone=phone,
                content=content,
                timestamp=timestamp,
                channel="lead_system",
                metadata={
                    "source": source,
                    "vehicle_interest": vehicle_interest,
                    "trade_in": trade_in,
                    "raw_lead_data": lead_data
                }
            )

        except Exception as e:
            print(f"Error parsing lead data: {e}")
            return None

    async def send_message(self, conversation_id: str, message: str) -> bool:
        """Send a message/response for a lead.

        Args:
            conversation_id: Lead ID to respond to.
            message: Message content.

        Returns:
            True if sent successfully.
        """
        if not self._session:
            return False

        formatted_message = self.format_message_for_channel(message)

        # Different systems have different ways to add notes/responses
        # This is a generic implementation
        payload = {
            "lead_id": conversation_id,
            "message": formatted_message,
            "type": "ai_response",
            "timestamp": datetime.now().isoformat()
        }

        try:
            url = f"{self.api_url}{self.message_endpoint}"
            async with self._session.post(url, json=payload) as response:
                return response.status in [200, 201]
        except Exception as e:
            print(f"Error sending message to lead system: {e}")
            return False

    async def _update_lead_status(self, lead_id: str, status: str) -> bool:
        """Update lead status in the system.

        Args:
            lead_id: Lead ID to update.
            status: New status value.

        Returns:
            True if updated successfully.
        """
        if not self._session:
            return False

        payload = {"status": status}

        try:
            url = f"{self.api_url}{self.lead_endpoint}/{lead_id}"
            async with self._session.patch(url, json=payload) as response:
                return response.status in [200, 204]
        except Exception as e:
            print(f"Error updating lead status: {e}")
            return False

    async def send_template_message(
        self,
        conversation_id: str,
        template_name: str,
        template_params: dict
    ) -> bool:
        """Send a template message (if supported by the lead system).

        Args:
            conversation_id: Lead ID.
            template_name: Template identifier.
            template_params: Template parameters.

        Returns:
            True if sent successfully.
        """
        if not self._session:
            return False

        payload = {
            "lead_id": conversation_id,
            "template": template_name,
            "params": template_params,
            "timestamp": datetime.now().isoformat()
        }

        try:
            url = f"{self.api_url}{self.message_endpoint}/template"
            async with self._session.post(url, json=payload) as response:
                return response.status in [200, 201]
        except Exception as e:
            print(f"Error sending template to lead system: {e}")
            return False

    async def create_task(self, lead_id: str, task_type: str, description: str, due_date: Optional[datetime] = None) -> bool:
        """Create a follow-up task for a lead.

        Args:
            lead_id: Lead ID to create task for.
            task_type: Type of task (call, email, appointment).
            description: Task description.
            due_date: When the task is due.

        Returns:
            True if task created successfully.
        """
        if not self._session:
            return False

        payload = {
            "lead_id": lead_id,
            "task_type": task_type,
            "description": description,
            "due_date": due_date.isoformat() if due_date else None,
            "status": "pending"
        }

        try:
            url = f"{self.api_url}/tasks"
            async with self._session.post(url, json=payload) as response:
                return response.status in [200, 201]
        except Exception as e:
            print(f"Error creating task: {e}")
            return False

    def format_message_for_channel(self, message: str) -> str:
        """Format message for lead system (plain text, no markdown)."""
        # Remove markdown formatting for lead system notes
        import re
        # Remove bold/italic
        message = re.sub(r'\*\*(.+?)\*\*', r'\1', message)
        message = re.sub(r'\*(.+?)\*', r'\1', message)
        message = re.sub(r'_(.+?)_', r'\1', message)
        # Remove links
        message = re.sub(r'\[(.+?)\]\(.+?\)', r'\1', message)
        return message


def create_lead_system_webhook_handler(
    channel: LeadSystemChannel,
    message_handler: Callable[[ChannelMessage], Awaitable[str]]
):
    """Create a FastAPI router for lead system webhooks.

    Some lead systems support push notifications via webhooks.

    Args:
        channel: LeadSystemChannel instance.
        message_handler: Async function to handle incoming leads.

    Returns:
        FastAPI APIRouter for webhook endpoints.
    """
    from fastapi import APIRouter, Request

    router = APIRouter()

    @router.post("/webhook/lead")
    async def receive_lead_webhook(request: Request):
        """Handle incoming lead webhook."""
        try:
            payload = await request.json()

            # Parse the lead
            channel_msg = channel._parse_lead(payload)
            if channel_msg:
                response_text = await message_handler(channel_msg)
                if response_text:
                    await channel.send_message(
                        channel_msg.conversation_id,
                        response_text
                    )
                    await channel._update_lead_status(
                        channel_msg.conversation_id,
                        "contacted"
                    )

            return {"status": "ok"}
        except Exception as e:
            print(f"Webhook error: {e}")
            return {"status": "error", "message": str(e)}

    return router
