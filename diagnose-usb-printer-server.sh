#!/bin/bash

# USB Printer Diagnostic Script for Server Deployment
# Run this script on your server to diagnose printing issues

echo "╔════════════════════════════════════════════════════════╗"
echo "║   USB Printer Diagnostic Tool - StudyHub Server       ║"
echo "╚════════════════════════════════════════════════════════╝"
echo ""

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print status
print_status() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✅ $2${NC}"
    else
        echo -e "${RED}❌ $2${NC}"
    fi
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_info() {
    echo -e "ℹ️  $1"
}

# 1. Check Operating System
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "1. System Information"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
print_info "OS: $(uname -s)"
print_info "Kernel: $(uname -r)"
print_info "User: $(whoami)"
print_info "Groups: $(groups)"
echo ""

# 2. Check for USB Devices
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "2. USB Device Detection"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if [ -d "/dev/" ]; then
    USB_DEVICES=$(ls /dev/cu.* 2>/dev/null | grep -i usb)
    TTY_DEVICES=$(ls /dev/tty.* 2>/dev/null | grep -i usb)
    
    if [ -n "$USB_DEVICES" ]; then
        print_status 0 "Found USB devices (cu.*):"
        echo "$USB_DEVICES" | while read device; do
            PERMISSIONS=$(ls -la "$device" 2>/dev/null | awk '{print $1}')
            echo "   📌 $device ($PERMISSIONS)"
        done
    else
        print_status 1 "No USB devices found in /dev/cu.*"
    fi
    
    if [ -n "$TTY_DEVICES" ]; then
        echo ""
        print_info "Found TTY USB devices:"
        echo "$TTY_DEVICES" | while read device; do
            PERMISSIONS=$(ls -la "$device" 2>/dev/null | awk '{print $1}')
            echo "   📌 $device ($PERMISSIONS)"
        done
    fi
    
    if [ -z "$USB_DEVICES" ] && [ -z "$TTY_DEVICES" ]; then
        print_warning "No USB serial devices found!"
        print_info "Possible causes:"
        echo "   1. Printer not connected"
        echo "   2. Printer not powered on"
        echo "   3. USB cable issue"
        echo "   4. Driver not installed"
    fi
else
    print_status 1 "/dev/ directory not found"
fi
echo ""

# 3. Check CUPS Printers
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "3. CUPS Printer Status"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if command -v lpstat &> /dev/null; then
    CUPS_PRINTERS=$(lpstat -p 2>/dev/null)
    if [ -n "$CUPS_PRINTERS" ]; then
        print_status 0 "CUPS printers found:"
        echo "$CUPS_PRINTERS" | while read line; do
            echo "   📄 $line"
        done
    else
        print_status 1 "No CUPS printers configured"
        print_info "Add printer in: System Settings → Printers & Scanners"
    fi
else
    print_status 1 "CUPS (lpstat) not available"
    print_info "Install CUPS: brew install cups (macOS) or apt-get install cups (Linux)"
fi
echo ""

# 4. Check Permissions
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "4. Permission Check"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

USB_SERIALS=$(ls /dev/cu.usbserial* 2>/dev/null)
if [ -n "$USB_SERIALS" ]; then
    echo "$USB_SERIALS" | while read device; do
        PERMS=$(ls -la "$device" | awk '{print $1}')
        OWNER=$(ls -la "$device" | awk '{print $3":"$4}')
        
        if [ -r "$device" ] && [ -w "$device" ]; then
            print_status 0 "$device is readable and writable"
            echo "   Owner: $OWNER | Permissions: $PERMS"
        else
            print_status 1 "$device is NOT accessible"
            echo "   Owner: $OWNER | Permissions: $PERMS"
            print_warning "Fix with: sudo chmod 666 $device"
        fi
    done
else
    print_warning "No /dev/cu.usbserial* devices found"
fi
echo ""

# 5. Test Write Permission
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "5. Write Permission Test"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

FIRST_USB=$(ls /dev/cu.usbserial* 2>/dev/null | head -1)
if [ -n "$FIRST_USB" ]; then
    print_info "Testing write to: $FIRST_USB"
    if echo "test" > "$FIRST_USB" 2>/dev/null; then
        print_status 0 "Write permission OK"
    else
        print_status 1 "Cannot write to device"
        print_warning "Run as root or fix permissions:"
        echo "   sudo chmod 666 $FIRST_USB"
        echo "   OR"
        echo "   sudo usermod -aG dialout $(whoami)  # Linux only"
    fi
