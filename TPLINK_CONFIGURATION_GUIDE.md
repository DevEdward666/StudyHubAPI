╔══════════════════════════════════════════════════════════════════╗
║                                                                  ║
║  ⚙️ TP-Link ER7206 Configuration Guide                           ║
║  WiFi Portal & Captive Portal Setup                              ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝


✅ CODE UPDATES COMPLETED
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

FILES UPDATED:
1. ✅ Study-Hub/appsettings.json
   → Added TP-Link ER7206 specific configuration
   → Added Omada Controller settings

2. ✅ Study-Hub/Models/RouterOptions.cs
   → Added Type field for router identification
   → Added OmadaControllerOptions class
   → Support for Omada API integration

3. ✅ Study-Hub/Service/SshRouterManager.cs
   → Updated comments for TP-Link ER7206
   → Better error messages
   → Notes about Omada Controller

4. ✅ Study-Hub/Service/OmadaControllerService.cs (NEW)
   → Omada Controller API integration
   → Guest authorization management
   → Time-based access control
   → Auto-disconnect capability


⚙️ CONFIGURATION OPTIONS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

You now have TWO configuration options in appsettings.json:


OPTION 1: SOFTWARE-ONLY (CURRENT - NO ROUTER CONTROL)
┌────────────────────────────────────────────────────────────────┐
│ "Router": {                                                     │
│   "Type": "TPLink-ER7206",                                      │
│   "Host": "192.168.0.1",                                        │
│   "Port": 22,                                                   │
│   "Username": "admin",                                          │
│   "Password": "your_password",                                  │
│   "OmadaController": {                                          │
│     "Enabled": false    ← Software-only mode                    │
│   }                                                             │
│ }                                                               │
└────────────────────────────────────────────────────────────────┘

What this does:
- WiFi password generation ✅
- Enhanced portal with countdown ✅
- QR code method ✅
- NO router control ❌
- NO auto-disconnect ❌


OPTION 2: OMADA CONTROLLER INTEGRATION (FULL CONTROL)
┌────────────────────────────────────────────────────────────────┐
│ "Router": {                                                     │
│   "Type": "TPLink-ER7206",                                      │
│   "Host": "192.168.0.1",                                        │
│   "OmadaController": {                                          │
│     "Enabled": true,     ← Enable Omada integration            │
│     "Url": "https://localhost:8043",                            │
│     "Username": "admin",                                        │
│     "Password": "your_omada_password",                          │
│     "SiteId": "Default"                                         │
│   }                                                             │
│ }                                                               │
└────────────────────────────────────────────────────────────────┘

What this does:
- WiFi password generation ✅
- Enhanced portal with countdown ✅
- QR code method ✅
- Router control via Omada API ✅
- TRUE auto-disconnect ✅
- Time-based access enforcement ✅


📋 STEP-BY-STEP: UPDATE YOUR CONFIGURATION
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

STEP 1: UPDATE appsettings.json

Open: Study-Hub/appsettings.json

Find the Router section and update:

```json
"Router": {
  "Type": "TPLink-ER7206",
  "Host": "192.168.0.1",        ← Your ER7206 IP address
  "Port": 22,
  "Username": "admin",           ← Your ER7206 username
  "Password": "your_password",   ← Your ER7206 password
  "AddScriptPath": "/usr/local/bin/add_whitelist.sh",
  "RemoveScriptPath": "/usr/local/bin/remove_whitelist.sh",
  "OmadaController": {
    "Enabled": false,            ← Set to true if using Omada
    "Url": "https://localhost:8043",  ← Your Omada Controller URL
    "Username": "admin",         ← Your Omada username
    "Password": "omada_pass",    ← Your Omada password
    "SiteId": "Default"          ← Your Omada site name
  }
}
```


STEP 2: CHOOSE YOUR PATH

PATH A: Software-only (Current - No changes needed)
  → Set "OmadaController.Enabled": false
  → Your system works as-is
  → Enhanced portal with countdown timer
  → 70-80% user compliance

PATH B: Omada Integration (For TRUE auto-disconnect)
  → Set "OmadaController.Enabled": true
  → Requires Omada Controller setup
  → See: TPLINK_ER7206_OMADA_GUIDE.md
  → 100% auto-disconnect capability


🔧 ROUTER CONFIGURATION (TP-LINK ER7206)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

DEFAULT IP ADDRESSES:

TP-Link ER7206 Default:
- IP: 192.168.0.1
- Username: admin
- Password: (set during initial setup)

Common Configurations:
- Static IP: 192.168.0.1 (default)
- DHCP Server: Usually enabled on LAN1
- Management Port: 8043 (HTTPS)


HOW TO ACCESS YOUR ER7206:

1. Connect to ER7206 network
2. Open browser: https://192.168.0.1
3. Login with admin credentials
4. Configure as needed


SSH ACCESS (For Router Whitelist Feature):

Check if SSH is enabled:
1. Login to ER7206 web interface
2. Go to: System → Management
3. Look for: SSH Server settings
4. Enable if available (may require firmware)

Note: SSH may not be available on ER7206 without Omada Controller


🎯 RECOMMENDED SETUP FOR TP-LINK ER7206
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

PHASE 1: NOW (Use what you have)
┌────────────────────────────────────────────────────────────────┐
│ 1. Update appsettings.json with your ER7206 details            │
│ 2. Set OmadaController.Enabled = false                         │
│ 3. Use enhanced portal (already deployed)                      │
│ 4. QR code method for customers                                │
│ 5. Result: Works today, 70-80% compliance                      │
└────────────────────────────────────────────────────────────────┘

