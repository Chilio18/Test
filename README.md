# Car Sales AI Agent

An AI-powered assistant that helps car salespeople respond to customer inquiries across WhatsApp, Email, and Lead Management Systems. The agent qualifies leads using the BANT framework and schedules appointments for test drives, trade-in valuations, and vehicle configurations.

## Features

- **Multi-Channel Support**: WhatsApp Business API, Email (IMAP/SMTP), Lead Management Systems
- **Lead Qualification**: Automatic BANT scoring (Budget, Authority, Need, Timeline)
- **Appointment Scheduling**: Book test drives, trade-in valuations, and consultations
- **Vehicle Inventory Search**: Find and compare vehicles based on customer preferences
- **Trade-in Estimation**: Provide preliminary trade-in value estimates
- **Conversation Memory**: Maintains context across interactions
- **Configurable**: Customize dealership info, business hours, and more

## Quick Start

### 1. Installation

```bash
# Clone the repository
git clone <repository-url>
cd car-sales-agent

# Create virtual environment
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt
```

### 2. Configuration

Copy the example environment file and configure it:

```bash
cp .env.example .env
```

Edit `.env` with your settings:

```env
# Required
ANTHROPIC_API_KEY=your_api_key_here

# Dealership Info
DEALERSHIP_NAME=Your Dealership Name
DEALERSHIP_ADDRESS=123 Main St, City, State 12345
DEALERSHIP_PHONE=+1-555-555-5555

# Optional: WhatsApp, Email, Lead System credentials
```

### 3. Run the Agent

**Interactive CLI (for testing):**

```bash
python cli.py
```

**API Server:**

```bash
python main.py
```

The server will start at `http://localhost:8000`

## Architecture

```
car-sales-agent/
├── src/
│   ├── agent.py              # Main agent with Claude API integration
│   ├── system_prompt.py      # Agent personality and instructions
│   ├── tools/
│   │   ├── inventory.py      # Vehicle search and details
│   │   ├── appointments.py   # Appointment scheduling
│   │   ├── qualification.py  # Lead management and BANT scoring
│   │   └── valuation.py      # Trade-in value estimation
│   ├── channels/
│   │   ├── whatsapp.py       # WhatsApp Business API
│   │   ├── email.py          # IMAP/SMTP email handling
│   │   └── lead_system.py    # CRM/Lead system integration
│   └── models/
│       ├── lead.py           # Lead data model
│       ├── appointment.py    # Appointment data model
│       └── vehicle.py        # Vehicle data model
├── config/
│   └── settings.py           # Configuration management
├── data/
│   └── sample_inventory.json # Example vehicle inventory
├── main.py                   # FastAPI server entry point
├── cli.py                    # Interactive testing CLI
└── requirements.txt
```

## Agent Tools

The agent has access to the following tools:

### Inventory Tools
- `search_inventory` - Find vehicles by make, model, price, features
- `get_vehicle_details` - Get full details on a specific vehicle
- `compare_vehicles` - Side-by-side vehicle comparison
- `check_vehicle_availability` - Verify a vehicle is still available

### Appointment Tools
- `get_available_slots` - Check available appointment times
- `schedule_appointment` - Book an appointment
- `reschedule_appointment` - Change appointment time
- `cancel_appointment` - Cancel an appointment
- `get_appointment_details` - View appointment information

### Lead Qualification Tools
- `create_lead` - Create a new lead record
- `update_lead_contact` - Update contact information
- `update_lead_qualification` - Update BANT scores
- `update_lead_vehicle_interest` - Track vehicle preferences
- `update_lead_trade_in` - Record trade-in information
- `update_lead_status` - Update pipeline status
- `add_lead_note` - Add conversation notes
- `get_lead_summary` - View lead overview

### Valuation Tools
- `estimate_trade_in_value` - Get preliminary trade-in estimate
- `explain_valuation_factors` - Explain what affects value

## API Endpoints

### Chat Endpoint
```
POST /api/chat
Content-Type: application/json

{
  "conversation_id": "unique_id",
  "message": "I'm looking for an SUV under $40,000",
  "channel": "api"
}
```

