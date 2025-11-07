#!/bin/bash

# Test script for Session Expiry Notification System
# This script helps test the automatic session expiry feature

echo "🧪 Session Expiry Notification System Test"
echo "=========================================="
echo ""

# Configuration
API_URL="${API_URL:-https://3qrbqpcx-5212.asse.devtunnels.ms/api}"
ADMIN_TOKEN="${ADMIN_TOKEN}"

if [ -z "$ADMIN_TOKEN" ]; then
    echo "❌ Error: ADMIN_TOKEN environment variable is required"
    echo "Usage: ADMIN_TOKEN='your-jwt-token' ./test-session-expiry.sh"
    exit 1
fi

echo "📋 Test Steps:"
echo "1. Create a test table session"
echo "2. Manually expire the session (set EndTime to past)"
echo "3. Wait for background service (max 5 minutes)"
echo "4. Check if session was auto-completed"
echo ""

# Function to make API calls
api_call() {
    local method=$1
    local endpoint=$2
    local data=$3
    
    if [ -n "$data" ]; then
        curl -s -X "$method" \
            -H "Authorization: Bearer $ADMIN_TOKEN" \
            -H "Content-Type: application/json" \
            -d "$data" \
            "$API_URL/$endpoint"
    else
        curl -s -X "$method" \
            -H "Authorization: Bearer $ADMIN_TOKEN" \
            "$API_URL/$endpoint"
    fi
}

# Step 1: Get available tables
echo "📊 Fetching available tables..."
TABLES=$(api_call GET "admin/tables")
echo "$TABLES" | jq '.' 2>/dev/null || echo "$TABLES"
echo ""

# Step 2: Get active sessions
echo "📊 Fetching active sessions..."
SESSIONS=$(api_call GET "admin/transactions/pending")
echo "$SESSIONS" | jq '.' 2>/dev/null || echo "$SESSIONS"
echo ""

# Step 3: Instructions for manual testing
echo "🔧 Manual Test Steps:"
echo ""
echo "1. Start a table session via the app or API"
echo ""
echo "2. Run this SQL to manually expire a session:"
echo "   UPDATE table_sessions"
echo "   SET end_time = NOW() - INTERVAL '1 minute'"
echo "   WHERE status = 'active'"
echo "   LIMIT 1;"
echo ""
echo "3. Wait up to 5 minutes for the background service to run"
echo ""
echo "4. Check the backend logs for:"
echo "   'SessionExpiryChecker started'"
echo "   'Found X expired sessions to process'"
echo "   'Session XXX expired for table Y'"
echo ""
echo "5. In the admin panel, you should see:"
echo "   - Toast notification appears"
echo "   - Sound alert plays"
echo "   - Session marked as completed"
echo "   - Table marked as available"
echo ""

# Step 4: Check SignalR endpoint
echo "🔌 Testing SignalR Hub endpoint..."
SIGNALR_STATUS=$(curl -s -w "\n%{http_code}" "$API_URL/../hubs/notifications" | tail -1)
if [ "$SIGNALR_STATUS" = "200" ] || [ "$SIGNALR_STATUS" = "404" ]; then
    echo "✅ SignalR endpoint is accessible"
else
    echo "⚠️  SignalR endpoint returned status: $SIGNALR_STATUS"
fi
echo ""

# Step 5: Database query helper
echo "📝 Database Query Helper:"
echo ""
echo "To check expired sessions in your PostgreSQL database:"
echo ""
echo "SELECT id, table_id, user_id, status, start_time, end_time, amount"
echo "FROM table_sessions"
echo "WHERE status = 'active' AND end_time < NOW();"
echo ""
echo "To check notifications:"
echo ""
echo "SELECT id, user_id, title, message, created_at, is_read"
echo "FROM notifications"
echo "ORDER BY created_at DESC"
echo "LIMIT 10;"
echo ""

echo "✅ Test setup complete!"
echo ""
echo "💡 Tips:"
echo "- Open browser console in admin panel to see SignalR logs"
echo "- Backend should log: 'SessionExpiryChecker started. Checking every 5 minutes.'"
echo "- Front-end should log: 'SignalR connected successfully'"
echo "- Toast notifications appear at the top of the screen with a beep sound"
echo ""

