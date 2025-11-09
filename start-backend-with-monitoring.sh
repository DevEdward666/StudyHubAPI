#!/bin/bash

echo "🚀 Starting Study Hub Backend with Cron Job Monitoring"
echo "=================================================="
echo ""

# Navigate to backend directory
cd /Users/edward/Documents/StudyHubAPI/Study-Hub || exit 1

echo "📍 Current directory: $(pwd)"
echo ""

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ ERROR: dotnet is not installed!"
    echo "Install it with: brew install dotnet-sdk"
    exit 1
fi

echo "✅ .NET SDK found: $(dotnet --version)"
echo ""

# Kill any existing backend processes
echo "🔍 Checking for existing backend processes..."
EXISTING_PID=$(lsof -ti:5212 2>/dev/null)
if [ -n "$EXISTING_PID" ]; then
    echo "⚠️  Found existing process on port 5212 (PID: $EXISTING_PID)"
    echo "   Killing it..."
    kill -9 $EXISTING_PID 2>/dev/null
    sleep 2
    echo "✅ Process killed"
else
    echo "✅ No existing processes found"
fi
echo ""

# Start the backend
echo "🚀 Starting backend server..."
echo "⏳ Please wait for startup messages..."
echo ""
echo "=================================================="
echo "BACKEND LOGS (Watch for 'SessionExpiryChecker'):"
echo "=================================================="
echo ""

# Run backend and highlight SessionExpiryChecker logs
dotnet run 2>&1 | while IFS= read -r line; do
    # Highlight SessionExpiryChecker logs in green
    if echo "$line" | grep -q "SessionExpiryChecker"; then
        echo -e "\033[1;32m🔔 CRON: $line\033[0m"
    # Highlight errors in red
    elif echo "$line" | grep -q -i "error\|fail\|exception"; then
        echo -e "\033[1;31m❌ ERROR: $line\033[0m"
    # Highlight important startup messages in yellow
    elif echo "$line" | grep -q "Application started\|Now listening"; then
        echo -e "\033[1;33m✅ $line\033[0m"
    # Normal logs
    else
        echo "$line"
    fi
done

