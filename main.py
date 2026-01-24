"""Main entry point for the Car Sales AI Agent with Frontend Support."""

import json
from datetime import datetime
from contextlib import asynccontextmanager
from pathlib import Path
from typing import Optional

from fastapi import FastAPI, Request
from fastapi.middleware.cors import CORSMiddleware
from fastapi.staticfiles import StaticFiles
from fastapi.responses import HTMLResponse, FileResponse
from pydantic import BaseModel
import uvicorn

from config.settings import get_settings
from src.agent import CarSalesAgent
from src.channels import WhatsAppChannel, EmailChannel, LeadSystemChannel
from src.channels.base import ChannelMessage
from src.channels.whatsapp import create_whatsapp_webhook_handler
from src.channels.lead_system import create_lead_system_webhook_handler


# Global state
agent: CarSalesAgent = None
conversation_metadata: dict = {}  # Store additional metadata per conversation


class ChatRequest(BaseModel):
    conversation_id: str
    message: str
    channel: str = "website"


class MessageRequest(BaseModel):
    message: str
    from_salesperson: bool = False


class NoteRequest(BaseModel):
    note: str


def get_conversation_metadata(conversation_id: str) -> dict:
    """Get or create metadata for a conversation."""
    if conversation_id not in conversation_metadata:
        conversation_metadata[conversation_id] = {
            "human_mode": False,
            "last_activity": datetime.now().isoformat(),
            "customer_name": None,
            "unread": False,
            "messages": []
        }
    return conversation_metadata[conversation_id]


def update_conversation_activity(conversation_id: str, message: str = None, is_user: bool = True):
    """Update conversation activity timestamp and last message."""
    meta = get_conversation_metadata(conversation_id)
    meta["last_activity"] = datetime.now().isoformat()
    if message:
        meta["last_message"] = message[:50] + "..." if len(message) > 50 else message
        meta["messages"].append({
            "role": "user" if is_user else "assistant",
            "content": message,
            "timestamp": datetime.now().isoformat(),
            "human_response": meta["human_mode"] and not is_user
        })


async def handle_message(message: ChannelMessage) -> str:
    """Handle incoming messages from any channel."""
    global agent

    customer_info = {
        "name": message.sender_name,
        "email": message.sender_email,
        "phone": message.sender_phone,
        "initial_inquiry": message.content if len(message.content) > 10 else None
    }

    if message.conversation_id not in agent._conversations:
        agent.start_conversation(
            message.conversation_id,
            channel=message.channel,
            customer_info=customer_info
        )

        # Update metadata with customer info
        meta = get_conversation_metadata(message.conversation_id)
        meta["customer_name"] = message.sender_name or f"Klant {message.conversation_id[-4:]}"

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

    # Initialize the agent with Wittebrug configuration
    inventory_path = settings.inventory_path or str(
        Path(__file__).parent / "data" / "sample_inventory.json"
    )

    # Wittebrug dealership configuration
    wittebrug_config = {
        "name": "Wittebrug",
        "address": "Donau 120, 2491 BC Den Haag",
        "phone": "088-7384480",
        "email": "info@wittebrug.nl",
        "website": "https://wittebrug.nl",
        "business_hours": "Maandag-Zaterdag, 9:00 - 18:00",
        "business_hours_start": "09:00",
        "business_hours_end": "18:00",
        "business_days": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]
    }

    agent = CarSalesAgent(
        api_key=settings.anthropic_api_key or None,
        model=settings.model_name,
        dealership_config=wittebrug_config,
        inventory_path=inventory_path,
        language="nl"  # Dutch language
    )

    # Initialize channels if configured
    channels = []

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
        whatsapp_router = create_whatsapp_webhook_handler(whatsapp, handle_message)
        app.include_router(whatsapp_router, prefix="/whatsapp", tags=["whatsapp"])

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

    if settings.lead_system_api_url and settings.lead_system_api_key:
        lead_system = LeadSystemChannel(
            api_url=settings.lead_system_api_url,
            api_key=settings.lead_system_api_key,
            api_secret=settings.lead_system_api_secret
        )
        lead_system.set_message_handler(handle_message)
        await lead_system.start()
        channels.append(lead_system)
        lead_router = create_lead_system_webhook_handler(lead_system, handle_message)
        app.include_router(lead_router, prefix="/leads", tags=["leads"])

    print(f"Wittebrug Car Sales Agent started with {len(channels)} channel(s)")
    print(f"Customer chat: http://localhost:8000/chat")
    print(f"Salesperson dashboard: http://localhost:8000/dashboard")

    yield

    for channel in channels:
        await channel.stop()


