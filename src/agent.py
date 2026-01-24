"""Main Car Sales AI Agent using the Anthropic API."""

import json
from typing import Optional
import anthropic

from .system_prompt import get_system_prompt
from .tools import InventoryTools, AppointmentTools, QualificationTools, ValuationTools
from .models.lead import Lead


class CarSalesAgent:
    """AI Agent for handling car sales conversations."""

    def __init__(
        self,
        api_key: Optional[str] = None,
        model: str = "claude-sonnet-4-20250514",
        dealership_config: Optional[dict] = None,
        inventory_path: Optional[str] = None
    ):
        """Initialize the car sales agent.

        Args:
            api_key: Anthropic API key. If not provided, uses ANTHROPIC_API_KEY env var.
            model: Claude model to use.
            dealership_config: Dealership information for the system prompt.
            inventory_path: Path to the vehicle inventory JSON file.
        """
        self.client = anthropic.Anthropic(api_key=api_key)
        self.model = model
        self.dealership_config = dealership_config or {}

        # Initialize tools
        self.inventory_tools = InventoryTools(inventory_path)
        self.appointment_tools = AppointmentTools(
            business_hours_start=self.dealership_config.get("business_hours_start", "09:00"),
            business_hours_end=self.dealership_config.get("business_hours_end", "18:00"),
            business_days=self.dealership_config.get("business_days")
        )
        self.qualification_tools = QualificationTools()
        self.valuation_tools = ValuationTools()

        # Conversation state
        self._conversations: dict[str, list[dict]] = {}
        self._lead_mapping: dict[str, str] = {}  # conversation_id -> lead_id

    def _get_all_tools(self) -> list[dict]:
        """Get all tool definitions."""
        tools = []
        tools.extend(self.inventory_tools.get_tool_definitions())
        tools.extend(self.appointment_tools.get_tool_definitions())
        tools.extend(self.qualification_tools.get_tool_definitions())
        tools.extend(self.valuation_tools.get_tool_definitions())
        return tools

    def _handle_tool_call(self, tool_name: str, tool_input: dict) -> dict:
        """Route tool calls to the appropriate handler."""
        # Inventory tools
        inventory_tool_names = ["search_inventory", "get_vehicle_details", "compare_vehicles", "check_vehicle_availability"]
        if tool_name in inventory_tool_names:
            return self.inventory_tools.handle_tool_call(tool_name, tool_input)

        # Appointment tools
        appointment_tool_names = ["get_available_slots", "schedule_appointment", "reschedule_appointment", "cancel_appointment", "get_appointment_details"]
        if tool_name in appointment_tool_names:
            return self.appointment_tools.handle_tool_call(tool_name, tool_input)

        # Qualification tools
        qualification_tool_names = ["create_lead", "update_lead_contact", "update_lead_qualification", "update_lead_vehicle_interest", "update_lead_trade_in", "update_lead_status", "add_lead_note", "get_lead_summary"]
        if tool_name in qualification_tool_names:
            return self.qualification_tools.handle_tool_call(tool_name, tool_input)

        # Valuation tools
        valuation_tool_names = ["estimate_trade_in_value", "explain_valuation_factors"]
        if tool_name in valuation_tool_names:
            return self.valuation_tools.handle_tool_call(tool_name, tool_input)

        return {"error": f"Unknown tool: {tool_name}"}

    def _get_system_prompt(
        self,
        channel: str = "unknown",
        lead: Optional[Lead] = None
    ) -> str:
        """Generate the system prompt with current context."""
        return get_system_prompt(
            dealership_name=self.dealership_config.get("name", "Premium Auto Sales"),
            dealership_address=self.dealership_config.get("address", "123 Auto Drive, Car City, CC 12345"),
            dealership_phone=self.dealership_config.get("phone", "+1-555-AUTO-SALE"),
            dealership_email=self.dealership_config.get("email", "sales@premiumauto.example.com"),
            dealership_website=self.dealership_config.get("website", "https://premiumauto.example.com"),
            business_hours=self.dealership_config.get("business_hours", "Monday-Saturday, 9:00 AM - 6:00 PM"),
            channel=channel,
            customer_name=lead.display_name if lead else "Valued Customer",
            lead_status=lead.status.value if lead else "new",
            interaction_summary=lead.conversation_summary or "No previous interactions" if lead else "No previous interactions"
        )

    def start_conversation(
        self,
        conversation_id: str,
        channel: str = "unknown",
        customer_info: Optional[dict] = None
    ) -> str:
        """Start a new conversation and create a lead.

        Args:
            conversation_id: Unique identifier for this conversation.
            channel: The communication channel (whatsapp, email, lead_system).
            customer_info: Initial customer information if available.

        Returns:
            The lead ID for this conversation.
        """
        # Create a new lead
        result = self.qualification_tools.create_lead(
            source=channel,
            name=customer_info.get("name") if customer_info else None,
            email=customer_info.get("email") if customer_info else None,
            phone=customer_info.get("phone") if customer_info else None,
            initial_notes=customer_info.get("initial_inquiry") if customer_info else None
        )

        lead_id = result["lead_id"]
        self._conversations[conversation_id] = []
        self._lead_mapping[conversation_id] = lead_id

        return lead_id

    def process_message(
        self,
        conversation_id: str,
        user_message: str,
        channel: str = "unknown"
    ) -> str:
        """Process an incoming message and generate a response.

        Args:
            conversation_id: Unique identifier for this conversation.
            user_message: The message from the customer.
            channel: The communication channel.

        Returns:
            The agent's response message.
        """
        # Get or create conversation history
        if conversation_id not in self._conversations:
            self.start_conversation(conversation_id, channel)

        messages = self._conversations[conversation_id]

        # Add user message to history
        messages.append({
            "role": "user",
            "content": user_message
        })

        # Get lead info for context
        lead_id = self._lead_mapping.get(conversation_id)
        lead = self.qualification_tools.get_lead(lead_id) if lead_id else None

        # Generate system prompt with context
        system_prompt = self._get_system_prompt(channel, lead)

        # Make API call with tools
        response = self.client.messages.create(
            model=self.model,
            max_tokens=4096,
            system=system_prompt,
            tools=self._get_all_tools(),
            messages=messages
        )

        # Process response, handling any tool calls
        while response.stop_reason == "tool_use":
            # Extract tool calls
            tool_calls = [block for block in response.content if block.type == "tool_use"]

            # Add assistant's response with tool calls to history
            messages.append({
                "role": "assistant",
                "content": response.content
            })

            # Process each tool call and collect results
            tool_results = []
            for tool_call in tool_calls:
                result = self._handle_tool_call(tool_call.name, tool_call.input)

                # Track lead_id if a lead was created
                if tool_call.name == "create_lead" and result.get("success"):
                    self._lead_mapping[conversation_id] = result["lead_id"]

                tool_results.append({
                    "type": "tool_result",
                    "tool_use_id": tool_call.id,
                    "content": json.dumps(result)
                })

            # Add tool results to messages
            messages.append({
                "role": "user",
                "content": tool_results
            })

            # Continue the conversation
            lead = self.qualification_tools.get_lead(self._lead_mapping.get(conversation_id))
            system_prompt = self._get_system_prompt(channel, lead)

            response = self.client.messages.create(
                model=self.model,
                max_tokens=4096,
                system=system_prompt,
                tools=self._get_all_tools(),
                messages=messages
            )

        # Extract final text response
        assistant_message = ""
        for block in response.content:
            if hasattr(block, "text"):
                assistant_message += block.text

        # Add final response to history
        messages.append({
            "role": "assistant",
            "content": response.content
        })

        return assistant_message

    def get_conversation_history(self, conversation_id: str) -> list[dict]:
        """Get the conversation history for a conversation ID."""
        return self._conversations.get(conversation_id, [])

    def get_lead_for_conversation(self, conversation_id: str) -> Optional[Lead]:
        """Get the lead associated with a conversation."""
        lead_id = self._lead_mapping.get(conversation_id)
        if lead_id:
            return self.qualification_tools.get_lead(lead_id)
        return None

    def get_lead_summary(self, conversation_id: str) -> Optional[dict]:
        """Get a summary of the lead for a conversation."""
        lead_id = self._lead_mapping.get(conversation_id)
        if lead_id:
            return self.qualification_tools.get_lead_summary(lead_id)
        return None
