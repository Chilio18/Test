"""Appointment data model for scheduling showroom visits."""

from datetime import datetime
from enum import Enum
from typing import Optional
from pydantic import BaseModel, Field


class AppointmentType(str, Enum):
    TEST_DRIVE = "test_drive"
    TRADE_IN_VALUATION = "trade_in_valuation"
    CONFIGURATION_QUOTE = "configuration_quote"
    GENERAL_CONSULTATION = "general_consultation"
    DELIVERY = "delivery"
    SERVICE = "service"


class AppointmentStatus(str, Enum):
    SCHEDULED = "scheduled"
    CONFIRMED = "confirmed"
    REMINDED = "reminded"
    COMPLETED = "completed"
    NO_SHOW = "no_show"
    CANCELLED = "cancelled"
    RESCHEDULED = "rescheduled"


class Appointment(BaseModel):
    """Represents a scheduled appointment at the dealership."""

    id: str
    created_at: datetime = Field(default_factory=datetime.now)
    updated_at: datetime = Field(default_factory=datetime.now)

    # Lead/Customer Reference
    lead_id: str
    customer_name: str
    customer_phone: Optional[str] = None
    customer_email: Optional[str] = None

    # Appointment Details
    appointment_type: AppointmentType
    status: AppointmentStatus = AppointmentStatus.SCHEDULED

    # Scheduling
    scheduled_datetime: datetime
    duration_minutes: int = 60

    # Vehicle Information (if applicable)
    vehicle_id: Optional[str] = None
    vehicle_description: Optional[str] = None

    # Trade-in (if applicable)
    has_trade_in: bool = False
    trade_in_details: Optional[str] = None

    # Staff Assignment
    assigned_salesperson: Optional[str] = None

    # Notes
    notes: Optional[str] = None
    special_requests: Optional[str] = None

    # Reminders
    reminder_sent: bool = False
    confirmation_sent: bool = False

    @property
    def appointment_type_display(self) -> str:
        type_displays = {
            AppointmentType.TEST_DRIVE: "Test Drive",
            AppointmentType.TRADE_IN_VALUATION: "Trade-in Valuation",
            AppointmentType.CONFIGURATION_QUOTE: "Configuration & Quote",
            AppointmentType.GENERAL_CONSULTATION: "General Consultation",
            AppointmentType.DELIVERY: "Vehicle Delivery",
            AppointmentType.SERVICE: "Service Appointment",
        }
        return type_displays.get(self.appointment_type, self.appointment_type.value)

    @property
    def formatted_datetime(self) -> str:
        return self.scheduled_datetime.strftime("%A, %B %d, %Y at %I:%M %p")
