"""Demo server that works without an API key - uses mock responses."""

import random
from datetime import datetime
from pathlib import Path

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.staticfiles import StaticFiles
from fastapi.responses import HTMLResponse
from pydantic import BaseModel
import uvicorn


# Demo responses in Dutch
DEMO_RESPONSES = [
    "Welkom bij Wittebrug! Waarmee kan ik u helpen vandaag? Bent u op zoek naar een nieuwe of gebruikte auto?",
    "Wat fijn dat u interesse heeft! We hebben een breed aanbod aan {brand}. Heeft u al een specifiek model in gedachten?",
    "Uitstekend! Met een budget van rond de €{budget} hebben we verschillende mooie opties. Wilt u een SUV, sedan, of een ander type?",
    "Ik kan een proefrit voor u inplannen! We hebben deze week nog beschikbaarheid op donderdag en vrijdag. Welke dag past u het beste?",
    "Voor een inruiltaxatie kunnen we uw auto vrijblijvend bekijken. Kunt u mij vertellen welk merk, model en bouwjaar uw huidige auto is?",
    "Uw {trade_in} is een populair model. We kunnen een goede indicatie geven van de inruilwaarde wanneer u langskomt. Zullen we een afspraak maken?",
    "Ik heb uw afspraak genoteerd voor {day} om {time}. U kunt ons vinden op Donau 120 in Den Haag. Tot dan!",
    "Natuurlijk! Bij Wittebrug bieden we ook financieringsmogelijkheden aan. Onze financieel adviseur kan u daar alles over vertellen tijdens uw bezoek.",
]

# State
conversations = {}
conversation_metadata = {}


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
    if conversation_id not in conversation_metadata:
        conversation_metadata[conversation_id] = {
            "human_mode": False,
            "last_activity": datetime.now().isoformat(),
            "customer_name": f"Demo Klant {conversation_id[-4:]}",
            "unread": False,
            "messages": [],
            "qualification_level": random.choice(["hot", "warm", "cool", "cold"])
        }
    return conversation_metadata[conversation_id]


