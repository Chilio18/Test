"""Vehicle data model for inventory management."""

from datetime import datetime
from enum import Enum
from typing import Optional
from pydantic import BaseModel, Field


class VehicleCondition(str, Enum):
    NEW = "new"
    CERTIFIED_PRE_OWNED = "certified_pre_owned"
    USED = "used"
    DEMO = "demo"


class Vehicle(BaseModel):
    """Represents a vehicle in inventory."""

    id: str

    # Basic Information
    make: str
    model: str
    year: int
    trim: Optional[str] = None

    # Condition and Status
    condition: VehicleCondition = VehicleCondition.NEW
    available: bool = True

    # Pricing
    msrp: Optional[float] = None
    sale_price: float
    discount: Optional[float] = None

    # Vehicle Details
    vin: Optional[str] = None
    stock_number: Optional[str] = None

    # Specifications
    exterior_color: Optional[str] = None
    interior_color: Optional[str] = None
    mileage: int = 0
    fuel_type: Optional[str] = None  # gasoline, diesel, electric, hybrid, plug-in hybrid
    transmission: Optional[str] = None  # automatic, manual, CVT
    drivetrain: Optional[str] = None  # FWD, RWD, AWD, 4WD
    engine: Optional[str] = None

    # Features
    features: list[str] = Field(default_factory=list)
    packages: list[str] = Field(default_factory=list)

    # Media
    images: list[str] = Field(default_factory=list)
    video_url: Optional[str] = None

    # Additional Info
    description: Optional[str] = None
    highlights: list[str] = Field(default_factory=list)

    # Timestamps
    added_date: datetime = Field(default_factory=datetime.now)

    @property
    def full_name(self) -> str:
        parts = [str(self.year), self.make, self.model]
        if self.trim:
            parts.append(self.trim)
        return " ".join(parts)

    @property
    def price_display(self) -> str:
        return f"${self.sale_price:,.0f}"

    @property
    def condition_display(self) -> str:
        condition_displays = {
            VehicleCondition.NEW: "New",
            VehicleCondition.CERTIFIED_PRE_OWNED: "Certified Pre-Owned",
            VehicleCondition.USED: "Pre-Owned",
            VehicleCondition.DEMO: "Demo",
        }
        return condition_displays.get(self.condition, self.condition.value)

    def matches_preferences(self, preferences: dict) -> bool:
        """Check if vehicle matches customer preferences."""
        if preferences.get("make") and self.make.lower() != preferences["make"].lower():
            return False
        if preferences.get("model") and self.model.lower() != preferences["model"].lower():
            return False
        if preferences.get("min_year") and self.year < preferences["min_year"]:
            return False
        if preferences.get("max_year") and self.year > preferences["max_year"]:
            return False
        if preferences.get("max_price") and self.sale_price > preferences["max_price"]:
            return False
        if preferences.get("min_price") and self.sale_price < preferences["min_price"]:
            return False
        if preferences.get("condition") and self.condition.value != preferences["condition"]:
            return False
        if preferences.get("fuel_type") and self.fuel_type and self.fuel_type.lower() != preferences["fuel_type"].lower():
            return False
        return True
