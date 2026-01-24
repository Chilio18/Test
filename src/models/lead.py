"""Lead data model for tracking potential customers."""

from datetime import datetime
from enum import Enum
from typing import Optional
from pydantic import BaseModel, Field


class LeadStatus(str, Enum):
    NEW = "new"
    CONTACTED = "contacted"
    QUALIFIED = "qualified"
    APPOINTMENT_SCHEDULED = "appointment_scheduled"
    VISITED = "visited"
    CONVERTED = "converted"
    LOST = "lost"


class LeadSource(str, Enum):
    WHATSAPP = "whatsapp"
    EMAIL = "email"
    LEAD_SYSTEM = "lead_system"
    WEBSITE = "website"
    WALK_IN = "walk_in"
    PHONE = "phone"
    REFERRAL = "referral"


class QualificationScore(BaseModel):
    """BANT-based lead qualification scoring."""

    budget_score: int = Field(default=0, ge=0, le=10, description="Budget qualification (0-10)")
    authority_score: int = Field(default=0, ge=0, le=10, description="Decision-making authority (0-10)")
    need_score: int = Field(default=0, ge=0, le=10, description="Need/urgency level (0-10)")
    timeline_score: int = Field(default=0, ge=0, le=10, description="Purchase timeline (0-10)")

    budget_notes: Optional[str] = None
    authority_notes: Optional[str] = None
    need_notes: Optional[str] = None
    timeline_notes: Optional[str] = None

    @property
    def total_score(self) -> int:
        return self.budget_score + self.authority_score + self.need_score + self.timeline_score

    @property
    def qualification_level(self) -> str:
        total = self.total_score
        if total >= 32:
            return "hot"
        elif total >= 24:
            return "warm"
        elif total >= 16:
            return "cool"
        else:
            return "cold"


class Lead(BaseModel):
    """Represents a potential customer lead."""

    id: str
    created_at: datetime = Field(default_factory=datetime.now)
    updated_at: datetime = Field(default_factory=datetime.now)

    # Contact Information
    name: Optional[str] = None
    first_name: Optional[str] = None
    last_name: Optional[str] = None
    email: Optional[str] = None
    phone: Optional[str] = None
    preferred_contact_method: Optional[str] = None

    # Lead Details
    source: LeadSource = LeadSource.WEBSITE
    status: LeadStatus = LeadStatus.NEW

    # Vehicle Interest
    interested_vehicle_ids: list[str] = Field(default_factory=list)
    vehicle_preferences: Optional[dict] = None  # make, model, year range, budget, etc.

    # Trade-in Information
    has_trade_in: bool = False
    trade_in_vehicle: Optional[dict] = None  # make, model, year, mileage, condition
    trade_in_estimate: Optional[float] = None

    # Financing
    needs_financing: Optional[bool] = None
    pre_approved: Optional[bool] = None

    # Qualification
    qualification: QualificationScore = Field(default_factory=QualificationScore)

    # Notes and History
    notes: list[str] = Field(default_factory=list)
    conversation_summary: Optional[str] = None

    # Assignment
    assigned_salesperson: Optional[str] = None

    def add_note(self, note: str) -> None:
        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M")
        self.notes.append(f"[{timestamp}] {note}")
        self.updated_at = datetime.now()

    @property
    def display_name(self) -> str:
        if self.name:
            return self.name
        if self.first_name and self.last_name:
            return f"{self.first_name} {self.last_name}"
        if self.first_name:
            return self.first_name
        if self.email:
            return self.email.split("@")[0]
        if self.phone:
            return f"Customer {self.phone[-4:]}"
        return "Unknown Customer"
