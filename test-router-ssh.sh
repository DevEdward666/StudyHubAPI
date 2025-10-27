#!/bin/bash

# PLDT HG8145V5 SSH Connectivity Test
# This script tests if SSH is available on your router

echo "╔═══════════════════════════════════════════════════════════╗"
echo "║  PLDT HG8145V5 Router - SSH Connectivity Test            ║"
echo "╚═══════════════════════════════════════════════════════════╝"
echo ""

# Router details from appsettings.json
ROUTER_IP="192.168.1.1"
ROUTER_PORT="22"
ROUTER_USER="adminpldt"
ROUTER_PASS="Eiijii@665306"

echo "Testing SSH connection to: $ROUTER_IP:$ROUTER_PORT"
echo "Username: $ROUTER_USER"
echo ""

# Test 1: Check if port 22 is open
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "Test 1: Checking if SSH port is open..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if command -v nc &> /dev/null; then
    # Using netcat with timeout
    if timeout 3 nc -zv $ROUTER_IP $ROUTER_PORT 2>&1 | grep -q "succeeded"; then
        echo "✅ Port $ROUTER_PORT is OPEN"
        PORT_OPEN=true
    else
        echo "❌ Port $ROUTER_PORT is CLOSED or FILTERED"
        PORT_OPEN=false
    fi
else
    echo "⚠️  netcat (nc) not available, skipping port check"
    PORT_OPEN="unknown"
fi

echo ""

# Test 2: Try to connect via SSH
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "Test 2: Attempting SSH connection..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if [ "$PORT_OPEN" = true ]; then
    echo "Attempting SSH connection (this may take a few seconds)..."
    echo ""
    
    # Try SSH with timeout and non-interactive mode
    # Just check if we can establish connection
    timeout 5 ssh -o ConnectTimeout=3 \
                   -o StrictHostKeyChecking=no \
                   -o UserKnownHostsFile=/dev/null \
                   -o BatchMode=yes \
                   $ROUTER_USER@$ROUTER_IP \
                   "echo 'SSH connection successful'" 2>&1
    
    SSH_EXIT=$?
    
    echo ""
    
    if [ $SSH_EXIT -eq 0 ]; then
        echo "✅ SSH CONNECTION SUCCESSFUL!"
        echo "   Your router DOES support SSH access"
        echo "   The MAC whitelist feature might work!"
    elif [ $SSH_EXIT -eq 255 ]; then
        echo "❌ SSH connection FAILED"
        echo "   Possible reasons:"
        echo "   - Wrong username/password"
        echo "   - SSH disabled by PLDT"
        echo "   - Firewall blocking connection"
    elif [ $SSH_EXIT -eq 124 ]; then
        echo "⏱️  SSH connection TIMEOUT"
        echo "   Router not responding to SSH"
    else
        echo "❌ SSH connection FAILED (Exit code: $SSH_EXIT)"
    fi
else
    echo "⏭️  Skipping SSH test (port not open)"
fi

echo ""

# Test 3: Check router web interface
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "Test 3: Checking web interface..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if command -v curl &> /dev/null; then
    HTTP_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 3 http://$ROUTER_IP 2>/dev/null)
    
    if [ "$HTTP_RESPONSE" = "200" ] || [ "$HTTP_RESPONSE" = "302" ] || [ "$HTTP_RESPONSE" = "401" ]; then
        echo "✅ Web interface is ACCESSIBLE at http://$ROUTER_IP"
        echo "   You can manage router via web browser"
    else
        echo "⚠️  Web interface status: HTTP $HTTP_RESPONSE"
    fi
else
    echo "⚠️  curl not available, skipping web test"
fi

echo ""

# Summary and recommendations
echo "╔═══════════════════════════════════════════════════════════╗"
echo "║  SUMMARY & RECOMMENDATIONS                                ║"
echo "╚═══════════════════════════════════════════════════════════╝"
echo ""

if [ "$PORT_OPEN" = true ] && [ $SSH_EXIT -eq 0 ]; then
    echo "🎉 GREAT NEWS!"
    echo "   Your PLDT HG8145V5 has SSH enabled!"
    echo ""
    echo "Next Steps:"
    echo "1. Upload whitelist scripts to router"
    echo "2. Test router whitelist feature"
    echo "3. Enjoy full automation!"
    echo ""
    echo "See: RouterScripts/README.md for script setup"
    
elif [ "$PORT_OPEN" = false ]; then
    echo "📋 AS EXPECTED"
    echo "   SSH is disabled on your PLDT HG8145V5"
    echo "   This is normal for PLDT routers"
    echo ""
    echo "Recommendations:"
    echo "✓ Use WiFi Portal WITHOUT router integration"
    echo "✓ Password generation works perfectly"
    echo "✓ Manual guest network management (Option 2)"
    echo "✓ Or add secondary router (Option 3)"
    echo ""
    echo "See: PLDT_HG8145V5_GUIDE.md for detailed options"
    
else
    echo "⚠️  UNABLE TO DETERMINE SSH STATUS"
    echo ""
    echo "Manual Check:"
    echo "1. Try: ssh $ROUTER_USER@$ROUTER_IP"
    echo "2. Check router web interface: http://$ROUTER_IP"
    echo "3. Look for SSH settings in router admin panel"
fi

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "Test Complete!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

