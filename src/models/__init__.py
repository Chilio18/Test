"""Data models for the car sales agent."""

from .lead import Lead, LeadStatus, LeadSource, QualificationScore
from .appointment import Appointment, AppointmentType, AppointmentStatus
from .vehicle import Vehicle, VehicleCondition

__all__ = [
    "Lead",
    "LeadStatus",
    "LeadSource",
    "QualificationScore",
    "Appointment",
    "AppointmentType",
    "AppointmentStatus",
    "Vehicle",
    "VehicleCondition",
]
