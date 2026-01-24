"""Trade-in valuation tools for the car sales agent."""

from typing import Optional


class ValuationTools:
    """Tools for estimating trade-in vehicle values."""

    def __init__(self):
        # Base depreciation rates by year (approximate)
        self._depreciation_rates = {
            0: 0.90,   # Current year (10% off)
            1: 0.80,   # 1 year old (20% off)
            2: 0.70,   # 2 years old
            3: 0.60,   # 3 years old
            4: 0.52,   # 4 years old
            5: 0.45,   # 5 years old
            6: 0.40,
            7: 0.35,
            8: 0.30,
            9: 0.26,
            10: 0.22,
        }

        # Condition multipliers
        self._condition_multipliers = {
            "excellent": 1.10,
            "good": 1.00,
            "fair": 0.85,
            "poor": 0.65
        }

        # Mileage adjustment per 10k miles over/under average (12k/year)
        self._mileage_adjustment_per_10k = 0.02  # 2% per 10k miles

        # Sample MSRP data for common vehicles (in production, this would come from a database)
        self._base_msrp = {
            "toyota_camry": 28000,
            "toyota_corolla": 22000,
            "toyota_rav4": 32000,
            "toyota_highlander": 40000,
            "honda_civic": 24000,
            "honda_accord": 28000,
            "honda_cr-v": 32000,
            "honda_pilot": 40000,
            "ford_f-150": 38000,
            "ford_mustang": 32000,
            "ford_escape": 30000,
            "ford_explorer": 38000,
            "chevrolet_silverado": 38000,
            "chevrolet_equinox": 28000,
            "chevrolet_malibu": 26000,
            "bmw_3 series": 45000,
            "bmw_5 series": 58000,
            "bmw_x3": 48000,
            "bmw_x5": 65000,
            "mercedes-benz_c-class": 47000,
            "mercedes-benz_e-class": 58000,
            "audi_a4": 42000,
            "audi_q5": 48000,
            "lexus_es": 42000,
            "lexus_rx": 50000,
            "nissan_altima": 26000,
            "nissan_rogue": 30000,
            "hyundai_sonata": 26000,
            "hyundai_tucson": 28000,
            "kia_optima": 25000,
            "kia_sportage": 28000,
            "subaru_outback": 32000,
            "subaru_forester": 30000,
            "mazda_cx-5": 30000,
            "mazda_mazda3": 24000,
            "volkswagen_jetta": 22000,
            "volkswagen_tiguan": 28000,
            "jeep_grand cherokee": 42000,
            "jeep_wrangler": 35000,
            "tesla_model 3": 45000,
            "tesla_model y": 55000,
        }

    def get_tool_definitions(self) -> list[dict]:
        """Return tool definitions for the Anthropic API."""
        return [
            {
                "name": "estimate_trade_in_value",
                "description": "Get an estimated trade-in value for a customer's vehicle. This is a preliminary estimate - the final value will be determined by an in-person inspection.",
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "make": {
                            "type": "string",
                            "description": "Vehicle make (e.g., Toyota, Honda, BMW)"
                        },
                        "model": {
                            "type": "string",
                            "description": "Vehicle model (e.g., Camry, Civic, X5)"
                        },
                        "year": {
                            "type": "integer",
                            "description": "Vehicle year"
                        },
                        "mileage": {
                            "type": "integer",
                            "description": "Current mileage"
                        },
                        "condition": {
                            "type": "string",
                            "enum": ["excellent", "good", "fair", "poor"],
                            "description": "Overall condition: excellent (like new, no issues), good (minor wear, well maintained), fair (some issues, average wear), poor (significant issues, high wear)"
                        },
                        "accident_history": {
                            "type": "boolean",
                            "description": "Whether the vehicle has been in any accidents"
                        },
                        "additional_info": {
                            "type": "string",
                            "description": "Any additional information (trim level, options, known issues)"
                        }
                    },
                    "required": ["make", "model", "year", "mileage", "condition"]
                }
            },
            {
                "name": "explain_valuation_factors",
                "description": "Explain what factors affect trade-in value and how customers can maximize their value.",
                "input_schema": {
                    "type": "object",
                    "properties": {},
                    "required": []
                }
            }
        ]

    def _get_base_msrp(self, make: str, model: str) -> Optional[float]:
        """Get base MSRP for a vehicle."""
        key = f"{make.lower()}_{model.lower()}"
        return self._base_msrp.get(key)

    def estimate_trade_in_value(
        self,
        make: str,
        model: str,
        year: int,
        mileage: int,
        condition: str,
        accident_history: bool = False,
        additional_info: Optional[str] = None
    ) -> dict:
        """Estimate trade-in value for a vehicle."""
        from datetime import datetime
        current_year = datetime.now().year

        # Get base MSRP
        base_msrp = self._get_base_msrp(make, model)
        if not base_msrp:
            # Use generic estimate based on segment
            base_msrp = 30000  # Default assumption

        # Calculate age
        age = current_year - year
        if age < 0:
            return {
                "success": False,
                "error": "Vehicle year cannot be in the future."
            }

        # Get depreciation rate
        if age > 10:
            depreciation_rate = 0.15  # Very old vehicles retain minimal value
        else:
            depreciation_rate = self._depreciation_rates.get(age, 0.22)

        # Start with depreciated value
        estimated_value = base_msrp * depreciation_rate

        # Apply condition multiplier
        condition_multiplier = self._condition_multipliers.get(condition.lower(), 1.0)
        estimated_value *= condition_multiplier

        # Adjust for mileage
        expected_mileage = age * 12000
        mileage_difference = mileage - expected_mileage
        mileage_adjustment = (mileage_difference / 10000) * self._mileage_adjustment_per_10k
        estimated_value *= (1 - mileage_adjustment)

        # Accident history penalty
        if accident_history:
            estimated_value *= 0.85  # 15% reduction for accident history

        # Ensure minimum value
        estimated_value = max(estimated_value, 500)

        # Calculate range (actual value could vary +/- 15%)
        low_estimate = estimated_value * 0.85
        high_estimate = estimated_value * 1.15

        return {
            "success": True,
            "vehicle": f"{year} {make} {model}",
            "mileage": f"{mileage:,} miles",
            "condition": condition,
            "estimate": {
                "low": f"${low_estimate:,.0f}",
                "mid": f"${estimated_value:,.0f}",
                "high": f"${high_estimate:,.0f}"
            },
            "raw_values": {
                "low": low_estimate,
                "mid": estimated_value,
                "high": high_estimate
            },
            "factors_applied": {
                "base_depreciation": f"{(1-depreciation_rate)*100:.0f}% depreciation for {age} year old vehicle",
                "condition_adjustment": f"{condition} condition ({condition_multiplier:.0%} of base)",
                "mileage_note": f"{'Above' if mileage > expected_mileage else 'Below'} average mileage for age" if mileage != expected_mileage else "Average mileage for age",
                "accident_history": "15% reduction applied" if accident_history else "No accident history discount"
            },
            "disclaimer": "This is a preliminary estimate only. Final trade-in value will be determined after an in-person inspection by our certified appraisers. Factors like service history, title status, and current market conditions will affect the final offer.",
            "recommendation": "We'd love to give you an accurate appraisal! Would you like to schedule a time to bring your vehicle in for a professional evaluation?"
        }

    def explain_valuation_factors(self) -> dict:
        """Explain what factors affect trade-in value."""
        return {
            "factors": [
                {
                    "factor": "Age & Depreciation",
                    "description": "Vehicles typically lose 15-20% of their value in the first year and about 10-15% each subsequent year.",
                    "tip": "Trading in before major depreciation milestones (3 years, 5 years) can maximize value."
                },
                {
                    "factor": "Mileage",
                    "description": "Average is about 12,000 miles per year. Higher mileage typically decreases value.",
                    "tip": "If you're close to a major mileage milestone (50k, 75k, 100k), consider trading in before you reach it."
                },
                {
                    "factor": "Condition",
                    "description": "Exterior, interior, and mechanical condition all affect value. Dents, scratches, stains, and mechanical issues reduce value.",
                    "tip": "Basic detailing and minor repairs can improve your offer. Fix small issues like burned out lights or worn wiper blades."
                },
                {
                    "factor": "Service History",
                    "description": "A complete service record shows the vehicle has been well maintained.",
                    "tip": "Bring all service records to your appraisal appointment."
                },
                {
                    "factor": "Accident History",
                    "description": "Vehicles with accident history typically have 10-25% lower values depending on severity.",
                    "tip": "Be upfront about any accidents - we can see them on vehicle history reports."
                },
                {
                    "factor": "Market Demand",
                    "description": "Popular models and body styles (like SUVs and trucks) often retain value better.",
                    "tip": "Timing can matter - convertibles are worth more in spring, trucks before winter."
                },
                {
                    "factor": "Options & Features",
                    "description": "Popular options like leather seats, sunroof, and advanced safety features can add value.",
                    "tip": "Make sure to mention any premium packages or aftermarket additions."
                }
            ],
            "maximize_value_tips": [
                "Clean your vehicle inside and out before the appraisal",
                "Gather all service records and the second key fob if you have it",
                "Fix minor issues (lights, wipers, small dents) if cost-effective",
                "Remove personal items and aftermarket accessories you want to keep",
                "Be honest about the vehicle's condition and history"
            ]
        }

    def handle_tool_call(self, tool_name: str, tool_input: dict) -> dict:
        """Handle a tool call from the agent."""
        if tool_name == "estimate_trade_in_value":
            return self.estimate_trade_in_value(**tool_input)
        elif tool_name == "explain_valuation_factors":
            return self.explain_valuation_factors()
        else:
            return {"error": f"Unknown tool: {tool_name}"}
