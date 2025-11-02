#!/bin/bash

echo "🔍 Quick Printer Check"
echo "====================="
echo ""

echo "1️⃣ Checking for RPP02N Serial Port..."
if ls /dev/cu.RPP02N-1175 2>/dev/null; then
    echo "   ✅ FOUND: /dev/cu.RPP02N-1175"
    echo "   ✅ Your printer is ready to use!"
    echo ""
    echo "   Port details:"
    ls -l /dev/cu.RPP02N-1175
    echo ""
    echo "✅ ALL GOOD! Printer is ready!"
    echo ""
    echo "Next: Start your backend and try printing:"
    echo "  cd Study-Hub && dotnet run"
    exit 0
else
    echo "   ❌ NOT FOUND: /dev/cu.RPP02N-1175"
    echo ""
fi

echo "2️⃣ Checking for any Bluetooth serial ports..."
BLUETOOTH_PORTS=$(ls /dev/cu.* 2>/dev/null | grep -i bluetooth)
if [ -n "$BLUETOOTH_PORTS" ]; then
    echo "   Found these Bluetooth ports:"
    echo "$BLUETOOTH_PORTS" | while read port; do
        echo "     - $port"
    done
    echo ""
    if echo "$BLUETOOTH_PORTS" | grep -q "Incoming-Port"; then
        echo "   ⚠️  Only found Incoming-Port (wrong direction)"
        echo "   ⚠️  Need: /dev/cu.RPP02N-1175"
        echo ""
        echo "   ACTION REQUIRED:"
        echo "   1. Open System Settings → Bluetooth"
        echo "   2. Find RPP02N-1175"
        echo "   3. Click 'i' button → 'Forget This Device'"
        echo "   4. Turn printer OFF then ON"
        echo "   5. Hold Bluetooth button until LED blinks rapidly"
        echo "   6. Pair again in Bluetooth settings"
        echo "   7. Run this script again"
    fi
else
    echo "   ❌ No Bluetooth ports found at all"
    echo ""
    echo "   ACTION REQUIRED:"
    echo "   1. Check Bluetooth is ON in System Settings"
    echo "   2. Pair RPP02N-1175 via Bluetooth settings"
    echo "   3. Run this script again"
fi
echo ""

echo "3️⃣ All available serial ports:"
ls /dev/cu.* 2>/dev/null || echo "   No serial ports found"
echo ""

exit 1

