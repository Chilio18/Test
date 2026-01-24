#!/usr/bin/env python3
"""CLI tool for testing the Car Sales AI Agent interactively."""

import argparse
import os
import sys
from pathlib import Path

# Add project root to path
sys.path.insert(0, str(Path(__file__).parent))

from src.agent import CarSalesAgent


def main():
    """Run interactive CLI chat with the agent."""
    parser = argparse.ArgumentParser(description="Car Sales AI Agent CLI")
    parser.add_argument(
        "--channel",
        choices=["whatsapp", "email", "lead_system", "cli"],
        default="cli",
        help="Simulate a specific channel"
    )
    parser.add_argument(
        "--dealership",
        default="Premium Auto Sales",
        help="Dealership name"
    )
    parser.add_argument(
        "--inventory",
        default=None,
        help="Path to inventory JSON file"
    )
    parser.add_argument(
        "--language", "-l",
        choices=["en", "nl"],
        default="en",
        help="Language for the agent (en=English, nl=Nederlands/Dutch)"
    )
    args = parser.parse_args()

    # Check for API key
    if not os.environ.get("ANTHROPIC_API_KEY"):
        print("Error: ANTHROPIC_API_KEY environment variable not set")
        print("Please set it with: export ANTHROPIC_API_KEY=your_api_key")
        sys.exit(1)

    # Initialize agent
    inventory_path = args.inventory or str(Path(__file__).parent / "data" / "sample_inventory.json")

    # Use Dutch defaults if language is nl
    if args.language == "nl":
        dealership_config = {
            "name": args.dealership if args.dealership != "Premium Auto Sales" else "Premium Auto Verkoop",
            "address": "Autoweg 123, 1234 AB Amsterdam",
            "phone": "+31-20-555-1234",
            "email": "verkoop@premiumauto.nl",
            "website": "https://premiumauto.nl",
            "business_hours": "Maandag-Zaterdag, 9:00 - 18:00",
            "business_hours_start": "09:00",
            "business_hours_end": "18:00",
            "business_days": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]
        }
    else:
        dealership_config = {
            "name": args.dealership,
            "address": "123 Auto Drive, Car City, CC 12345",
            "phone": "+1-555-AUTO-SALE",
            "email": "sales@premiumauto.example.com",
            "website": "https://premiumauto.example.com",
            "business_hours": "Monday-Saturday, 9:00 AM - 6:00 PM",
            "business_hours_start": "09:00",
            "business_hours_end": "18:00",
            "business_days": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]
        }

    lang_name = "Nederlands" if args.language == "nl" else "English"
    print(f"\n{'='*60}")
    print(f"  Car Sales AI Agent - {args.dealership}")
    print(f"  Channel: {args.channel} | Language: {lang_name}")
    print(f"{'='*60}")
    print("\nType 'quit' or 'exit' to end the conversation")
    print("Type 'lead' to see the current lead summary")
    print("Type 'history' to see conversation history")
    print("-" * 60 + "\n")

    agent = CarSalesAgent(
        model="claude-sonnet-4-20250514",
        dealership_config=dealership_config,
        inventory_path=inventory_path,
        language=args.language
    )

    # Start a conversation
    conversation_id = "cli_test_001"
    agent.start_conversation(conversation_id, channel=args.channel)

    # Initial greeting based on channel
    if args.channel == "whatsapp":
        print("Customer connected via WhatsApp")
        print("(Simulating a WhatsApp conversation)\n")
    elif args.channel == "email":
        print("Customer inquiry received via Email")
        print("(Simulating an email thread)\n")
    elif args.channel == "lead_system":
        print("New lead received from Lead Management System")
        print("(Simulating lead system responses)\n")

    while True:
        try:
            # Get user input
            user_input = input("\nYou: ").strip()

            if not user_input:
                continue

            # Handle special commands
            if user_input.lower() in ["quit", "exit"]:
                print("\nThank you for using Car Sales AI Agent. Goodbye!")
                break

            if user_input.lower() == "lead":
                summary = agent.get_lead_summary(conversation_id)
                if summary and summary.get("found"):
                    lead = summary["lead"]
                    print("\n" + "=" * 40)
                    print("LEAD SUMMARY")
                    print("=" * 40)
                    print(f"Name: {lead['name']}")
                    print(f"Status: {lead['status']}")
                    print(f"Qualification: {lead['qualification']['level']} ({lead['qualification']['total_score']}/40)")
                    print(f"  Budget: {lead['qualification']['budget']['score']}/10 - {lead['qualification']['budget']['notes'] or 'N/A'}")
                    print(f"  Authority: {lead['qualification']['authority']['score']}/10 - {lead['qualification']['authority']['notes'] or 'N/A'}")
                    print(f"  Need: {lead['qualification']['need']['score']}/10 - {lead['qualification']['need']['notes'] or 'N/A'}")
                    print(f"  Timeline: {lead['qualification']['timeline']['score']}/10 - {lead['qualification']['timeline']['notes'] or 'N/A'}")
                    if lead['vehicle_interests']:
                        print(f"Interested Vehicles: {', '.join(lead['vehicle_interests'])}")
                    if lead['has_trade_in']:
                        print(f"Trade-in: Yes - {lead['trade_in']}")
                    if lead['notes']:
                        print("Recent Notes:")
                        for note in lead['notes'][-3:]:
                            print(f"  - {note}")
                    print("=" * 40)
                else:
                    print("\nNo lead information available yet.")
                continue

            if user_input.lower() == "history":
                history = agent.get_conversation_history(conversation_id)
                print("\n" + "=" * 40)
                print("CONVERSATION HISTORY")
                print("=" * 40)
                for msg in history:
                    role = msg["role"].upper()
                    content = msg["content"]
                    if isinstance(content, list):
                        # Handle tool use responses
                        for block in content:
                            if hasattr(block, "text"):
                                print(f"{role}: {block.text[:200]}...")
                            elif hasattr(block, "type") and block.type == "tool_use":
                                print(f"{role}: [Tool: {block.name}]")
                    else:
                        print(f"{role}: {content[:200]}..." if len(str(content)) > 200 else f"{role}: {content}")
                print("=" * 40)
                continue

            # Process the message
            print("\nAgent: ", end="", flush=True)
            response = agent.process_message(
                conversation_id,
                user_input,
                channel=args.channel
            )
            print(response)

        except KeyboardInterrupt:
            print("\n\nInterrupted. Goodbye!")
            break
        except Exception as e:
            print(f"\nError: {e}")
            print("Please try again or type 'quit' to exit.")


if __name__ == "__main__":
    main()
