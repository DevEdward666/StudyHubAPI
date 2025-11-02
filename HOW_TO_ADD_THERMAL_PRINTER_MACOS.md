# 🖨️ How to Add RPP02N-1175 Thermal Printer to macOS

## 📝 IMPORTANT: This is a Bluetooth Serial Printer

The RPP02N-1175 is **NOT** a regular macOS printer. It's a **Bluetooth serial device** that communicates directly through a serial port, not through the macOS Printer system.

❌ **DON'T** add it to System Settings → Printers & Scanners  
✅ **DO** pair it via System Settings → Bluetooth

---

## 🔧 STEP-BY-STEP PAIRING GUIDE

### Step 1: Prepare the Printer

1. **Turn ON the printer**
   - Press and hold the power button until LED lights up
   - LED should be blue (Bluetooth mode)

2. **Enable Pairing Mode**
   - Press and hold the **Bluetooth button** (usually has ⊛ symbol)
   - Hold for 3-5 seconds until LED **blinks rapidly**
   - This means printer is in **discoverable mode**

---

### Step 2: Pair via macOS Bluetooth

1. **Open System Settings**
   - Click  (Apple menu) → **System Settings**
   - OR click Bluetooth icon in menu bar → **Bluetooth Settings**

2. **Go to Bluetooth**
   - Click **Bluetooth** in the sidebar
   - Make sure Bluetooth is **ON**

3. **Wait for Printer to Appear**
   - Look for device named:
     - **RPP02N-1175**
     - **RPP02N**
     - **Bluetooth Printer**
     - Or similar name
   - It should appear in "Nearby Devices" section

4. **Click "Connect"**
   - Click the **Connect** button next to the printer name
   - Wait for connection (~5-10 seconds)
   - Status should change to **Connected**

5. **Verify Connection**
   - Printer should now be in "My Devices" section
   - Status: **Connected**
   - You may see "Not Connected" briefly then "Connected"

---

### Step 3: Verify Serial Port Created

After pairing, macOS creates a serial port. Let's verify:

1. **Open Terminal** (Spotlight → type "Terminal")

2. **Run this command:**
   ```bash
   ls -la /dev/cu.* | grep -E "(RPP|Bluetooth)"
   ```

3. **Expected Output:**
   ```
   /dev/cu.Bluetooth-Incoming-Port
   /dev/cu.RPP02N-SerialPort          ← This is what you need! ✅
   ```

4. **If you see both ports** = SUCCESS! ✅

5. **If you only see Incoming-Port** = Need to reconnect:
   ```bash
   # Check all available ports
   ls /dev/cu.*
   ```

---

### Step 4: Test the Connection

**⚠️ IMPORTANT: macOS Permission Issue**

If you get `operation not permitted` error when testing, this is normal! macOS blocks Terminal from accessing serial ports for security reasons.

**Option A: Grant Terminal Full Disk Access** (Not Recommended)
1. System Settings → Privacy & Security → Full Disk Access
2. Click + and add Terminal
3. Restart Terminal
4. Try: `echo "TEST" > /dev/cu.RPP02N-SerialPort`

**Option B: Use Backend App Instead** (✅ Recommended)
The backend app already has the necessary permissions. Just use it to test:
1. Start your backend: `cd Study-Hub && dotnet run`
2. Add a transaction in the app
3. Click "Print Receipt"
4. Check console output - it should work!

**Option C: Use screen command** (Works without permissions)
```bash
# Connect to printer (read-only, but verifies connection)
screen /dev/cu.RPP02N-SerialPort 9600

# Press Ctrl+A then K to exit
# If this works, your backend will work too!
```

---

## 🔍 TROUBLESHOOTING

### Issue 0: "operation not permitted" Error in Terminal

**This is NORMAL!** macOS blocks Terminal from accessing serial ports.

**Solution:**
✅ **Don't worry about it!** Your backend app will work fine because:
- .NET apps have proper serial port access
- Only Terminal is blocked, not all apps
- This is a macOS security feature

**To verify the port exists (without permissions needed):**
```bash
# This will show the port exists (even if you can't write to it)
ls -l /dev/cu.RPP02N-SerialPort

# Expected output:
crw-rw-rw-  1 root  wheel   ... /dev/cu.RPP02N-SerialPort
```

**To actually test printing:**
Just use your backend - it will work! The permission error ONLY affects Terminal.

---

### Issue 1: Printer Not Appearing in Bluetooth

**Solution:**
1. Make sure printer is in pairing mode (LED blinking rapidly)
2. Turn printer OFF and ON again
3. Move printer closer to Mac (within 1 meter)
4. Check printer battery is charged
5. Try forgetting other Bluetooth devices to free up slots

### Issue 2: "Connection Failed" Error

**Solution:**
1. **Forget the device:**
   - Click ⓘ next to printer name
   - Click "Forget This Device"
   - Wait 10 seconds
