# USB Printer Server Deployment Guide

## Problem Summary
Backend reports "Receipt printed successfully" but nothing prints when deployed on server with USB printer.

## Solution Implemented
Updated `ThermalPrinterService.cs` to:
1. **Wait for actual print completion** (default: 15 seconds timeout)
2. **Return real success/failure status** instead of always returning true
3. **Enhanced logging** to identify server-specific issues
4. **Better USB printer detection** for server environments

---

## How It Works Now

### Before (Fire-and-Forget)
```
API Request → Queue Print Job → Return Success Immediately ✅
                    ↓
           (Print happens in background)
           (May fail silently)
```

### After (Synchronous with Timeout)
```
API Request → Start Print Job → Wait up to 15s → Return Real Status ✅/❌
                    ↓
           (Print completes or fails)
           (Status reflects reality)
```

---

## Server Deployment Checklist

### 1. Check Printer Connection
```bash
# List all USB devices
ls -la /dev/cu.* | grep -i usb
ls -la /dev/tty.* | grep -i usb

# Expected output (example):
# /dev/cu.usbserial-1234
# /dev/tty.usbserial-1234
```

### 2. Check CUPS Printer (macOS/Linux)
```bash
# List installed printers
lpstat -p

# Expected output:
# printer Manufacture_Virtual_PRN is idle.  enabled since ...
```

### 3. Check File Permissions
```bash
# Check who can access the USB port
ls -la /dev/cu.usbserial*

# If permission denied, grant access:
sudo chmod 666 /dev/cu.usbserial*
```

### 4. Test Print Manually
```bash
# Create test file
echo -e '\x1B\x40Hello World\x0A\x0A\x0A\x1D\x56\x41\x00' > test.bin

# Print via CUPS (if available)
lp -d Manufacture_Virtual_PRN -o raw test.bin

# OR print directly to USB port
cat test.bin > /dev/cu.usbserial-1234
```

### 5. Monitor Backend Logs
When you call the print API, check the console output for detailed logs:

```
🔍 Starting printer detection...
📊 Data size: 1234 bytes
💻 OS: Unix
👤 User: appuser
📁 Temp path: /tmp/

📋 Scanning /dev/ for serial ports...
📊 Found 12 potential serial ports
📋 Available serial ports:
   - /dev/cu.usbserial-1234 (permissions: -rw-rw-rw-)
   
✅ Found USB printer port: /dev/cu.usbserial-1234
🔌 Connection type: USB
📝 Pattern matched: usbserial

🖨️  Attempting serial port print...
📡 Connecting to printer on /dev/cu.usbserial-1234...
✅ Port opened successfully, sending 1234 bytes...
🔌 USB connection detected - using FAST mode
📤 Sending chunk 1/3 (512 bytes)...
✓ Progress: 512/1234 bytes (41%)
📤 Sending chunk 2/3 (512 bytes)...
✓ Progress: 1024/1234 bytes (82%)
📤 Sending chunk 3/3 (210 bytes)...
✓ Progress: 1234/1234 bytes (100%)
⏳ Waiting 1000ms for printer to complete...
🔓 Closing port...
✅ Print completed successfully on attempt 1!
✅ Sent 1234 bytes in 3 chunks
```

---

## Common Issues & Solutions

### Issue 1: "No printer port found"
**Cause:** Printer not connected or not detected by OS

**Solutions:**
1. Check USB cable is firmly connected
2. Check printer is powered on
3. Run: `ls -la /dev/cu.* | grep -i usb`
4. Try unplugging and re-plugging USB cable
5. Check if server user has permission to access `/dev` ports

---

### Issue 2: "Permission denied" on /dev/cu.usbserial
**Cause:** Server user doesn't have permission to access USB port

**Solutions:**
```bash
# Option 1: Grant permission (temporary)
sudo chmod 666 /dev/cu.usbserial*

# Option 2: Add user to dialout group (permanent, Linux)
sudo usermod -aG dialout $USER
sudo systemctl restart your-app.service

# Option 3: Run app as root (not recommended for production)
sudo dotnet run
```

---

### Issue 3: "Port opened successfully" but nothing prints
**Cause:** Wrong port, wrong baud rate, or printer not responding

**Solutions:**
1. Verify it's the correct port:
   ```bash
   # Check what's connected
   system_profiler SPUSBDataType | grep -A 10 "Serial"
   ```