def get_demo_response(message: str) -> str:
    """Generate a contextual demo response."""
    message_lower = message.lower()

    if any(word in message_lower for word in ["hallo", "hi", "hey", "goedemorgen", "goedemiddag"]):
        return "Welkom bij Wittebrug! 👋 Ik ben uw digitale assistent. Waarmee kan ik u vandaag helpen? Bent u op zoek naar een nieuwe auto, of heeft u vragen over onze diensten?"

    if any(word in message_lower for word in ["suv", "sedan", "hatchback", "auto"]):
        return "Uitstekend! We hebben een prachtig aanbod aan voertuigen. Bij Wittebrug vertegenwoordigen we merken zoals Volkswagen, Audi, SEAT, Škoda, Hyundai en nog veel meer. Heeft u een voorkeur voor een bepaald merk of type?"

    if any(word in message_lower for word in ["budget", "prijs", "euro", "€", "kost"]):
        return "Dat is een mooi budget om mee te werken! In die prijsklasse hebben we verschillende aantrekkelijke opties. Wilt u een nieuw of een gebruikt voertuig? En heeft u voorkeur voor benzine, diesel, of bent u geïnteresseerd in elektrisch rijden?"

    if any(word in message_lower for word in ["proefrit", "test", "rijden", "proberen"]):
        return "Natuurlijk kunnen we een proefrit voor u regelen! 🚗 We hebben deze week nog mogelijkheden op:\n\n• Donderdag 10:00 of 14:00\n• Vrijdag 11:00 of 15:00\n• Zaterdag 10:00 of 13:00\n\nWelk moment past u het beste?"

    if any(word in message_lower for word in ["inruil", "huidige auto", "trade", "ruilen"]):
        return "Een inruil is zeker mogelijk! 🔄 Om een goede indicatie te kunnen geven, heb ik wat informatie nodig:\n\n• Merk en model\n• Bouwjaar\n• Kilometerstand\n• Algemene staat\n\nKunt u mij deze gegevens geven?"

    if any(word in message_lower for word in ["afspraak", "langskomen", "bezoek", "wanneer"]):
        return "Uitstekend! Ik plan graag een afspraak voor u in bij onze vestiging in Den Haag (Donau 120). Wanneer zou het u uitkomen? We zijn geopend van maandag tot en met zaterdag, 9:00 - 18:00."

    if any(word in message_lower for word in ["financier", "lenen", "maandelijks", "betalen"]):
        return "Bij Wittebrug bieden we diverse financieringsmogelijkheden aan, waaronder private lease en traditionele financiering. Onze financieel adviseurs kunnen u een op maat gemaakt voorstel doen. Zullen we dit bespreken tijdens een afspraak?"

    if any(word in message_lower for word in ["volkswagen", "vw", "audi", "seat", "skoda", "hyundai"]):
        brand = "Volkswagen" if "vw" in message_lower or "volkswagen" in message_lower else message_lower.split()[0].title()
        return f"Wittebrug is een trotse {brand}-dealer! We hebben een uitgebreid aanbod aan zowel nieuwe als gebruikte {brand} modellen. Zijn er specifieke modellen waar u naar kijkt?"

    if any(word in message_lower for word in ["elektrisch", "ev", "hybrid", "elektrische"]):
        return "Elektrisch rijden is de toekomst! ⚡ We hebben diverse elektrische en hybride modellen, waaronder de populaire Volkswagen ID-serie en Hyundai IONIQ. Wilt u hier meer over weten of een proefrit maken?"

    if any(word in message_lower for word in ["dank", "bedankt", "thanks"]):
        return "Graag gedaan! Als u nog vragen heeft, sta ik altijd voor u klaar. Wilt u nu al een afspraak inplannen, of heeft u eerst nog tijd nodig om na te denken?"

    # Default response
    return random.choice([
        "Interessant! Kunt u mij daar wat meer over vertellen? Dan kan ik u beter helpen.",
        "Begrijp ik het goed dat u geïnteresseerd bent in een nieuwe auto? We hebben bij Wittebrug een breed aanbod voor elke wens en budget.",
        "Ik help u graag verder! Heeft u specifieke wensen qua merk, type of budget waar ik rekening mee moet houden?",
    ])


# Create FastAPI app
app = FastAPI(title="Wittebrug Demo (No API Key Required)")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.mount("/static", StaticFiles(directory=Path(__file__).parent / "static"), name="static")


@app.get("/", response_class=HTMLResponse)
async def root():
    return '<meta http-equiv="refresh" content="0; url=/chat" />'


@app.get("/chat", response_class=HTMLResponse)
async def chat_page():
    template_path = Path(__file__).parent / "templates" / "chat_widget.html"
    return HTMLResponse(content=template_path.read_text())


@app.get("/dashboard", response_class=HTMLResponse)
async def dashboard_page():
    template_path = Path(__file__).parent / "templates" / "dashboard.html"
    return HTMLResponse(content=template_path.read_text())


@app.get("/health")
async def health():
    return {"status": "healthy", "mode": "demo"}


@app.post("/api/chat")
async def chat(request: ChatRequest):
    meta = get_conversation_metadata(request.conversation_id)

    if meta["human_mode"]:
        meta["messages"].append({
            "role": "user",
            "content": request.message,
            "timestamp": datetime.now().isoformat()
        })
        meta["unread"] = True
        return {"response": None, "human_mode": True}

    # Store user message
    meta["messages"].append({
        "role": "user",
        "content": request.message,
        "timestamp": datetime.now().isoformat()
    })

    # Generate demo response
    response = get_demo_response(request.message)

    meta["messages"].append({
        "role": "assistant",
        "content": response,
        "timestamp": datetime.now().isoformat(),
        "human_response": False
    })
    meta["last_message"] = request.message[:50]
    meta["last_activity"] = datetime.now().isoformat()

    return {"response": response, "conversation_id": request.conversation_id, "human_mode": False}


