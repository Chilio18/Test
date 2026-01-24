"""Vehicle inventory tools for the car sales agent."""

import json
from typing import Optional
from pathlib import Path

from ..models.vehicle import Vehicle, VehicleCondition


class InventoryTools:
    """Tools for searching and retrieving vehicle inventory."""

    def __init__(self, inventory_path: Optional[str] = None):
        self.inventory_path = inventory_path or str(
            Path(__file__).parent.parent.parent / "data" / "sample_inventory.json"
        )
        self._vehicles: list[Vehicle] = []
        self._load_inventory()

    def _load_inventory(self) -> None:
        """Load vehicle inventory from JSON file."""
        try:
            with open(self.inventory_path, "r") as f:
                data = json.load(f)
                self._vehicles = [Vehicle(**v) for v in data.get("vehicles", [])]
        except FileNotFoundError:
            self._vehicles = []

    def get_tool_definitions(self) -> list[dict]:
        """Return tool definitions for the Anthropic API."""
        return [
            {
                "name": "search_inventory",
                "description": "Search the vehicle inventory based on customer preferences. Use this to find vehicles that match what the customer is looking for.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "make": {
                            "type": "string",
                            "description": "Vehicle make/brand (e.g., Toyota, Honda, BMW)"
                        },
                        "model": {
                            "type": "string",
                            "description": "Vehicle model (e.g., Camry, Civic, X5)"
                        },
                        "min_year": {
                            "type": "integer",
                            "description": "Minimum year of the vehicle"
                        },
                        "max_year": {
                            "type": "integer",
                            "description": "Maximum year of the vehicle"
                        },
                        "min_price": {
                            "type": "number",
                            "description": "Minimum price in dollars"
                        },
                        "max_price": {
                            "type": "number",
                            "description": "Maximum price in dollars"
                        },
                        "condition": {
                            "type": "string",
                            "enum": ["new", "certified_pre_owned", "used", "demo"],
                            "description": "Vehicle condition"
                        },
                        "fuel_type": {
                            "type": "string",
                            "enum": ["gasoline", "diesel", "electric", "hybrid", "plug-in hybrid"],
                            "description": "Fuel type preference"
                        },
                        "body_type": {
                            "type": "string",
                            "description": "Body type (e.g., sedan, SUV, truck, coupe)"
                        },
                        "limit": {
                            "type": "integer",
                            "description": "Maximum number of results to return (default: 5)"
                        }
                    },
                    "required": []
                }
            },
            {
                "name": "get_vehicle_details",
                "description": "Get detailed information about a specific vehicle by its ID. Use this when the customer wants to know more about a particular vehicle.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "vehicle_id": {
                            "type": "string",
                            "description": "The unique identifier of the vehicle"
                        }
                    },
                    "required": ["vehicle_id"]
                }
            },
            {
                "name": "compare_vehicles",
                "description": "Compare two or more vehicles side by side. Use this when a customer is deciding between multiple options.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "vehicle_ids": {
                            "type": "array",
                            "items": {"type": "string"},
                            "description": "List of vehicle IDs to compare"
                        }
                    },
                    "required": ["vehicle_ids"]
                }
            },
            {
                "name": "check_vehicle_availability",
                "description": "Check if a specific vehicle is still available for purchase or test drive.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "vehicle_id": {
                            "type": "string",
                            "description": "The unique identifier of the vehicle"
                        }
                    },
                    "required": ["vehicle_id"]
                }
            }
        ]

    def search_inventory(
        self,
        make: Optional[str] = None,
        model: Optional[str] = None,
        min_year: Optional[int] = None,
        max_year: Optional[int] = None,
        min_price: Optional[float] = None,
        max_price: Optional[float] = None,
        condition: Optional[str] = None,
        fuel_type: Optional[str] = None,
        body_type: Optional[str] = None,
        limit: int = 5
    ) -> dict:
        """Search inventory based on criteria."""
        results = []

        for vehicle in self._vehicles:
            if not vehicle.available:
                continue

            # Apply filters
            if make and make.lower() not in vehicle.make.lower():
                continue
            if model and model.lower() not in vehicle.model.lower():
                continue
            if min_year and vehicle.year < min_year:
                continue
            if max_year and vehicle.year > max_year:
                continue
            if min_price and vehicle.sale_price < min_price:
                continue
            if max_price and vehicle.sale_price > max_price:
                continue
            if condition and vehicle.condition.value != condition:
                continue
            if fuel_type and vehicle.fuel_type and fuel_type.lower() not in vehicle.fuel_type.lower():
                continue

            results.append(vehicle)

        # Sort by year (newest first), then price
        results.sort(key=lambda v: (-v.year, v.sale_price))

        # Limit results
        results = results[:limit]

        if not results:
            return {
                "found": 0,
                "message": "No vehicles found matching your criteria. Would you like to broaden your search?",
                "vehicles": []
            }

        return {
            "found": len(results),
            "vehicles": [
                {
                    "id": v.id,
                    "name": v.full_name,
                    "condition": v.condition_display,
                    "price": v.price_display,
                    "price_value": v.sale_price,
                    "mileage": f"{v.mileage:,} miles" if v.mileage else "New",
                    "exterior_color": v.exterior_color,
                    "fuel_type": v.fuel_type,
                    "highlights": v.highlights[:3] if v.highlights else []
                }
                for v in results
            ]
        }

    def get_vehicle_details(self, vehicle_id: str) -> dict:
        """Get detailed information about a specific vehicle."""
        for vehicle in self._vehicles:
            if vehicle.id == vehicle_id:
                return {
                    "found": True,
                    "vehicle": {
                        "id": vehicle.id,
                        "name": vehicle.full_name,
                        "condition": vehicle.condition_display,
                        "price": vehicle.price_display,
                        "msrp": f"${vehicle.msrp:,.0f}" if vehicle.msrp else None,
                        "mileage": f"{vehicle.mileage:,} miles" if vehicle.mileage else "New",
                        "exterior_color": vehicle.exterior_color,
                        "interior_color": vehicle.interior_color,
                        "fuel_type": vehicle.fuel_type,
                        "transmission": vehicle.transmission,
                        "drivetrain": vehicle.drivetrain,
                        "engine": vehicle.engine,
                        "features": vehicle.features,
                        "packages": vehicle.packages,
                        "highlights": vehicle.highlights,
                        "description": vehicle.description,
                        "vin": vehicle.vin,
                        "stock_number": vehicle.stock_number,
                        "available": vehicle.available
                    }
                }

        return {
            "found": False,
            "message": f"Vehicle with ID {vehicle_id} not found in inventory."
        }

    def compare_vehicles(self, vehicle_ids: list[str]) -> dict:
        """Compare multiple vehicles."""
        vehicles = []
        for vid in vehicle_ids:
            for vehicle in self._vehicles:
                if vehicle.id == vid:
                    vehicles.append(vehicle)
                    break

        if len(vehicles) < 2:
            return {
                "success": False,
                "message": "Need at least 2 valid vehicles to compare."
            }

        comparison = {
            "success": True,
            "vehicles": [
                {
                    "id": v.id,
                    "name": v.full_name,
                    "condition": v.condition_display,
                    "price": v.price_display,
                    "price_value": v.sale_price,
                    "mileage": v.mileage,
                    "exterior_color": v.exterior_color,
                    "fuel_type": v.fuel_type,
                    "transmission": v.transmission,
                    "drivetrain": v.drivetrain,
                    "engine": v.engine,
                    "key_features": v.features[:5] if v.features else []
                }
                for v in vehicles
            ]
        }

        # Add price difference
        if len(vehicles) == 2:
            diff = abs(vehicles[0].sale_price - vehicles[1].sale_price)
            comparison["price_difference"] = f"${diff:,.0f}"

        return comparison

    def check_vehicle_availability(self, vehicle_id: str) -> dict:
        """Check if a vehicle is available."""
        for vehicle in self._vehicles:
            if vehicle.id == vehicle_id:
                return {
                    "found": True,
                    "vehicle_name": vehicle.full_name,
                    "available": vehicle.available,
                    "message": (
                        f"Great news! The {vehicle.full_name} is available for viewing and test drive."
                        if vehicle.available
                        else f"Unfortunately, the {vehicle.full_name} is no longer available. Would you like to see similar options?"
                    )
                }

        return {
            "found": False,
            "message": f"Vehicle with ID {vehicle_id} not found in inventory."
        }

    def handle_tool_call(self, tool_name: str, tool_input: dict) -> dict:
        """Handle a tool call from the agent."""
        if tool_name == "search_inventory":
            return self.search_inventory(**tool_input)
        elif tool_name == "get_vehicle_details":
            return self.get_vehicle_details(**tool_input)
        elif tool_name == "compare_vehicles":
            return self.compare_vehicles(**tool_input)
        elif tool_name == "check_vehicle_availability":
            return self.check_vehicle_availability(**tool_input)
        else:
            return {"error": f"Unknown tool: {tool_name}"}