# Create FastAPI app
app = FastAPI(
    title="Wittebrug Car Sales AI Agent",
    description="AI-powered assistant for Wittebrug car dealership",
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

# Mount static files
app.mount("/static", StaticFiles(directory=Path(__file__).parent / "static"), name="static")


# ============================================
# Frontend Routes
# ============================================

@app.get("/", response_class=HTMLResponse)
async def root():
    """Redirect to customer chat."""
    return """
    <html>
        <head>
            <meta http-equiv="refresh" content="0; url=/chat" />
        </head>
        <body>
            <p>Redirecting to <a href="/chat">chat</a>...</p>
        </body>
    </html>
    """


@app.get("/chat", response_class=HTMLResponse)
async def chat_page():
    """Serve customer chat widget page."""
    template_path = Path(__file__).parent / "templates" / "chat_widget.html"
    return HTMLResponse(content=template_path.read_text())


@app.get("/dashboard", response_class=HTMLResponse)
async def dashboard_page():
    """Serve salesperson dashboard."""
    template_path = Path(__file__).parent / "templates" / "dashboard.html"
    return HTMLResponse(content=template_path.read_text())


@app.get("/health")
async def health():
    """Health check endpoint."""
    return {"status": "healthy"}


# ============================================
# Chat API Endpoints
# ============================================

@app.post("/api/chat")
async def chat(request: ChatRequest):
    """Handle chat messages from customers."""
    global agent

    if agent is None:
        return {"error": "Agent not initialized"}

    conversation_id = request.conversation_id
    meta = get_conversation_metadata(conversation_id)

    # Check if human has taken over
    if meta["human_mode"]:
        # Store message but don't process with AI
        update_conversation_activity(conversation_id, request.message, is_user=True)
        meta["unread"] = True
        return {
            "response": None,
            "human_mode": True,
            "message": "Een medewerker zal zo snel mogelijk reageren."
        }

    # Start conversation if needed
    if conversation_id not in agent._conversations:
        agent.start_conversation(conversation_id, channel=request.channel)
        meta["customer_name"] = f"Klant {conversation_id[-4:]}"

    # Update activity
    update_conversation_activity(conversation_id, request.message, is_user=True)

    # Process with AI
    response = agent.process_message(conversation_id, request.message, channel=request.channel)

    # Store response
    update_conversation_activity(conversation_id, response, is_user=False)

    # Get lead info
    lead_summary = agent.get_lead_summary(conversation_id)
    if lead_summary and lead_summary.get("found"):
        lead = lead_summary["lead"]
        meta["customer_name"] = lead.get("name") or meta["customer_name"]
        meta["qualification_level"] = lead.get("qualification", {}).get("level", "cold")

    return {
        "response": response,
        "conversation_id": conversation_id,
        "human_mode": False
    }


@app.get("/api/conversation/{conversation_id}/status")
async def get_conversation_status(conversation_id: str):
    """Get conversation status (for polling)."""
    meta = get_conversation_metadata(conversation_id)
    return {
        "human_mode": meta.get("human_mode", False),
        "last_activity": meta.get("last_activity")
    }


# ============================================
# Dashboard API Endpoints
# ============================================

@app.get("/api/conversations")
async def list_conversations():
    """List all active conversations for the dashboard."""
    global agent

    conversations = []

    if agent:
        for conv_id in agent._conversations.keys():
            meta = get_conversation_metadata(conv_id)
            lead_summary = agent.get_lead_summary(conv_id)

            conv_data = {
                "id": conv_id,
                "customer_name": meta.get("customer_name", f"Klant {conv_id[-4:]}"),
                "last_message": meta.get("last_message", ""),
                "last_activity": meta.get("last_activity"),
                "human_mode": meta.get("human_mode", False),
                "unread": meta.get("unread", False),
                "qualification_level": "cold"
            }

            if lead_summary and lead_summary.get("found"):
                lead = lead_summary["lead"]
                conv_data["customer_name"] = lead.get("name") or conv_data["customer_name"]
                conv_data["qualification_level"] = lead.get("qualification", {}).get("level", "cold")

            conversations.append(conv_data)

    return {"conversations": conversations}


@app.get("/api/conversation/{conversation_id}")
async def get_conversation(conversation_id: str):
    """Get full conversation details."""
    global agent

    if agent is None:
        return {"error": "Agent not initialized"}

    meta = get_conversation_metadata(conversation_id)
    meta["unread"] = False  # Mark as read

    # Get lead info for customer name
    lead_summary = agent.get_lead_summary(conversation_id)
    customer_name = meta.get("customer_name", f"Klant {conversation_id[-4:]}")
    if lead_summary and lead_summary.get("found"):
        customer_name = lead_summary["lead"].get("name") or customer_name

    return {
        "id": conversation_id,
        "customer_name": customer_name,
        "human_mode": meta.get("human_mode", False),
        "messages": meta.get("messages", [])
    }


@app.post("/api/conversation/{conversation_id}/takeover")
async def takeover_conversation(conversation_id: str):
    """Salesperson takes over a conversation from AI."""
    meta = get_conversation_metadata(conversation_id)
    meta["human_mode"] = True

    # Add system message to conversation
    meta["messages"].append({
        "role": "system",
        "content": "Een verkoper heeft het gesprek overgenomen.",
        "timestamp": datetime.now().isoformat()
    })

    return {"success": True, "human_mode": True}


@app.post("/api/conversation/{conversation_id}/handback")
async def handback_conversation(conversation_id: str):
    """Hand conversation back to AI."""
    meta = get_conversation_metadata(conversation_id)
    meta["human_mode"] = False

    # Add system message
    meta["messages"].append({
        "role": "system",
        "content": "Het gesprek is teruggegeven aan de AI-assistent.",
        "timestamp": datetime.now().isoformat()
    })

    return {"success": True, "human_mode": False}


@app.post("/api/conversation/{conversation_id}/message")
async def send_salesperson_message(conversation_id: str, request: MessageRequest):
    """Send a message from the salesperson."""
    global agent

    meta = get_conversation_metadata(conversation_id)

    if not meta.get("human_mode"):
        return {"error": "Conversation is in AI mode. Take over first."}

    # Store the message
    meta["messages"].append({
        "role": "assistant",
        "content": request.message,
        "timestamp": datetime.now().isoformat(),
        "human_response": True
    })
    meta["last_message"] = request.message[:50] + "..." if len(request.message) > 50 else request.message
    meta["last_activity"] = datetime.now().isoformat()

    return {"success": True}


@app.get("/api/lead/{conversation_id}")
async def get_lead(conversation_id: str):
    """Get lead information for a conversation."""
    global agent

    if agent is None:
        return {"error": "Agent not initialized"}

    summary = agent.get_lead_summary(conversation_id)
    if summary and summary.get("found"):
        return summary["lead"]
    return {"error": "Lead not found"}


@app.post("/api/lead/{conversation_id}/note")
async def add_lead_note(conversation_id: str, request: NoteRequest):
    """Add a note to a lead."""
    global agent

    if agent is None:
        return {"error": "Agent not initialized"}

    lead_id = agent._lead_mapping.get(conversation_id)
    if not lead_id:
        return {"error": "Lead not found"}

    result = agent.qualification_tools.add_lead_note(lead_id, request.note)
    return result


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
