"""Main entry point for the Car Sales AI Agent."""

import asyncio
from contextlib import asynccontextmanager
from pathlib import Path

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
import uvicorn

from config.settings import get_settings
from src.agent import CarSalesAgent
from src.channels import WhatsAppChannel, EmailChannel, LeadSystemChannel
from src.channels.base import ChannelMessage
from src.channels.whatsapp import create_whatsapp_webhook_handler
from src.channels.lead_system import create_lead_system_webhook_handler


# Global agent instance
agent: CarSalesAgent = None


async def handle_message(message: ChannelMessage) -> str:
    """Handle incoming messages from any channel.

    Args:
        message: The incoming channel message.

    Returns:
        Response text from the agent.
    """
    global agent

    # Build customer info from the message
    customer_info = {
        "name": message.sender_name,
        "email": message.sender_email,
        "phone": message.sender_phone,
        "initial_inquiry": message.content if len(message.content) > 10 else None
    }

    # Use conversation_id to maintain context
    if message.conversation_id not in agent._conversations:
        agent.start_conversation(
            message.conversation_id,
            channel=message.channel,
            customer_info=customer_info
        )

    # Process the message
    response = agent.process_message(
        message.conversation_id,
        message.content,
        channel=message.channel
    )

    return response


@asynccontextmanager
async def lifespan(app: FastAPI):
    """FastAPI lifespan handler for startup/shutdown."""
    global agent

    settings = get_settings()

    # Initialize the agent
    inventory_path = settings.inventory_path or str(
        Path(__file__).parent / "data" / "sample_inventory.json"
    )

    agent = CarSalesAgent(
        api_key=settings.anthropic_api_key or None,
        model=settings.model_name,
        dealership_config=settings.dealership_config,
        inventory_path=inventory_path
    )

    # Initialize channels if configured
    channels = []

    # WhatsApp
    if settings.whatsapp_api_url and settings.whatsapp_api_token:
        whatsapp = WhatsAppChannel(
            api_url=settings.whatsapp_api_url,
            api_token=settings.whatsapp_api_token,
            phone_number_id=settings.whatsapp_phone_number_id,
            verify_token=settings.whatsapp_verify_token,
            app_secret=settings.whatsapp_app_secret
        )
        await whatsapp.start()
        channels.append(whatsapp)

        # Add webhook routes
        whatsapp_router = create_whatsapp_webhook_handler(whatsapp, handle_message)
        app.include_router(whatsapp_router, prefix="/whatsapp", tags=["whatsapp"])

    # Email
    if settings.smtp_host and settings.imap_host:
        email_channel = EmailChannel(
            smtp_host=settings.smtp_host,
            smtp_port=settings.smtp_port,
            smtp_user=settings.smtp_user,
            smtp_password=settings.smtp_password,
            imap_host=settings.imap_host,
            imap_port=settings.imap_port,
            from_email=settings.dealership_email,
            from_name=settings.dealership_name
        )
        email_channel.set_message_handler(handle_message)
        await email_channel.start()
        channels.append(email_channel)

    # Lead System
    if settings.lead_system_api_url and settings.lead_system_api_key:
        lead_system = LeadSystemChannel(
            api_url=settings.lead_system_api_url,
            api_key=settings.lead_system_api_key,
            api_secret=settings.lead_system_api_secret
        )
        lead_system.set_message_handler(handle_message)
        await lead_system.start()
        channels.append(lead_system)

        # Add webhook routes
        lead_router = create_lead_system_webhook_handler(lead_system, handle_message)
        app.include_router(lead_router, prefix="/leads", tags=["leads"])

    print(f"Car Sales Agent started with {len(channels)} channel(s)")

    yield

    # Cleanup
    for channel in channels:
        await channel.stop()


# Create FastAPI app
app = FastAPI(
    title="Car Sales AI Agent",
    description="AI-powered assistant for car dealership sales teams",
    version="1.0.0",
    lifespan=lifespan
)

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/")
async def root():
    """Root endpoint."""
    return {
        "name": "Car Sales AI Agent",
        "version": "1.0.0",
        "status": "running"
    }


@app.get("/health")
async def health():
    """Health check endpoint."""
    return {"status": "healthy"}


@app.post("/api/chat")
async def chat(conversation_id: str, message: str, channel: str = "api"):
    """Direct chat endpoint for testing.

    Args:
        conversation_id: Unique conversation identifier.
        message: User message.
        channel: Channel name (default: api).

    Returns:
        Agent response.
    """
    global agent

    if agent is None:
        return {"error": "Agent not initialized"}

    # Start conversation if needed
    if conversation_id not in agent._conversations:
        agent.start_conversation(conversation_id, channel=channel)

    # Process message
    response = agent.process_message(conversation_id, message, channel=channel)

    # Get lead summary
    lead_summary = agent.get_lead_summary(conversation_id)

    return {
        "response": response,
        "conversation_id": conversation_id,
        "lead": lead_summary.get("lead") if lead_summary and lead_summary.get("found") else None
    }


@app.get("/api/lead/{conversation_id}")
async def get_lead(conversation_id: str):
    """Get lead information for a conversation.

    Args:
        conversation_id: The conversation identifier.

    Returns:
        Lead summary.
    """
    global agent

    if agent is None:
        return {"error": "Agent not initialized"}

    summary = agent.get_lead_summary(conversation_id)
    if summary and summary.get("found"):
        return summary["lead"]
    return {"error": "Lead not found"}


def main():
    """Run the server."""
    settings = get_settings()
    uvicorn.run(
        "main:app",
        host=settings.host,
        port=settings.port,
        reload=settings.debug
    )


if __name__ == "__main__":
    main()
