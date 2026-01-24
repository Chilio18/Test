"""Appointment scheduling tools for the car sales agent."""

import uuid
from datetime import datetime, timedelta
from typing import Optional

from ..models.appointment import Appointment, AppointmentType, AppointmentStatus


class AppointmentTools:
    """Tools for scheduling and managing appointments."""

    def __init__(
        self,
        business_hours_start: str = "09:00",
        business_hours_end: str = "18:00",
        business_days: list[str] = None,
        slot_duration_minutes: int = 60
    ):
        self.business_hours_start = business_hours_start
        self.business_hours_end = business_hours_end
        self.business_days = business_days or [
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
        ]
        self.slot_duration_minutes = slot_duration_minutes
        self._appointments: dict[str, Appointment] = {}

    def get_tool_definitions(self) -> list[dict]:
        """Return tool definitions for the Anthropic API."""
        return [
            {
                "name": "get_available_slots",
                "description": "Get available appointment slots for a specific date or date range. Use this to show customers when they can come in.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "date": {
                            "type": "string",
                            "description": "Date to check (YYYY-MM-DD format). If not specified, shows next 7 days."
                        },
                        "appointment_type": {
                            "type": "string",
                            "enum": ["test_drive", "trade_in_valuation", "configuration_quote", "general_consultation"],
                            "description": "Type of appointment (affects duration)"
                        },
                        "days_ahead": {
                            "type": "integer",
                            "description": "Number of days to look ahead (default: 7, max: 30)"
                        }
                    },
                    "required": []
                }
            },
            {
                "name": "schedule_appointment",
                "description": "Schedule an appointment for a customer. Use this when the customer agrees to come in.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "lead_id": {
                            "type": "string",
                            "description": "The lead/customer ID"
                        },
                        "customer_name": {
                            "type": "string",
                            "description": "Customer's full name"
                        },
                        "customer_phone": {
                            "type": "string",
                            "description": "Customer's phone number"
                        },
                        "customer_email": {
                            "type": "string",
                            "description": "Customer's email address"
                        },
                        "appointment_type": {
                            "type": "string",
                            "enum": ["test_drive", "trade_in_valuation", "configuration_quote", "general_consultation"],
                            "description": "Type of appointment"
                        },
                        "date": {
                            "type": "string",
                            "description": "Appointment date (YYYY-MM-DD format)"
                        },
                        "time": {
                            "type": "string",
                            "description": "Appointment time (HH:MM format, 24-hour)"
                        },
                        "vehicle_id": {
                            "type": "string",
                            "description": "ID of the vehicle for test drive (if applicable)"
                        },
                        "vehicle_description": {
                            "type": "string",
                            "description": "Description of the vehicle (if no ID available)"
                        },
                        "has_trade_in": {
                            "type": "boolean",
                            "description": "Whether the customer has a trade-in"
                        },
                        "trade_in_details": {
                            "type": "string",
                            "description": "Details about the trade-in vehicle"
                        },
                        "notes": {
                            "type": "string",
                            "description": "Additional notes for the appointment"
                        }
                    },
                    "required": ["lead_id", "customer_name", "appointment_type", "date", "time"]
                }
            },
            {
                "name": "reschedule_appointment",
                "description": "Reschedule an existing appointment to a new date/time.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "appointment_id": {
                            "type": "string",
                            "description": "The appointment ID to reschedule"
                        },
                        "new_date": {
                            "type": "string",
                            "description": "New appointment date (YYYY-MM-DD format)"
                        },
                        "new_time": {
                            "type": "string",
                            "description": "New appointment time (HH:MM format, 24-hour)"
                        }
                    },
                    "required": ["appointment_id", "new_date", "new_time"]
                }
            },
            {
                "name": "cancel_appointment",
                "description": "Cancel an existing appointment.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "appointment_id": {
                            "type": "string",
                            "description": "The appointment ID to cancel"
                        },
                        "reason": {
                            "type": "string",
                            "description": "Reason for cancellation"
                        }
                    },
                    "required": ["appointment_id"]
                }
            },
            {
                "name": "get_appointment_details",
                "description": "Get details of a specific appointment.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "appointment_id": {
                            "type": "string",
                            "description": "The appointment ID"
                        }
                    },
                    "required": ["appointment_id"]
                }
            }
        ]

    def _parse_time(self, time_str: str) -> tuple[int, int]:
        """Parse time string to hours and minutes."""
        parts = time_str.split(":")
        return int(parts[0]), int(parts[1])

    def _is_business_day(self, date: datetime) -> bool:
        """Check if a date is a business day."""
        day_name = date.strftime("%A")
        return day_name in self.business_days

    def _get_slots_for_date(self, date: datetime, duration_minutes: int = 60) -> list[str]:
        """Get available time slots for a specific date."""
        if not self._is_business_day(date):
            return []

        start_h, start_m = self._parse_time(self.business_hours_start)
        end_h, end_m = self._parse_time(self.business_hours_end)

        slots = []
        current = datetime(date.year, date.month, date.day, start_h, start_m)
        end_time = datetime(date.year, date.month, date.day, end_h, end_m)

        # Don't show slots in the past
        now = datetime.now()
        if date.date() == now.date():
            # Round up to next slot
            current = max(current, now + timedelta(minutes=30))
            current = current.replace(minute=(current.minute // 30) * 30, second=0, microsecond=0)
            if current.minute % 30 != 0:
                current += timedelta(minutes=30 - (current.minute % 30))

        while current + timedelta(minutes=duration_minutes) <= end_time:
            slot_time = current.strftime("%H:%M")
            # Check if slot is already booked
            is_booked = False
            for appt in self._appointments.values():
                if appt.status in [AppointmentStatus.SCHEDULED, AppointmentStatus.CONFIRMED]:
                    appt_end = appt.scheduled_datetime + timedelta(minutes=appt.duration_minutes)
                    slot_end = current + timedelta(minutes=duration_minutes)
                    if (appt.scheduled_datetime < slot_end and appt_end > current):
                        is_booked = True
                        break

            if not is_booked:
                slots.append(slot_time)

            current += timedelta(minutes=30)  # 30-minute intervals

        return slots

    def get_available_slots(
        self,
        date: Optional[str] = None,
        appointment_type: Optional[str] = None,
        days_ahead: int = 7
    ) -> dict:
        """Get available appointment slots."""
        # Determine duration based on appointment type
        duration = self.slot_duration_minutes
        if appointment_type == "test_drive":
            duration = 60
        elif appointment_type == "trade_in_valuation":
            duration = 45
        elif appointment_type == "configuration_quote":
            duration = 90
        elif appointment_type == "general_consultation":
            duration = 30

        days_ahead = min(days_ahead, 30)
        results = {}

        if date:
            try:
                check_date = datetime.strptime(date, "%Y-%m-%d")
                slots = self._get_slots_for_date(check_date, duration)
                if slots:
                    results[date] = slots
            except ValueError:
                return {"error": "Invalid date format. Please use YYYY-MM-DD."}
        else:
            start_date = datetime.now()
            for i in range(days_ahead):
                check_date = start_date + timedelta(days=i)
                date_str = check_date.strftime("%Y-%m-%d")
                slots = self._get_slots_for_date(check_date, duration)
                if slots:
                    results[date_str] = slots

        if not results:
            return {
                "available": False,
                "message": "No available slots found for the requested period. Would you like to check different dates?"
            }

        # Format for friendly display
        formatted_slots = []
        for date_str, slots in list(results.items())[:5]:  # Limit to first 5 days
            date_obj = datetime.strptime(date_str, "%Y-%m-%d")
            formatted_slots.append({
                "date": date_str,
                "day": date_obj.strftime("%A, %B %d"),
                "slots": slots[:6]  # Limit slots shown
            })

        return {
            "available": True,
            "duration_minutes": duration,
            "slots": formatted_slots
        }

    def schedule_appointment(
        self,
        lead_id: str,
        customer_name: str,
        appointment_type: str,
        date: str,
        time: str,
        customer_phone: Optional[str] = None,
        customer_email: Optional[str] = None,
        vehicle_id: Optional[str] = None,
        vehicle_description: Optional[str] = None,
        has_trade_in: bool = False,
        trade_in_details: Optional[str] = None,
        notes: Optional[str] = None
    ) -> dict:
        """Schedule a new appointment."""
        try:
            scheduled_datetime = datetime.strptime(f"{date} {time}", "%Y-%m-%d %H:%M")
        except ValueError:
            return {"success": False, "error": "Invalid date or time format."}

        # Validate it's not in the past
        if scheduled_datetime < datetime.now():
            return {"success": False, "error": "Cannot schedule appointments in the past."}

        # Validate business hours
        if not self._is_business_day(scheduled_datetime):
            return {"success": False, "error": f"We're closed on {scheduled_datetime.strftime('%A')}s."}

        start_h, start_m = self._parse_time(self.business_hours_start)
        end_h, end_m = self._parse_time(self.business_hours_end)
        if (scheduled_datetime.hour < start_h or
            (scheduled_datetime.hour == start_h and scheduled_datetime.minute < start_m) or
            scheduled_datetime.hour >= end_h):
            return {
                "success": False,
                "error": f"Please choose a time between {self.business_hours_start} and {self.business_hours_end}."
            }

        # Create appointment
        appointment_id = str(uuid.uuid4())[:8]
        appt_type = AppointmentType(appointment_type)

        duration = self.slot_duration_minutes
        if appt_type == AppointmentType.TEST_DRIVE:
            duration = 60
        elif appt_type == AppointmentType.TRADE_IN_VALUATION:
            duration = 45
        elif appt_type == AppointmentType.CONFIGURATION_QUOTE:
            duration = 90

        appointment = Appointment(
            id=appointment_id,
            lead_id=lead_id,
            customer_name=customer_name,
            customer_phone=customer_phone,
            customer_email=customer_email,
            appointment_type=appt_type,
            scheduled_datetime=scheduled_datetime,
            duration_minutes=duration,
            vehicle_id=vehicle_id,
            vehicle_description=vehicle_description,
            has_trade_in=has_trade_in,
            trade_in_details=trade_in_details,
            notes=notes
        )

        self._appointments[appointment_id] = appointment

        return {
            "success": True,
            "appointment_id": appointment_id,
            "confirmation": {
                "type": appointment.appointment_type_display,
                "date_time": appointment.formatted_datetime,
                "duration": f"{duration} minutes",
                "customer_name": customer_name,
                "vehicle": vehicle_description or vehicle_id or "To be discussed"
            },
            "message": f"Your {appointment.appointment_type_display} appointment has been scheduled for {appointment.formatted_datetime}. We look forward to seeing you!"
        }

    def reschedule_appointment(
        self,
        appointment_id: str,
        new_date: str,
        new_time: str
    ) -> dict:
        """Reschedule an existing appointment."""
        if appointment_id not in self._appointments:
            return {"success": False, "error": "Appointment not found."}

        try:
            new_datetime = datetime.strptime(f"{new_date} {new_time}", "%Y-%m-%d %H:%M")
        except ValueError:
            return {"success": False, "error": "Invalid date or time format."}

        if new_datetime < datetime.now():
            return {"success": False, "error": "Cannot reschedule to a past time."}

        appointment = self._appointments[appointment_id]
        old_datetime = appointment.formatted_datetime
        appointment.scheduled_datetime = new_datetime
        appointment.status = AppointmentStatus.RESCHEDULED
        appointment.updated_at = datetime.now()

        return {
            "success": True,
            "appointment_id": appointment_id,
            "message": f"Your appointment has been rescheduled from {old_datetime} to {appointment.formatted_datetime}."
        }

    def cancel_appointment(self, appointment_id: str, reason: Optional[str] = None) -> dict:
        """Cancel an appointment."""
        if appointment_id not in self._appointments:
            return {"success": False, "error": "Appointment not found."}

        appointment = self._appointments[appointment_id]
        appointment.status = AppointmentStatus.CANCELLED
        appointment.updated_at = datetime.now()
        if reason:
            appointment.notes = f"{appointment.notes or ''}\nCancellation reason: {reason}".strip()

        return {
            "success": True,
            "message": "Your appointment has been cancelled. We hope to see you another time! Would you like to reschedule for a different day?"
        }

    def get_appointment_details(self, appointment_id: str) -> dict:
        """Get details of an appointment."""
        if appointment_id not in self._appointments:
            return {"found": False, "error": "Appointment not found."}

        appt = self._appointments[appointment_id]
        return {
            "found": True,
            "appointment": {
                "id": appt.id,
                "type": appt.appointment_type_display,
                "status": appt.status.value,
                "date_time": appt.formatted_datetime,
                "duration": f"{appt.duration_minutes} minutes",
                "customer_name": appt.customer_name,
                "vehicle": appt.vehicle_description or appt.vehicle_id,
                "has_trade_in": appt.has_trade_in,
                "trade_in_details": appt.trade_in_details,
                "notes": appt.notes
            }
        }

    def handle_tool_call(self, tool_name: str, tool_input: dict) -> dict:
        """Handle a tool call from the agent."""
        if tool_name == "get_available_slots":
            return self.get_available_slots(**tool_input)
        elif tool_name == "schedule_appointment":
            return self.schedule_appointment(**tool_input)
        elif tool_name == "reschedule_appointment":
            return self.reschedule_appointment(**tool_input)
        elif tool_name == "cancel_appointment":
            return self.cancel_appointment(**tool_input)
        elif tool_name == "get_appointment_details":
            return self.get_appointment_details(**tool_input)
        else:
            return {"error": f"Unknown tool: {tool_name}"}