2. **Reset printer Bluetooth:**
   - Turn OFF printer
   - Wait 10 seconds
   - Turn ON and enter pairing mode again
3. **Try pairing again**

### Issue 3: Connected but No Serial Port

**Solution:**
1. **Check if it's actually connected:**
   ```bash
   system_profiler SPBluetoothDataType | grep -A 10 RPP
   ```

2. **If connected but no port, reconnect:**
   ```bash
   # Disconnect
   blueutil --disconnect [MAC_ADDRESS]
   
   # Wait 5 seconds
   
   # Reconnect via System Settings
   ```

3. **Check all serial ports:**
   ```bash
   ls -la /dev/cu.* /dev/tty.* | grep -i bluetooth
   ```

### Issue 4: Only See "Bluetooth-Incoming-Port"

**Solution:**
This means the pairing isn't complete. The printer needs to create **two** ports:
- Incoming-Port (for receiving data TO Mac)
- SerialPort or RPP02N-Port (for sending data TO printer)

**Fix:**
1. **Forget the device** in Bluetooth settings
2. **Reset printer:**
   - Turn OFF
   - Press and hold power + Bluetooth buttons together
   - Hold for 5 seconds (may reset to factory)
3. **Pair again from scratch**

---

## 🎯 WHAT THE APP NEEDS

Your backend application needs to see:
```
/dev/cu.RPP02N-SerialPort
```

**NOT** this:
```
/dev/cu.Bluetooth-Incoming-Port  ← Wrong direction
```

---

## 📋 VERIFICATION CHECKLIST

After pairing, verify everything works:

- [ ] Printer shows "Connected" in Bluetooth settings
- [ ] LED on printer is solid blue (not blinking)
- [ ] Terminal command shows RPP02N-SerialPort exists
- [ ] Test print works: `echo "TEST" > /dev/cu.RPP02N-SerialPort`
- [ ] Backend console shows: "Found RPP02N printer port"
- [ ] Backend can send data successfully
- [ ] Receipt actually prints

---

## 🔧 ALTERNATIVE: Using Terminal for Advanced Pairing

If GUI pairing doesn't work, try Terminal:

```bash
# 1. Enable Bluetooth
blueutil --power 1

# 2. Start scanning
blueutil --inquiry

# 3. Find your printer's MAC address in the list

# 4. Pair with the printer (replace XX:XX:XX:XX:XX:XX)
blueutil --pair XX:XX:XX:XX:XX:XX

# 5. Connect
blueutil --connect XX:XX:XX:XX:XX:XX

# 6. Check port
ls /dev/cu.* | grep RPP
```

**Install blueutil if needed:**
```bash
brew install blueutil
```

---

## 📱 PRINTER SPECIFICATIONS

**RPP02N-1175 Details:**
- Type: Bluetooth thermal receipt printer
- Connection: Bluetooth 3.0 + EDR
- Profile: SPP (Serial Port Profile)
- Paper Width: 58mm
- Interface: Serial (not USB, not regular printer)

**Important:** This printer uses **SPP (Serial Port Profile)**, which is why it appears as a serial device (`/dev/cu.*`) and NOT in the Printers & Scanners settings.

---

## ✅ EXPECTED RESULT

After successful pairing:

**In Bluetooth Settings:**
```
RPP02N-1175
Status: Connected
```

**In Terminal:**
```bash
$ ls /dev/cu.* | grep RPP
/dev/cu.RPP02N-SerialPort
```

**In Backend Console:**
```
🔍 Searching for Bluetooth printer...
📋 Available serial ports:
   - /dev/cu.Bluetooth-Incoming-Port
   - /dev/cu.RPP02N-SerialPort
✅ Found RPP02N printer port: /dev/cu.RPP02N-SerialPort
```

**When Printing:**
```
📡 Connecting to printer on /dev/cu.RPP02N-SerialPort...
✅ Port opened successfully, sending 1019 bytes...
📤 Sent 512/1019 bytes (50%)
📤 Sent 1019/1019 bytes (100%)
✅ Successfully sent 1019 bytes to printer
🖨️ Receipt prints! ✅
```

---

## 🎉 SUMMARY

1. ✅ Pair via **Bluetooth Settings** (not Printers & Scanners)
2. ✅ Verify `/dev/cu.RPP02N-SerialPort` exists
3. ✅ Restart backend
4. ✅ Test printing
5. ✅ Done!

**This is NOT a regular printer - it's a Bluetooth serial device!**

---

**Need Help?** Run diagnostics:
```bash
# Check Bluetooth status
system_profiler SPBluetoothDataType

# Check serial ports
ls -la /dev/cu.* | grep -i bluetooth

# Test printer
echo "TEST" > /dev/cu.RPP02N-SerialPort
```

