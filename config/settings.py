"""Settings and configuration management."""

import os
from functools import lru_cache
from typing import Optional
from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    """Application settings loaded from environment variables."""

    # Anthropic API
    anthropic_api_key: str = ""
    model_name: str = "claude-sonnet-4-20250514"

    # Dealership Information
    dealership_name: str = "Premium Auto Sales"
    dealership_address: str = "123 Auto Drive, Car City, CC 12345"
    dealership_phone: str = "+1-555-AUTO-SALE"
    dealership_email: str = "sales@premiumauto.example.com"
    dealership_website: str = "https://premiumauto.example.com"

    # Business Hours
    business_hours_start: str = "09:00"
    business_hours_end: str = "18:00"
    business_days: str = "Monday,Tuesday,Wednesday,Thursday,Friday,Saturday"

    # WhatsApp Configuration
    whatsapp_api_url: str = ""
    whatsapp_api_token: str = ""
    whatsapp_phone_number_id: str = ""
    whatsapp_verify_token: str = ""
    whatsapp_app_secret: str = ""

    # Email Configuration
    smtp_host: str = ""
    smtp_port: int = 587
    smtp_user: str = ""
    smtp_password: str = ""
    imap_host: str = ""
    imap_port: int = 993

    # Lead Management System
    lead_system_api_url: str = ""
    lead_system_api_key: str = ""
    lead_system_api_secret: str = ""

    # Server Configuration
    host: str = "0.0.0.0"
    port: int = 8000
    debug: bool = False

    # Inventory
    inventory_path: str = ""

    class Config:
        env_file = ".env"
        env_file_encoding = "utf-8"
        extra = "ignore"

    @property
    def business_days_list(self) -> list[str]:
        """Get business days as a list."""
        return [day.strip() for day in self.business_days.split(",")]

    @property
    def business_hours(self) -> str:
        """Get formatted business hours string."""
        days = self.business_days_list
        if len(days) > 2:
            days_str = f"{days[0]}-{days[-1]}"
        else:
            days_str = ", ".join(days)

        # Convert 24h to 12h format for display
        def to_12h(time_str: str) -> str:
            h, m = map(int, time_str.split(":"))
            suffix = "AM" if h < 12 else "PM"
            h = h % 12 or 12
            return f"{h}:{m:02d} {suffix}"

        return f"{days_str}, {to_12h(self.business_hours_start)} - {to_12h(self.business_hours_end)}"

    @property
    def dealership_config(self) -> dict:
        """Get dealership configuration as a dictionary."""
        return {
            "name": self.dealership_name,
            "address": self.dealership_address,
            "phone": self.dealership_phone,
            "email": self.dealership_email,
            "website": self.dealership_website,
            "business_hours": self.business_hours,
            "business_hours_start": self.business_hours_start,
            "business_hours_end": self.business_hours_end,
            "business_days": self.business_days_list,
        }


@lru_cache()
def get_settings() -> Settings:
    """Get cached settings instance."""
    return Settings()