### Lead Information
```
GET /api/lead/{conversation_id}
```

### Health Check
```
GET /health
```

### WhatsApp Webhook
```
GET/POST /whatsapp/webhook
```

### Lead System Webhook
```
POST /leads/webhook/lead
```

## Example Conversation

```
Customer: Hi, I'm interested in getting a new SUV. My budget is around $35-40k.

Agent: Welcome to Premium Auto Sales! I'd be happy to help you find the perfect SUV.
       With your budget of $35-40k, we have some excellent options. Let me search our
       inventory for you...

       I found 3 SUVs that might interest you:

       1. 2024 Honda CR-V EX-L ($37,200) - AWD, Honda Sensing suite included
       2. 2024 Hyundai Tucson Limited Hybrid ($38,900) - Great fuel economy, AWD
       3. 2023 Toyota RAV4 XLE Premium CPO ($31,900) - Certified Pre-Owned with warranty

       Would you like more details on any of these? I'd also love to know:
       - What features are most important to you?
       - When are you looking to make a purchase?

Customer: The Honda sounds interesting. I also have a 2019 Camry I might trade in.

Agent: Great choice! The 2024 Honda CR-V EX-L is very popular.

       Regarding your 2019 Toyota Camry trade-in, I can give you a preliminary estimate.
       Could you tell me:
       - Approximately how many miles are on it?
       - What's the overall condition (excellent, good, fair)?

       For the CR-V, I see we have it in Crystal Black Pearl with Gray Leather interior.
       Key features include:
       - AWD standard
       - Full Honda Sensing safety suite
       - Wireless Apple CarPlay
       - Panoramic roof
       - Power tailgate

       Would you like to schedule a test drive? We have availability this week.
```

## Lead Qualification (BANT)

The agent automatically qualifies leads based on:

| Criteria | Score Range | Description |
|----------|-------------|-------------|
| Budget | 0-10 | Does their budget match our inventory? |
| Authority | 0-10 | Are they the decision maker? |
| Need | 0-10 | How urgent is their need? |
| Timeline | 0-10 | When are they planning to buy? |

**Qualification Levels:**
- **Hot** (32-40): Ready to buy, schedule immediately
- **Warm** (24-31): Interested, nurture actively
- **Cool** (16-23): Early stage, provide information
- **Cold** (0-15): Just browsing, low priority

## Channel Integration

### WhatsApp Business API

Configure in `.env`:
```env
WHATSAPP_API_URL=https://graph.facebook.com/v17.0
WHATSAPP_API_TOKEN=your_access_token
WHATSAPP_PHONE_NUMBER_ID=your_phone_number_id
WHATSAPP_VERIFY_TOKEN=your_verify_token
WHATSAPP_APP_SECRET=your_app_secret
```

### Email (IMAP/SMTP)

Configure in `.env`:
```env
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USER=your_email@gmail.com
SMTP_PASSWORD=your_app_password
IMAP_HOST=imap.gmail.com
IMAP_PORT=993
```

### Lead Management Systems

The agent supports common CRM systems. Configure the API endpoint:
```env
LEAD_SYSTEM_API_URL=https://your-crm.com/api
LEAD_SYSTEM_API_KEY=your_api_key
```

## Customization

### Adding Vehicles

Edit `data/sample_inventory.json` or connect to your dealership's DMS:

```json
{
  "vehicles": [
    {
      "id": "VH001",
      "make": "Toyota",
      "model": "Camry",
      "year": 2024,
      "sale_price": 31500,
      "condition": "new",
      ...
    }
  ]
}
```

### Customizing the Agent Personality

Edit `src/system_prompt.py` to adjust:
- Tone and communication style
- Qualification questions
- Sales approach
- Dealership-specific policies

## Development

### Running Tests

```bash
pytest tests/
```

### Code Formatting

```bash
black src/ tests/
ruff check src/ tests/
```

## License

MIT License - See LICENSE file for details.

## Support

For issues and feature requests, please open a GitHub issue.
