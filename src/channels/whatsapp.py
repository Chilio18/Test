"""WhatsApp Business API channel adapter."""

import asyncio
import hashlib
import hmac
import json
from datetime import datetime
from typing import Optional, Callable, Awaitable
import aiohttp

from .base import BaseChannel, ChannelMessage


class WhatsAppChannel(BaseChannel):
    """Channel adapter for WhatsApp Business API."""

    def __init__(
        self,
        api_url: str,
        api_token: str,
        phone_number_id: str,
        verify_token: str,
        app_secret: Optional[str] = None,
        webhook_port: int = 8080
    ):
        """Initialize WhatsApp channel.

        Args:
            api_url: WhatsApp Business API URL.
            api_token: Access token for the API.
            phone_number_id: Your WhatsApp Business phone number ID.
            verify_token: Token for webhook verification.
            app_secret: App secret for webhook signature verification.
            webhook_port: Port to listen for webhooks.
        """
        super().__init__("whatsapp")
        self.api_url = api_url.rstrip("/")
        self.api_token = api_token
        self.phone_number_id = phone_number_id
        self.verify_token = verify_token
        self.app_secret = app_secret
        self.webhook_port = webhook_port
        self._session: Optional[aiohttp.ClientSession] = None
        self._webhook_server = None

    async def start(self) -> None:
        """Start the WhatsApp channel (initialize HTTP session)."""
        self._session = aiohttp.ClientSession(
            headers={
                "Authorization": f"Bearer {self.api_token}",
                "Content-Type": "application/json"
            }
        )

    async def stop(self) -> None:
        """Stop the WhatsApp channel."""
        if self._session:
            await self._session.close()
            self._session = None

    def verify_webhook_signature(self, payload: bytes, signature: str) -> bool:
        """Verify webhook request signature.

        Args:
            payload: Raw request body.
            signature: X-Hub-Signature-256 header value.

        Returns:
            True if signature is valid.
        """
        if not self.app_secret:
            return True  # Skip verification if no secret configured

        expected = hmac.new(
            self.app_secret.encode(),
            payload,
            hashlib.sha256
        ).hexdigest()

        return hmac.compare_digest(f"sha256={expected}", signature)

    def parse_webhook_message(self, payload: dict) -> Optional[ChannelMessage]:
        """Parse incoming webhook payload into a ChannelMessage.

        Args:
            payload: Webhook JSON payload.

        Returns:
            ChannelMessage if valid message found, None otherwise.
        """
        try:
            entry = payload.get("entry", [{}])[0]
            changes = entry.get("changes", [{}])[0]
            value = changes.get("value", {})

            messages = value.get("messages", [])
            if not messages:
                return None

            message = messages[0]
            contact = value.get("contacts", [{}])[0]

            # Handle text messages
            if message.get("type") == "text":
                content = message["text"]["body"]
            elif message.get("type") == "button":
                content = message["button"]["text"]
            elif message.get("type") == "interactive":
                interactive = message["interactive"]
                if interactive.get("type") == "button_reply":
                    content = interactive["button_reply"]["title"]
                elif interactive.get("type") == "list_reply":
                    content = interactive["list_reply"]["title"]
                else:
                    content = "[Interactive message]"
            else:
                content = f"[{message.get('type', 'unknown')} message]"

            return ChannelMessage(
                message_id=message["id"],
                conversation_id=message["from"],  # Phone number
                sender_id=message["from"],
                sender_name=contact.get("profile", {}).get("name"),
                sender_phone=message["from"],
                content=content,
                timestamp=datetime.fromtimestamp(int(message["timestamp"])),
                channel="whatsapp",
                metadata={
                    "message_type": message.get("type"),
                    "wa_id": contact.get("wa_id")
                }
            )
        except (KeyError, IndexError) as e:
            print(f"Error parsing WhatsApp webhook: {e}")
            return None

    async def send_message(self, conversation_id: str, message: str) -> bool:
        """Send a text message via WhatsApp.

        Args:
            conversation_id: Recipient phone number.
            message: Message text.

        Returns:
            True if sent successfully.
        """
        if not self._session:
            return False

        formatted_message = self.format_message_for_channel(message)

        payload = {
            "messaging_product": "whatsapp",
            "recipient_type": "individual",
            "to": conversation_id,
            "type": "text",
            "text": {
                "preview_url": True,
                "body": formatted_message
            }
        }

        try:
            url = f"{self.api_url}/{self.phone_number_id}/messages"
            async with self._session.post(url, json=payload) as response:
                return response.status == 200
        except Exception as e:
            print(f"Error sending WhatsApp message: {e}")
            return False

    async def send_template_message(
        self,
        conversation_id: str,
        template_name: str,
        template_params: dict
    ) -> bool:
        """Send a template message via WhatsApp.

        Args:
            conversation_id: Recipient phone number.
            template_name: WhatsApp template name.
            template_params: Template parameters.

        Returns:
            True if sent successfully.
        """
        if not self._session:
            return False

        # Build template components
        components = []
        if template_params.get("header_params"):
            components.append({
                "type": "header",
                "parameters": [
                    {"type": "text", "text": p}
                    for p in template_params["header_params"]
                ]
            })
        if template_params.get("body_params"):
            components.append({
                "type": "body",
                "parameters": [
                    {"type": "text", "text": p}
                    for p in template_params["body_params"]
                ]
            })

        payload = {
            "messaging_product": "whatsapp",
            "recipient_type": "individual",
            "to": conversation_id,
            "type": "template",
            "template": {
                "name": template_name,
                "language": {"code": template_params.get("language", "en")},
                "components": components
            }
        }

        try:
            url = f"{self.api_url}/{self.phone_number_id}/messages"
            async with self._session.post(url, json=payload) as response:
                return response.status == 200
        except Exception as e:
            print(f"Error sending WhatsApp template: {e}")
            return False

    async def send_interactive_buttons(
        self,
        conversation_id: str,
        body_text: str,
        buttons: list[dict]
    ) -> bool:
        """Send an interactive message with buttons.

        Args:
            conversation_id: Recipient phone number.
            body_text: Message body text.
            buttons: List of button definitions [{"id": "btn1", "title": "Option 1"}, ...]

        Returns:
            True if sent successfully.
        """
        if not self._session:
            return False

        payload = {
            "messaging_product": "whatsapp",
            "recipient_type": "individual",
            "to": conversation_id,
            "type": "interactive",
            "interactive": {
                "type": "button",
                "body": {"text": body_text},
                "action": {
                    "buttons": [
                        {
                            "type": "reply",
                            "reply": {
                                "id": btn["id"],
                                "title": btn["title"][:20]  # WhatsApp limit
                            }
                        }
                        for btn in buttons[:3]  # WhatsApp limit: 3 buttons
                    ]
                }
            }
        }

        try:
            url = f"{self.api_url}/{self.phone_number_id}/messages"
            async with self._session.post(url, json=payload) as response:
                return response.status == 200
        except Exception as e:
            print(f"Error sending WhatsApp buttons: {e}")
            return False

    def format_message_for_channel(self, message: str) -> str:
        """Format message for WhatsApp (supports basic markdown).

        Args:
            message: Raw message from agent.

        Returns:
            WhatsApp-formatted message.
        """
        # WhatsApp supports: *bold*, _italic_, ~strikethrough~, ```code```
        # The agent's markdown should mostly work, but we can add specific formatting here

        # Truncate very long messages (WhatsApp limit is 4096 chars)
        if len(message) > 4000:
            message = message[:3997] + "..."

        return message


