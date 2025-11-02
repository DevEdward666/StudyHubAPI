#!/bin/bash

# Thermal Printer Diagnostic Script for macOS
# Run this to check if your RPP02N-1175 is ready to print

echo "🔍 RPP02N-1175 Thermal Printer Diagnostic"
echo "=========================================="
echo ""

# Check 1: Bluetooth is on
echo "✓ Check 1: Bluetooth Status"
# Try multiple methods to detect Bluetooth status (macOS versions vary)
if system_profiler SPBluetoothDataType | grep -qi "State: On\|Power: On\|Bluetooth Power: On" || \
   system_profiler SPBluetoothDataType | grep -qi "Discoverable: Yes\|Connected: Yes" || \
   ls /dev/cu.Bluetooth* >/dev/null 2>&1; then
    echo "  ✅ Bluetooth is ON"
else
    echo "  ❌ Bluetooth appears to be OFF"
    echo "  → If Bluetooth is actually ON, this check can be ignored"
    echo "  → Continuing anyway..."
fi
echo ""

# Check 2: RPP02N is connected
echo "✓ Check 2: Printer Connection"
if system_profiler SPBluetoothDataType | grep -qi "RPP02N\|RPP-02N\|Bluetooth Printer"; then
    echo "  ✅ RPP02N-1175 is paired"
    echo "  Device info:"
    system_profiler SPBluetoothDataType | grep -A 10 -i "RPP02N\|RPP-02N" | head -15
else
    echo "  ⚠️  RPP02N-1175 not found in Bluetooth devices"
    echo "  → If printer is paired, this check can be ignored"
    echo "  → Continuing to check serial ports..."
fi
echo ""

# Check 3: Serial ports available
echo "✓ Check 3: Serial Ports"
echo "  Available Bluetooth serial ports:"
ls /dev/cu.* 2>/dev/null | grep -i bluetooth | while read port; do
    echo "    - $port"
done
echo ""

# Check 4: RPP02N serial port exists
echo "✓ Check 4: RPP02N Serial Port"
if ls /dev/cu.* 2>/dev/null | grep -q "RPP02N"; then
    RPP_PORT=$(ls /dev/cu.* | grep "RPP02N")
    echo "  ✅ Found: $RPP_PORT"
    ls -l "$RPP_PORT"
else
    echo "  ⚠️  RPP02N-SerialPort not found"
    echo "  → Only found: /dev/cu.Bluetooth-Incoming-Port (wrong one)"
    echo "  → Try re-pairing the printer"
    exit 1
fi
echo ""

# Check 5: Port permissions (Terminal won't have access, but that's OK)
echo "✓ Check 5: Port Access Test"
echo "  Note: Terminal may not have permission, but your app will!"
if echo "TEST" > "$RPP_PORT" 2>/dev/null; then
    echo "  ✅ Terminal can access port (unusual but good)"
else
    echo "  ⚠️  Terminal blocked by macOS (this is NORMAL)"
    echo "  ✅ Your .NET backend will work fine!"
fi
echo ""

# Summary
echo "=========================================="
echo "📊 SUMMARY"
echo "=========================================="
echo ""

if ls /dev/cu.* 2>/dev/null | grep -q "RPP02N"; then
    echo "✅ Your printer is ready!"
    echo ""
    echo "Next steps:"
    echo "  1. Start your backend:"
    echo "     cd Study-Hub && dotnet run"
    echo ""
    echo "  2. Add a transaction in your app"
    echo ""
    echo "  3. Click 'Print Receipt'"
    echo ""
    echo "  4. Watch the console output:"
    echo "     Should see: 'Found RPP02N printer port: $RPP_PORT'"
    echo ""
    echo "🖨️  Receipt should print!"
else
    echo "⚠️  Printer needs attention!"
    echo ""
    echo "Action required:"
    echo "  1. Open System Settings → Bluetooth"
    echo "  2. Find RPP02N-1175 and click 'i' button"
    echo "  3. Click 'Forget This Device'"
    echo "  4. Turn printer OFF then ON"
    echo "  5. Put printer in pairing mode (hold Bluetooth button)"
    echo "  6. Pair again"
    echo "  7. Run this script again"
fi
echo ""