2. Try different baud rate (currently 9600):
   - Common rates: 9600, 19200, 38400, 115200
   - Check printer manual for correct baud rate

3. Test with direct echo:
   ```bash
   echo "Test" > /dev/cu.usbserial-1234
   ```

---

### Issue 4: "Print timed out after 15000ms"
**Cause:** Print taking too long or printer hanging

**Solutions:**
1. Increase timeout in controller:
   ```csharp
   // In your controller
   await _thermalPrinterService.PrintReceiptAsync(receipt, waitForCompletion: true, timeoutMs: 30000);
   ```

2. Check printer queue isn't stuck:
   ```bash
   lpstat -o  # List print jobs
   cancel -a  # Cancel all jobs
   ```

3. Power cycle the printer

---

### Issue 5: CUPS printer found but print fails
**Cause:** CUPS configuration issue or wrong driver

**Solutions:**
1. Check printer status:
   ```bash
   lpstat -p -d
   ```

2. Check print queue:
   ```bash
   lpstat -o
   ```

3. View printer details:
   ```bash
   lpoptions -d Manufacture_Virtual_PRN -l
   ```

4. Test raw print mode:
   ```bash
   lp -d Manufacture_Virtual_PRN -o raw test.bin
   ```

5. Re-add printer in System Settings with correct driver

---

## Configuration for Different Environments

### Development (macOS with Bluetooth)
```csharp
// Use non-blocking mode to avoid frontend timeout
await _thermalPrinterService.PrintReceiptAsync(receipt, waitForCompletion: false);
```

### Production Server (USB Printer)
```csharp
// Use blocking mode to get accurate status
var success = await _thermalPrinterService.PrintReceiptAsync(receipt, waitForCompletion: true, timeoutMs: 15000);

if (!success)
{
    // Log error, retry, or save for later
    _logger.LogError("Failed to print receipt. Check printer connection.");
}
```

### Docker Container
```dockerfile
# Dockerfile - Ensure USB access
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install CUPS and serial port tools
RUN apt-get update && apt-get install -y \
    cups \
    cu \
    setserial

# Add app user to dialout group for serial port access
RUN usermod -aG dialout app

# Run container with device access
# docker run --device=/dev/ttyUSB0:/dev/ttyUSB0 your-image
```

---

## Testing Commands

### Test 1: List All Available Printers
```bash
lpstat -p -d
```

### Test 2: Check USB Devices
```bash
ls -la /dev/cu.* | grep -i usb
ls -la /dev/tty.* | grep -i usb
```

### Test 3: Check Permissions
```bash
whoami  # Check current user
groups  # Check current user's groups
ls -la /dev/cu.usbserial*  # Check port permissions
```

### Test 4: Manual Print Test
```bash
# Create simple ESC/POS test
echo -e '\x1B\x40\x1B\x61\x01TEST PRINT\x0A\x0A\x0A\x1D\x56\x41\x00' > /tmp/test.bin

# Method 1: CUPS (if available)
lp -d Manufacture_Virtual_PRN -o raw /tmp/test.bin

# Method 2: Direct to USB
cat /tmp/test.bin > /dev/cu.usbserial-1234
```

### Test 5: Monitor Serial Port Activity
```bash
# Install cu if not available
brew install cu  # macOS
apt-get install cu  # Linux

# Connect to serial port
cu -l /dev/cu.usbserial-1234 -s 9600

# Type commands and see printer response
# Press ~. to exit
```

---

## Fallback: Save Failed Prints

If printing fails, the service automatically saves the receipt to a file:

```
📄 Receipt saved to: /tmp/receipt_20250103143022.bin
```

You can print these files later:
```bash
# Print all failed receipts
for file in /tmp/receipt_*.bin; do
  lp -d Manufacture_Virtual_PRN -o raw "$file"
done
```

---

## Summary

✅ **What Changed:**
- Print operation now waits for completion (default 15s timeout)
- Returns accurate success/failure status
- Enhanced logging for troubleshooting
- Better USB printer detection

🔍 **Troubleshooting Steps:**
1. Check logs for detailed printer detection info
2. Verify USB device exists: `ls -la /dev/cu.* | grep -i usb`
3. Check permissions: `ls -la /dev/cu.usbserial*`
4. Test manual print: `echo "test" > /dev/cu.usbserial-1234`
5. Check CUPS: `lpstat -p`

📞 **Still Not Working?**
Share the console logs from the backend - they now contain detailed diagnostic information to identify the exact issue.