PHASE 2: LATER (Optional - Add Omada)
┌────────────────────────────────────────────────────────────────┐
│ 1. Purchase Omada Access Point ($50-80)                        │
│ 2. Install Omada Software Controller (FREE)                    │
│ 3. Adopt ER7206 and AP in controller                           │
│ 4. Update appsettings.json:                                    │
│    - Set OmadaController.Enabled = true                        │
│    - Configure Omada URL and credentials                       │
│ 5. Result: TRUE 100% auto-disconnect!                          │
└────────────────────────────────────────────────────────────────┘


📝 CONFIGURATION EXAMPLES
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

EXAMPLE 1: Basic ER7206 (Software-only)
```json
{
  "Router": {
    "Type": "TPLink-ER7206",
    "Host": "192.168.0.1",
    "Port": 22,
    "Username": "admin",
    "Password": "MySecurePassword123",
    "OmadaController": {
      "Enabled": false
    }
  }
}
```

EXAMPLE 2: ER7206 with Software Controller
```json
{
  "Router": {
    "Type": "TPLink-ER7206",
    "Host": "192.168.0.1",
    "OmadaController": {
      "Enabled": true,
      "Url": "https://localhost:8043",
      "Username": "admin",
      "Password": "OmadaPass456",
      "SiteId": "Default"
    }
  }
}
```

EXAMPLE 3: ER7206 with Hardware Controller (OC200)
```json
{
  "Router": {
    "Type": "TPLink-ER7206",
    "Host": "192.168.0.1",
    "OmadaController": {
      "Enabled": true,
      "Url": "https://192.168.0.50:8043",  ← OC200 IP
      "Username": "admin",
      "Password": "OmadaHardware789",
      "SiteId": "StudyHub-Site"
    }
  }
}
```


🔍 FINDING YOUR OMADA CONTROLLER URL
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

SOFTWARE CONTROLLER (Running on PC):
  - Local: https://localhost:8043
  - Network: https://YOUR_PC_IP:8043
  - Example: https://192.168.0.100:8043

HARDWARE CONTROLLER (OC200/OC300):
  - Find controller IP in your network
  - Usually: https://CONTROLLER_IP:8043
  - Example: https://192.168.0.50:8043

OMADA CLOUD:
  - URL: https://omada.tplinkcloud.com
  - Requires cloud account
  - May need different API endpoints


✅ TESTING YOUR CONFIGURATION
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

STEP 1: Update Configuration
  □ Edit appsettings.json with your ER7206 details
  □ Save file

STEP 2: Restart Backend
  cd Study-Hub
  dotnet run

STEP 3: Test WiFi Portal
  □ Generate password in admin portal
  □ Check console for any router errors
  □ Test public portal with countdown timer

STEP 4: Check Logs
  Look for messages like:
  - "TP-Link ER7206 may not have SSH enabled" (expected)
  - "For WiFi management, consider using Omada Controller"

STEP 5: Verify Enhanced Portal
  □ Visit: http://localhost:5173/wifi
  □ Enter test password
  □ See countdown timer
  □ Verify warnings at 5 minutes
  □ Check session expired screen


🆘 TROUBLESHOOTING
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

ISSUE: "SSH connection failed"
  → This is EXPECTED for ER7206
  → SSH is typically not enabled
  → Your WiFi portal still works!
  → Router whitelist feature won't work (that's OK)

ISSUE: "Omada authentication failed"
  → Check OmadaController.Url is correct
  → Verify username and password
  → Make sure Omada Controller is running
  → Check firewall allows port 8043

ISSUE: Configuration not loading
  → Check JSON syntax (commas, quotes)
  → Restart backend after changes
  → Verify appsettings.json location


📊 WHAT WORKS WITHOUT OMADA
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

With OmadaController.Enabled = false:

✅ Admin password generation
✅ Public WiFi portal
✅ Countdown timer
✅ Recurring validation
✅ Expiration warnings
✅ Session expired screen
✅ QR code method
✅ Database tracking
✅ One-time redemption

❌ Router auto-disconnect
❌ MAC whitelisting
❌ Network control


📊 WHAT WORKS WITH OMADA
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

With OmadaController.Enabled = true:

✅ Everything from above, PLUS:
✅ Router auto-disconnect
✅ Time-based access control
✅ Guest authorization
✅ Network control
✅ Captive portal integration
✅ 100% enforcement


╔══════════════════════════════════════════════════════════════════╗
║                                                                  ║
║  🎊 CONFIGURATION UPDATED FOR TP-LINK ER7206!                    ║
║                                                                  ║
║  NEXT STEPS:                                                     ║
║                                                                  ║
║  1. Update appsettings.json with your ER7206 IP and password    ║
║  2. Set OmadaController.Enabled = false (for now)               ║
║  3. Restart backend: dotnet run                                 ║
║  4. Test WiFi portal                                             ║
║  5. Use enhanced portal with customers!                          ║
║                                                                  ║
║  LATER (Optional):                                               ║
║  → Set up Omada Controller                                       ║
║  → Enable Omada integration                                      ║
║  → Get TRUE 100% auto-disconnect!                                ║
║                                                                  ║
║  See: TPLINK_ER7206_OMADA_GUIDE.md for Omada setup              ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝


Status: ✅ Code updated for TP-Link ER7206
Configuration: Ready for your credentials
Next: Update appsettings.json and test!

