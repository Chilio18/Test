"""Channel adapters for different communication platforms."""

from .base import BaseChannel, ChannelMessage
from .whatsapp import WhatsAppChannel
from .email import EmailChannel
from .lead_system import LeadSystemChannel

__all__ = [
    "BaseChannel",
    "ChannelMessage",
    "WhatsAppChannel",
    "EmailChannel",
    "LeadSystemChannel",
]
