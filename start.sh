#!/bin/bash
# Wittebrug Car Sales AI Agent - Startup Script

echo "=============================================="
echo "  Wittebrug Car Sales AI Agent"
echo "=============================================="
echo ""

# Check for API key
if [ -z "$ANTHROPIC_API_KEY" ]; then
    echo "ERROR: ANTHROPIC_API_KEY is not set!"
    echo ""
    echo "Please set your API key first:"
    echo "  export ANTHROPIC_API_KEY=sk-ant-..."
    echo ""
    echo "Get your API key at: https://console.anthropic.com/"
    exit 1
fi

echo "Starting server..."
echo ""
echo "Open in your browser:"
echo "  Customer chat:  http://localhost:8000/chat"
echo "  Dashboard:      http://localhost:8000/dashboard"
echo ""
echo "Press Ctrl+C to stop"
echo "=============================================="
echo ""

python main.py
