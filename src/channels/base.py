"""Base channel adapter interface."""

from abc import ABC, abstractmethod
from datetime import datetime
from typing import Optional, Callable, Awaitable
from pydantic import BaseModel


class ChannelMessage(BaseModel):
    """Represents a message received from any channel."""

    message_id: str
    conversation_id: str
    sender_id: str
    sender_name: Optional[str] = None
    sender_email: Optional[str] = None
    sender_phone: Optional[str] = None
    content: str
    timestamp: datetime
    channel: str
    metadata: dict = {}


class BaseChannel(ABC):
    """Abstract base class for channel adapters."""

    def __init__(self, channel_name: str):
        self.channel_name = channel_name
        self._message_handler: Optional[Callable[[ChannelMessage], Awaitable[str]]] = None

    def set_message_handler(self, handler: Callable[[ChannelMessage], Awaitable[str]]) -> None:
        """Set the handler function that processes incoming messages.

        Args:
            handler: Async function that takes a ChannelMessage and returns a response string.
        """
        self._message_handler = handler

    @abstractmethod
    async def start(self) -> None:
        """Start listening for messages on this channel."""
        pass

    @abstractmethod
    async def stop(self) -> None:
        """Stop listening for messages."""
        pass

    @abstractmethod
    async def send_message(self, conversation_id: str, message: str) -> bool:
        """Send a message to a conversation.

        Args:
            conversation_id: The conversation/thread ID.
            message: The message content to send.

        Returns:
            True if message was sent successfully.
        """
        pass

    @abstractmethod
    async def send_template_message(
        self,
        conversation_id: str,
        template_name: str,
        template_params: dict
    ) -> bool:
        """Send a template/structured message.

        Args:
            conversation_id: The conversation/thread ID.
            template_name: Name of the message template.
            template_params: Parameters for the template.

        Returns:
            True if message was sent successfully.
        """
        pass

    def format_message_for_channel(self, message: str) -> str:
        """Format a message appropriately for this channel.

        Override this method to apply channel-specific formatting.

        Args:
            message: The raw message from the agent.

        Returns:
            Formatted message for this channel.
        """
        return message