def create_whatsapp_webhook_handler(
    channel: WhatsAppChannel,
    message_handler: Callable[[ChannelMessage], Awaitable[str]]
):
    """Create a FastAPI router for WhatsApp webhooks.

    Args:
        channel: WhatsApp channel instance.
        message_handler: Async function to handle incoming messages.

    Returns:
        FastAPI APIRouter for webhook endpoints.
    """
    from fastapi import APIRouter, Request, Response

    router = APIRouter()

    @router.get("/webhook")
    async def verify_webhook(request: Request):
        """Handle webhook verification from WhatsApp."""
        params = request.query_params
        mode = params.get("hub.mode")
        token = params.get("hub.verify_token")
        challenge = params.get("hub.challenge")

        if mode == "subscribe" and token == channel.verify_token:
            return Response(content=challenge, media_type="text/plain")
        return Response(status_code=403)

    @router.post("/webhook")
    async def receive_webhook(request: Request):
        """Handle incoming webhook messages."""
        # Verify signature if configured
        signature = request.headers.get("X-Hub-Signature-256", "")
        body = await request.body()

        if not channel.verify_webhook_signature(body, signature):
            return Response(status_code=401)

        payload = await request.json()

        # Parse and handle message
        channel_message = channel.parse_webhook_message(payload)
        if channel_message:
            # Process message and send response
            response_text = await message_handler(channel_message)
            if response_text:
                await channel.send_message(
                    channel_message.conversation_id,
                    response_text
                )

        return {"status": "ok"}

    return router
