"""Lead qualification tools for the car sales agent."""

import uuid
from datetime import datetime
from typing import Optional

from ..models.lead import Lead, LeadStatus, LeadSource, QualificationScore


class QualificationTools:
    """Tools for managing and qualifying leads."""

    def __init__(self):
        self._leads: dict[str, Lead] = {}

    def get_tool_definitions(self) -> list[dict]:
        """Return tool definitions for the Anthropic API."""
        return [
            {
                "name": "create_lead",
                "description": "Create a new lead record when a potential customer first contacts. Use this at the start of a conversation with a new customer.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "source": {
                            "type": "string",
                            "enum": ["whatsapp", "email", "lead_system", "website", "phone", "referral"],
                            "description": "How the lead contacted us"
                        },
                        "name": {
                            "type": "string",
                            "description": "Customer's full name (if known)"
                        },
                        "email": {
                            "type": "string",
                            "description": "Customer's email address (if known)"
                        },
                        "phone": {
                            "type": "string",
                            "description": "Customer's phone number (if known)"
                        },
                        "initial_notes": {
                            "type": "string",
                            "description": "Initial notes about the customer's inquiry"
                        }
                    },
                    "required": ["source"]
                }
            },
            {
                "name": "update_lead_contact",
                "description": "Update a lead's contact information when you learn new details about them.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "lead_id": {
                            "type": "string",
                            "description": "The lead ID"
                        },
                        "name": {
                            "type": "string",
                            "description": "Customer's full name"
                        },
                        "email": {
                            "type": "string",
                            "description": "Customer's email address"
                        },
                        "phone": {
                            "type": "string",
                            "description": "Customer's phone number"
                        },
                        "preferred_contact_method": {
                            "type": "string",
                            "enum": ["phone", "email", "whatsapp", "sms"],
                            "description": "How they prefer to be contacted"
                        }
                    },
                    "required": ["lead_id"]
                }
            },
            {
                "name": "update_lead_qualification",
                "description": "Update the lead qualification scores based on BANT criteria. Use this as you learn more about the customer's situation.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "lead_id": {
                            "type": "string",
                            "description": "The lead ID"
                        },
                        "budget_score": {
                            "type": "integer",
                            "minimum": 0,
                            "maximum": 10,
                            "description": "Budget qualification (0-10). 10 = confirmed budget aligns with our inventory, 5 = unclear budget, 0 = budget clearly doesn't match"
                        },
                        "budget_notes": {
                            "type": "string",
                            "description": "Notes about budget (e.g., 'Looking for $30-40k range, may finance')"
                        },
                        "authority_score": {
                            "type": "integer",
                            "minimum": 0,
                            "maximum": 10,
                            "description": "Decision-making authority (0-10). 10 = sole decision maker, 5 = needs to consult others, 0 = not the decision maker"
                        },
                        "authority_notes": {
                            "type": "string",
                            "description": "Notes about authority (e.g., 'Buying with spouse, both need to agree')"
                        },
                        "need_score": {
                            "type": "integer",
                            "minimum": 0,
                            "maximum": 10,
                            "description": "Need/urgency level (0-10). 10 = urgent need (car totaled), 5 = considering upgrade, 0 = just browsing"
                        },
                        "need_notes": {
                            "type": "string",
                            "description": "Notes about need (e.g., 'Current lease ending in 2 months')"
                        },
                        "timeline_score": {
                            "type": "integer",
                            "minimum": 0,
                            "maximum": 10,
                            "description": "Purchase timeline (0-10). 10 = buying today/this week, 7 = this month, 5 = 1-3 months, 3 = 3-6 months, 0 = just researching"
                        },
                        "timeline_notes": {
                            "type": "string",
                            "description": "Notes about timeline (e.g., 'Wants to buy before end of month for tax reasons')"
                        }
                    },
                    "required": ["lead_id"]
                }
            },
            {
                "name": "update_lead_vehicle_interest",
                "description": "Update the lead's vehicle preferences and interests.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "lead_id": {
                            "type": "string",
                            "description": "The lead ID"
                        },
                        "interested_vehicle_ids": {
                            "type": "array",
                            "items": {"type": "string"},
                            "description": "IDs of specific vehicles they're interested in"
                        },
                        "preferences": {
                            "type": "object",
                            "properties": {
                                "makes": {"type": "array", "items": {"type": "string"}},
                                "body_types": {"type": "array", "items": {"type": "string"}},
                                "min_year": {"type": "integer"},
                                "max_year": {"type": "integer"},
                                "min_price": {"type": "number"},
                                "max_price": {"type": "number"},
                                "fuel_type": {"type": "string"},
                                "must_have_features": {"type": "array", "items": {"type": "string"}},
                                "other_requirements": {"type": "string"}
                            },
                            "description": "General vehicle preferences"
                        }
                    },
                    "required": ["lead_id"]
                }
            },
            {
                "name": "update_lead_trade_in",
                "description": "Record trade-in vehicle information.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "lead_id": {
                            "type": "string",
                            "description": "The lead ID"
                        },
                        "has_trade_in": {
                            "type": "boolean",
                            "description": "Whether they have a trade-in"
                        },
                        "make": {
                            "type": "string",
                            "description": "Trade-in vehicle make"
                        },
                        "model": {
                            "type": "string",
                            "description": "Trade-in vehicle model"
                        },
                        "year": {
                            "type": "integer",
                            "description": "Trade-in vehicle year"
                        },
                        "mileage": {
                            "type": "integer",
                            "description": "Trade-in vehicle mileage"
                        },
                        "condition": {
                            "type": "string",
                            "enum": ["excellent", "good", "fair", "poor"],
                            "description": "Overall condition"
                        },
                        "paid_off": {
                            "type": "boolean",
                            "description": "Whether the vehicle is paid off"
                        },
                        "amount_owed": {
                            "type": "number",
                            "description": "Amount still owed on the vehicle"
                        }
                    },
                    "required": ["lead_id", "has_trade_in"]
                }
            },
            {
                "name": "update_lead_status",
                "description": "Update the lead's status in the sales pipeline.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "lead_id": {
                            "type": "string",
                            "description": "The lead ID"
                        },
                        "status": {
                            "type": "string",
                            "enum": ["new", "contacted", "qualified", "appointment_scheduled", "visited", "converted", "lost"],
                            "description": "New status for the lead"
                        }
                    },
                    "required": ["lead_id", "status"]
                }
            },
            {
                "name": "add_lead_note",
                "description": "Add a note to the lead record. Use this to record important information from the conversation.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "lead_id": {
                            "type": "string",
                            "description": "The lead ID"
                        },
                        "note": {
                            "type": "string",
                            "description": "The note to add"
                        }
                    },
                    "required": ["lead_id", "note"]
                }
            },
            {
                "name": "get_lead_summary",
                "description": "Get a summary of the lead's current information and qualification status.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "lead_id": {
                            "type": "string",
                            "description": "The lead ID"
                        }
                    },
                    "required": ["lead_id"]
                }
            },
            {
                "name": "request_human_handoff",
                "description": "Request a human salesperson to take over the conversation. Use this when the customer explicitly asks to speak with a human, or when the conversation requires human expertise (price negotiations, complex financing, complaints, etc.).",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "lead_id": {
                            "type": "string",
                            "description": "The lead ID"
                        },
                        "reason": {
                            "type": "string",
                            "description": "Reason for requesting handoff (e.g., 'customer requested human', 'price negotiation', 'complex question')"
                        },
                        "summary": {
                            "type": "string",
                            "description": "Brief summary of the conversation so far for the salesperson"
                        },
                        "priority": {
                            "type": "string",
                            "enum": ["low", "normal", "high", "urgent"],
                            "description": "Priority level for the handoff request"
                        }
                    },
                    "required": ["lead_id", "reason"]
                }
            }
        ]

    def create_lead(
        self,
        source: str,
        name: Optional[str] = None,
        email: Optional[str] = None,
        phone: Optional[str] = None,
        initial_notes: Optional[str] = None
    ) -> dict:
        """Create a new lead record."""
        lead_id = str(uuid.uuid4())[:8]

        lead = Lead(
            id=lead_id,
            source=LeadSource(source),
            name=name,
            email=email,
            phone=phone
        )

        if initial_notes:
            lead.add_note(initial_notes)

        self._leads[lead_id] = lead

        return {
            "success": True,
            "lead_id": lead_id,
            "message": f"Lead record created for {lead.display_name}"
        }

    def update_lead_contact(
        self,
        lead_id: str,
        name: Optional[str] = None,
        email: Optional[str] = None,
        phone: Optional[str] = None,
        preferred_contact_method: Optional[str] = None
    ) -> dict:
        """Update lead contact information."""
        if lead_id not in self._leads:
            return {"success": False, "error": "Lead not found"}

        lead = self._leads[lead_id]

        if name:
            lead.name = name
        if email:
            lead.email = email
        if phone:
            lead.phone = phone
        if preferred_contact_method:
            lead.preferred_contact_method = preferred_contact_method

        lead.updated_at = datetime.now()

        return {
            "success": True,
            "message": f"Contact information updated for {lead.display_name}"
        }

    def update_lead_qualification(
        self,
        lead_id: str,
        budget_score: Optional[int] = None,
        budget_notes: Optional[str] = None,
        authority_score: Optional[int] = None,
        authority_notes: Optional[str] = None,
        need_score: Optional[int] = None,
        need_notes: Optional[str] = None,
        timeline_score: Optional[int] = None,
        timeline_notes: Optional[str] = None
    ) -> dict:
        """Update lead qualification scores."""
        if lead_id not in self._leads:
            return {"success": False, "error": "Lead not found"}

        lead = self._leads[lead_id]
        qual = lead.qualification

        if budget_score is not None:
            qual.budget_score = budget_score
        if budget_notes:
            qual.budget_notes = budget_notes
        if authority_score is not None:
            qual.authority_score = authority_score
        if authority_notes:
            qual.authority_notes = authority_notes
        if need_score is not None:
            qual.need_score = need_score
        if need_notes:
            qual.need_notes = need_notes
        if timeline_score is not None:
            qual.timeline_score = timeline_score
        if timeline_notes:
            qual.timeline_notes = timeline_notes

        lead.updated_at = datetime.now()

        # Auto-update status if qualified
        if qual.total_score >= 24 and lead.status == LeadStatus.CONTACTED:
            lead.status = LeadStatus.QUALIFIED

        return {
            "success": True,
            "qualification_level": qual.qualification_level,
            "total_score": qual.total_score,
            "message": f"Lead qualified as '{qual.qualification_level}' (score: {qual.total_score}/40)"
        }

    def update_lead_vehicle_interest(
        self,
        lead_id: str,
        interested_vehicle_ids: Optional[list[str]] = None,
        preferences: Optional[dict] = None
    ) -> dict:
        """Update lead's vehicle interests."""
        if lead_id not in self._leads:
            return {"success": False, "error": "Lead not found"}

        lead = self._leads[lead_id]

        if interested_vehicle_ids:
            lead.interested_vehicle_ids.extend(interested_vehicle_ids)
            lead.interested_vehicle_ids = list(set(lead.interested_vehicle_ids))  # Remove duplicates

        if preferences:
            if lead.vehicle_preferences:
                lead.vehicle_preferences.update(preferences)
            else:
                lead.vehicle_preferences = preferences

        lead.updated_at = datetime.now()

        return {
            "success": True,
            "message": "Vehicle interests updated"
        }

    def update_lead_trade_in(
        self,
        lead_id: str,
        has_trade_in: bool,
        make: Optional[str] = None,
        model: Optional[str] = None,
        year: Optional[int] = None,
        mileage: Optional[int] = None,
        condition: Optional[str] = None,
        paid_off: Optional[bool] = None,
        amount_owed: Optional[float] = None
    ) -> dict:
        """Update lead's trade-in information."""
        if lead_id not in self._leads:
            return {"success": False, "error": "Lead not found"}

        lead = self._leads[lead_id]
        lead.has_trade_in = has_trade_in

        if has_trade_in:
            lead.trade_in_vehicle = {
                "make": make,
                "model": model,
                "year": year,
                "mileage": mileage,
                "condition": condition,
                "paid_off": paid_off,
                "amount_owed": amount_owed
            }

        lead.updated_at = datetime.now()

        return {
            "success": True,
            "message": "Trade-in information updated"
        }

    def update_lead_status(self, lead_id: str, status: str) -> dict:
        """Update lead status."""
        if lead_id not in self._leads:
            return {"success": False, "error": "Lead not found"}

        lead = self._leads[lead_id]
        lead.status = LeadStatus(status)
        lead.updated_at = datetime.now()

        return {
            "success": True,
            "message": f"Lead status updated to '{status}'"
        }

    def add_lead_note(self, lead_id: str, note: str) -> dict:
        """Add a note to the lead."""
        if lead_id not in self._leads:
            return {"success": False, "error": "Lead not found"}

        lead = self._leads[lead_id]
        lead.add_note(note)

        return {
            "success": True,
            "message": "Note added to lead record"
        }

    def get_lead_summary(self, lead_id: str) -> dict:
        """Get a summary of the lead."""
        if lead_id not in self._leads:
            return {"found": False, "error": "Lead not found"}

        lead = self._leads[lead_id]
        qual = lead.qualification

        return {
            "found": True,
            "lead": {
                "id": lead.id,
                "name": lead.display_name,
                "email": lead.email,
                "phone": lead.phone,
                "source": lead.source.value,
                "status": lead.status.value,
                "created": lead.created_at.strftime("%Y-%m-%d %H:%M"),
                "qualification": {
                    "level": qual.qualification_level,
                    "total_score": qual.total_score,
                    "budget": {"score": qual.budget_score, "notes": qual.budget_notes},
                    "authority": {"score": qual.authority_score, "notes": qual.authority_notes},
                    "need": {"score": qual.need_score, "notes": qual.need_notes},
                    "timeline": {"score": qual.timeline_score, "notes": qual.timeline_notes}
                },
                "vehicle_interests": lead.interested_vehicle_ids,
                "preferences": lead.vehicle_preferences,
                "has_trade_in": lead.has_trade_in,
                "trade_in": lead.trade_in_vehicle,
                "notes": lead.notes[-5:] if lead.notes else []  # Last 5 notes
            }
        }

    def get_lead(self, lead_id: str) -> Optional[Lead]:
        """Get a lead by ID (internal use)."""
        return self._leads.get(lead_id)

    def request_human_handoff(
        self,
        lead_id: str,
        reason: str,
        summary: Optional[str] = None,
        priority: str = "normal"
    ) -> dict:
        """Request human handoff for the conversation."""
        if lead_id not in self._leads:
            return {"success": False, "error": "Lead not found"}

        lead = self._leads[lead_id]

        # Add note about handoff request
        handoff_note = f"HANDOFF REQUESTED [{priority.upper()}]: {reason}"
        if summary:
            handoff_note += f"\nSamenvatting: {summary}"
        lead.add_note(handoff_note)

        # Mark lead as needing human attention
        lead.needs_human_attention = True
        lead.handoff_reason = reason
        lead.handoff_priority = priority
        lead.updated_at = datetime.now()

        return {
            "success": True,
            "handoff_requested": True,
            "priority": priority,
            "message": "Een medewerker is gevraagd om het gesprek over te nemen. De klant wordt zo snel mogelijk geholpen."
        }

    def handle_tool_call(self, tool_name: str, tool_input: dict) -> dict:
        """Handle a tool call from the agent."""
        handlers = {
            "create_lead": self.create_lead,
            "update_lead_contact": self.update_lead_contact,
            "update_lead_qualification": self.update_lead_qualification,
            "update_lead_vehicle_interest": self.update_lead_vehicle_interest,
            "update_lead_trade_in": self.update_lead_trade_in,
            "update_lead_status": self.update_lead_status,
            "add_lead_note": self.add_lead_note,
            "get_lead_summary": self.get_lead_summary,
            "request_human_handoff": self.request_human_handoff
        }

        if tool_name in handlers:
            return handlers[tool_name](**tool_input)
        return {"error": f"Unknown tool: {tool_name}"}
