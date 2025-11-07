#!/bin/bash

# SignalR Live Server Test Script
# Tests SignalR connection on production server

echo "🔍 Testing SignalR on Live Server..."
echo ""

LIVE_URL="https://studyhubapi-i0o7.onrender.com"
SIGNALR_ENDPOINT="${LIVE_URL}/hubs/notifications"

echo "1️⃣ Checking if backend is reachable..."
HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "${LIVE_URL}/api/health" || echo "000")

if [ "$HTTP_STATUS" = "200" ] || [ "$HTTP_STATUS" = "404" ]; then
    echo "✅ Backend server is reachable (HTTP $HTTP_STATUS)"
else
    echo "❌ Backend server is not reachable (HTTP $HTTP_STATUS)"
    echo "   Make sure the server is deployed and running on Render.com"
    exit 1
fi

echo ""
echo "2️⃣ Checking SignalR negotiate endpoint..."
NEGOTIATE_URL="${SIGNALR_ENDPOINT}/negotiate"
NEGOTIATE_RESPONSE=$(curl -s -X POST "${NEGOTIATE_URL}" || echo "FAILED")

if [[ "$NEGOTIATE_RESPONSE" == *"connectionId"* ]] || [[ "$NEGOTIATE_RESPONSE" == *"error"* ]]; then
    echo "✅ SignalR negotiate endpoint is responding"
    echo "   Response: ${NEGOTIATE_RESPONSE:0:100}..."
else
    echo "⚠️  SignalR negotiate endpoint response:"
    echo "   $NEGOTIATE_RESPONSE"
fi

echo ""
echo "3️⃣ Checking WebSocket support..."
echo "   Testing: wss://studyhubapi-i0o7.onrender.com/hubs/notifications"
echo "   (WebSocket test requires browser - check browser console)"

echo ""
echo "📋 Frontend Environment Configuration:"
echo "   Add this to your Vercel environment variables:"
echo "   VITE_API_URL=https://studyhubapi-i0o7.onrender.com/api"

echo ""
echo "🧪 Testing Instructions:"
echo ""
echo "   1. Open browser console on your deployed frontend"
echo "   2. Look for these messages:"
echo "      ✓ 'SignalR connected successfully'"
echo "      ✓ 'Joined admins group'"
echo ""
echo "   3. Test session end notification:"
echo "      - Create a table session with 1-minute duration"
echo "      - Wait for expiry"
echo "      - Check if notification appears"
echo ""
echo "   4. If connection fails, check Render.com logs:"
echo "      - Go to https://dashboard.render.com"
echo "      - Select your service"
echo "      - View logs for SignalR errors"

echo ""
echo "🔧 Troubleshooting:"
echo ""
echo "   If SignalR doesn't connect:"
echo "   • Verify CORS includes your frontend URL"
echo "   • Check Render.com logs for errors"
echo "   • Ensure WebSocket middleware is enabled"
echo "   • Verify the service isn't sleeping (free tier)"
echo ""
echo "   Browser Console Commands:"
echo "   > signalRService.isConnected()"
echo "   > signalRService.getConnectionState()"
echo ""

echo "✅ Test script completed!"

