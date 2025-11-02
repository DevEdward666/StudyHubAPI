#!/bin/bash

echo "🔍 USB Thermal Printer Diagnostic"
echo "=================================="
echo ""

echo "1️⃣ Checking for serial ports..."
SERIAL_PORTS=$(ls /dev/cu.* 2>/dev/null | grep -v "Bluetooth-Incoming-Port")
if [ -n "$SERIAL_PORTS" ]; then
    echo "✅ Found serial ports:"
    echo "$SERIAL_PORTS" | while read port; do
        echo "   - $port"
    done
else
    echo "❌ No serial ports found (except Bluetooth-Incoming-Port)"
    echo "   → USB-to-Serial driver may be needed"
fi
echo ""

echo "2️⃣ Checking USB devices..."
USB_PRINTER=$(system_profiler SPUSBDataType 2>/dev/null | grep -B 5 -A 10 -i "printer\|rpp02n\|serial\|ch340\|ftdi\|cp210")
if [ -n "$USB_PRINTER" ]; then
    echo "✅ Found USB printer/serial device:"
    echo "$USB_PRINTER" | head -20
else
    echo "⚠️  No USB printer detected in system"
    echo "   → Check USB cable is connected"
    echo "   → Try different USB port"
fi
echo ""

echo "3️⃣ Checking macOS Printers..."
CUPS_PRINTERS=$(lpstat -p 2>/dev/null)
if [ -n "$CUPS_PRINTERS" ]; then
    echo "📝 Printers in Printers & Scanners:"
    echo "$CUPS_PRINTERS"
else
    echo "ℹ️  No printers in Printers & Scanners"
fi
echo ""

echo "4️⃣ Checking for common USB-to-Serial chips..."
echo "   Checking for CH340/CH341..."
if system_profiler SPUSBDataType 2>/dev/null | grep -qi "ch340\|ch341"; then
    echo "   ✅ CH340/CH341 detected"
    if ls /dev/cu.* 2>/dev/null | grep -qi "usbserial\|usbmodem"; then
        echo "   ✅ Driver appears to be working"
    else
        echo "   ⚠️  Driver may not be installed properly"
        echo "   → Download CH340 driver from manufacturer"
    fi
else
    echo "   ℹ️  CH340/CH341 not detected"
fi
echo ""

echo "   Checking for FTDI..."
if system_profiler SPUSBDataType 2>/dev/null | grep -qi "ftdi\|ft232"; then
    echo "   ✅ FTDI chip detected (usually works without driver)"
else
    echo "   ℹ️  FTDI not detected"
fi
echo ""

echo "   Checking for CP210x..."
if system_profiler SPUSBDataType 2>/dev/null | grep -qi "cp210\|silicon labs"; then
    echo "   ✅ CP210x detected"
    if ls /dev/cu.* 2>/dev/null | grep -qi "SLAB"; then
        echo "   ✅ Driver appears to be working"
    else
        echo "   ⚠️  Driver may not be installed properly"
        echo "   → Download CP210x driver from Silicon Labs"
    fi
else
    echo "   ℹ️  CP210x not detected"
fi
echo ""

echo "=================================="
echo "📊 SUMMARY"
echo "=================================="
echo ""

USABLE_PORTS=$(ls /dev/cu.* 2>/dev/null | grep -v "Bluetooth-Incoming-Port" | grep -E "(usbserial|usbmodem|SLAB|USB)")
if [ -n "$USABLE_PORTS" ]; then
    echo "✅ Your printer should work!"
    echo ""
    echo "Usable ports found:"
    echo "$USABLE_PORTS"
    echo ""
    echo "Next steps:"
    echo "1. Restart your backend:"
    echo "   cd Study-Hub && dotnet run"
    echo ""
    echo "2. Try printing a receipt"
    echo ""
    echo "3. Watch console for:"
    echo "   'Found USB printer port: /dev/cu.xxx'"
else
    echo "⚠️  No USB serial ports detected"
    echo ""
    echo "Possible issues:"
    echo "1. USB cable not connected"
    echo "2. Printer not powered on"
    echo "3. USB-to-Serial driver not installed"
    echo ""
    echo "Solutions:"
    echo "1. Check USB cable is firmly connected"
    echo "2. Try different USB port on Mac"
    echo "3. Restart printer"
    echo "4. Install USB-to-Serial driver (CH340, FTDI, or CP210x)"
    echo ""
    echo "Driver downloads:"
    echo "- CH340: Search 'CH340 macOS driver download'"
    echo "- FTDI: Usually built into macOS"
    echo "- CP210x: https://www.silabs.com/developers/usb-to-uart-bridge-vcp-drivers"
fi
echo ""