else
    print_warning "No USB device to test"
fi
echo ""

# 6. Check Serial Port Tools
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "6. Required Tools"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

check_command() {
    if command -v $1 &> /dev/null; then
        print_status 0 "$1 is installed"
    else
        print_status 1 "$1 is NOT installed"
        print_info "Install with: $2"
    fi
}

check_command "lpstat" "brew install cups (macOS) | apt-get install cups (Linux)"
check_command "lp" "brew install cups (macOS) | apt-get install cups (Linux)"
check_command "cu" "brew install cu (macOS) | apt-get install cu (Linux)"
check_command "dotnet" "Install .NET SDK from https://dot.net"
echo ""

# 7. Bluetooth Status (if macOS)
if [[ "$OSTYPE" == "darwin"* ]]; then
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "7. Bluetooth Status (macOS)"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    BT_DEVICES=$(system_profiler SPBluetoothDataType 2>/dev/null | grep -A 10 "RPP02N")
    if [ -n "$BT_DEVICES" ]; then
        print_status 0 "RPP02N Bluetooth printer found"
        echo "$BT_DEVICES" | grep -E "(Address|RSSI|Services)" | while read line; do
            echo "   $line"
        done
    else
        print_warning "RPP02N Bluetooth printer not found"
        print_info "Pair printer in: System Settings → Bluetooth"
    fi
    echo ""
fi

# 8. Create Test Print File
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "8. Manual Test Print"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

TEST_FILE="/tmp/studyhub_test_print.bin"

# Create ESC/POS test file
printf '\x1B\x40' > "$TEST_FILE"  # Initialize
printf '\x1B\x61\x01' >> "$TEST_FILE"  # Center align
printf 'STUDYHUB TEST PRINT\n' >> "$TEST_FILE"
printf '\x1B\x61\x00' >> "$TEST_FILE"  # Left align
printf 'Time: %s\n' "$(date)" >> "$TEST_FILE"
printf 'User: %s\n' "$(whoami)" >> "$TEST_FILE"
printf '\n\n\n' >> "$TEST_FILE"
printf '\x1D\x56\x41\x00' >> "$TEST_FILE"  # Cut paper

print_info "Test file created at: $TEST_FILE"
print_info "File size: $(wc -c < "$TEST_FILE") bytes"
echo ""

# Provide test commands
print_info "To test printing, run ONE of these commands:"
echo ""

if [ -n "$FIRST_USB" ]; then
    echo "   📌 USB Direct:"
    echo "      cat $TEST_FILE > $FIRST_USB"
    echo ""
fi

if command -v lp &> /dev/null; then
    FIRST_CUPS=$(lpstat -p 2>/dev/null | head -1 | awk '{print $2}')
    if [ -n "$FIRST_CUPS" ]; then
        echo "   📌 CUPS Print:"
        echo "      lp -d $FIRST_CUPS -o raw $TEST_FILE"
        echo ""
    fi
fi
echo ""

# 9. Recommendations
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "9. Recommendations"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if [ -z "$USB_DEVICES" ]; then
    echo "🔴 CRITICAL: No USB printer detected"
    echo "   1. Check USB cable is firmly connected"
    echo "   2. Check printer is powered on"
    echo "   3. Try a different USB port"
    echo "   4. Check dmesg logs: dmesg | tail -50"
elif [ -n "$FIRST_USB" ] && ! [ -w "$FIRST_USB" ]; then
    echo "🔴 CRITICAL: USB device found but no write permission"
    echo "   Fix with: sudo chmod 666 $FIRST_USB"
elif [ -z "$CUPS_PRINTERS" ]; then
    echo "🟡 WARNING: No CUPS printer configured"
    echo "   The app can still print via direct USB"
    echo "   OR add printer in System Settings → Printers & Scanners"
else
    echo "🟢 GOOD: Printer detected and accessible"
    echo "   Run the test print command above to verify"
fi

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "For more help, see: USB_PRINTER_SERVER_DEPLOYMENT.md"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