@app.get("/api/conversation/{conversation_id}/status")
async def get_status(conversation_id: str):
    meta = get_conversation_metadata(conversation_id)
    return {"human_mode": meta.get("human_mode", False)}


@app.get("/api/conversations")
async def list_conversations():
    convs = []
    for conv_id, meta in conversation_metadata.items():
        convs.append({
            "id": conv_id,
            "customer_name": meta.get("customer_name", f"Klant {conv_id[-4:]}"),
            "last_message": meta.get("last_message", ""),
            "last_activity": meta.get("last_activity"),
            "human_mode": meta.get("human_mode", False),
            "unread": meta.get("unread", False),
            "qualification_level": meta.get("qualification_level", "cold")
        })
    return {"conversations": convs}


@app.get("/api/conversation/{conversation_id}")
async def get_conversation(conversation_id: str):
    meta = get_conversation_metadata(conversation_id)
    meta["unread"] = False
    return {
        "id": conversation_id,
        "customer_name": meta.get("customer_name"),
        "human_mode": meta.get("human_mode", False),
        "messages": meta.get("messages", [])
    }


@app.post("/api/conversation/{conversation_id}/takeover")
async def takeover(conversation_id: str):
    meta = get_conversation_metadata(conversation_id)
    meta["human_mode"] = True
    return {"success": True, "human_mode": True}


@app.post("/api/conversation/{conversation_id}/handback")
async def handback(conversation_id: str):
    meta = get_conversation_metadata(conversation_id)
    meta["human_mode"] = False
    return {"success": True, "human_mode": False}


@app.post("/api/conversation/{conversation_id}/message")
async def send_message(conversation_id: str, request: MessageRequest):
    meta = get_conversation_metadata(conversation_id)
    meta["messages"].append({
        "role": "assistant",
        "content": request.message,
        "timestamp": datetime.now().isoformat(),
        "human_response": True
    })
    meta["last_message"] = request.message[:50]
    meta["last_activity"] = datetime.now().isoformat()
    return {"success": True}


@app.get("/api/lead/{conversation_id}")
async def get_lead(conversation_id: str):
    scores = {
        "hot": {"budget": 9, "authority": 8, "need": 9, "timeline": 8},
        "warm": {"budget": 7, "authority": 6, "need": 7, "timeline": 5},
        "cool": {"budget": 4, "authority": 5, "need": 6, "timeline": 3},
        "cold": {"budget": 2, "authority": 3, "need": 4, "timeline": 2},
    }
    meta = get_conversation_metadata(conversation_id)
    level = meta.get("qualification_level", "cold")
    s = scores[level]

    return {
        "name": meta.get("customer_name"),
        "source": "website",
        "phone": "+31 6 12345678",
        "email": "demo@example.com",
        "qualification": {
            "level": level,
            "total_score": s["budget"] + s["authority"] + s["need"] + s["timeline"],
            "budget": {"score": s["budget"]},
            "authority": {"score": s["authority"]},
            "need": {"score": s["need"]},
            "timeline": {"score": s["timeline"]}
        },
        "vehicle_interests": ["Volkswagen Tiguan", "Audi Q3"],
        "has_trade_in": True,
        "notes": ["[2024-01-24 10:30] Klant zoekt SUV voor gezin", "[2024-01-24 10:35] Budget rond €40.000"]
    }


@app.post("/api/lead/{conversation_id}/note")
async def add_note(conversation_id: str, request: NoteRequest):
    return {"success": True}


if __name__ == "__main__":
    print("\n" + "=" * 50)
    print("  WITTEBRUG DEMO MODE")
    print("  (Geen API key nodig)")
    print("=" * 50)
    print("\nOpen in je browser:")
    print("  Klant chat:    http://localhost:8000/chat")
    print("  Dashboard:     http://localhost:8000/dashboard")
    print("\nDruk Ctrl+C om te stoppen")
    print("=" * 50 + "\n")

    uvicorn.run(app, host="0.0.0.0", port=8000)
