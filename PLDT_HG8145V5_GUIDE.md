╔══════════════════════════════════════════════════════════════════╗
║                                                                  ║
║  📡 PLDT HG8145V5 Router - WiFi Portal Setup Guide               ║
║  Huawei ONT Configuration for WiFi Password System               ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝


🔍 ROUTER INFORMATION
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Model:          PLDT HG8145V5
Manufacturer:   Huawei
Type:           GPON ONT (Optical Network Terminal)
Common in:      Philippines (PLDT Fiber)
Web Interface:  http://192.168.1.1
Default User:   adminpldt (or admin)
Default Pass:   Your custom password


⚠️ IMPORTANT NOTES ABOUT HG8145V5
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

1. SSH ACCESS:
   ❌ SSH is typically DISABLED by default on PLDT HG8145V5
   ❌ PLDT locks SSH access for security reasons
   ❌ Even if enabled, SSH might not allow custom scripts
   
2. TELNET ACCESS:
   ⚠️  May be available with admin credentials
   ⚠️  Not recommended for security reasons
   ⚠️  Limited command availability

3. WEB API:
   ✅ Has a web interface API
   ✅ Can be accessed via HTTP/HTTPS
   ⚠️  Not officially documented
   ⚠️  May change with firmware updates


🎯 RECOMMENDED APPROACH FOR HG8145V5
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Since SSH is likely unavailable, you have THREE OPTIONS:

┌──────────────────────────────────────────────────────────────────┐
│ OPTION 1: SOFTWARE-ONLY SOLUTION (RECOMMENDED)                  │
└──────────────────────────────────────────────────────────────────┘

Use the WiFi password system WITHOUT router integration:

✅ Generate WiFi passwords in the portal
✅ Share passwords with customers
✅ Customers connect using the EXISTING WiFi SSID and password
✅ Your app TRACKS who has valid passwords
✅ No router modification needed

HOW IT WORKS:
1. Admin generates password in portal → "Abc3Xy9Z"
2. Customer asks for WiFi
3. Admin shares: "SSID: PLDT_HomeWiFi, Password: YourMainPassword"
4. Customer connects to WiFi
5. Customer enters "Abc3Xy9Z" in your captive portal webpage
6. System validates and tracks usage
7. After expiry, customer must get new password

PROS:
✓ Works immediately, no router config needed
✓ Simple to implement
✓ Safe, no router modifications
✓ Works with any router

CONS:
✗ Customers still use main WiFi password
✗ No automatic blocking after expiry
✗ Relies on honor system


┌──────────────────────────────────────────────────────────────────┐
│ OPTION 2: GUEST NETWORK WITH MANUAL CHANGES                     │
└──────────────────────────────────────────────────────────────────┘

Use the router's Guest WiFi feature:

SETUP:
1. Login to http://192.168.1.1
2. Go to WLAN → Guest Network
3. Enable Guest WiFi
4. Set Guest SSID: "StudyHub-Guest"
5. Set security: WPA2-PSK
6. Change password manually when needed

HOW IT WORKS:
1. Admin generates password in portal
2. Admin MANUALLY changes Guest WiFi password in router
3. Admin shares new Guest WiFi credentials
4. Customer connects with temp password
5. Admin changes password again after customer leaves

PROS:
✓ True network isolation
✓ Can disable guest network anytime
✓ Separate from main network

CONS:
✗ Requires manual password changes
✗ Time-consuming
✗ Not automated


┌──────────────────────────────────────────────────────────────────┐
│ OPTION 3: THIRD-PARTY ROUTER (ADVANCED)                         │
└──────────────────────────────────────────────────────────────────┘

Add a secondary router with SSH/API support:

HARDWARE:
- TP-Link (with OpenWrt)
- Mikrotik
- Ubiquiti UniFi
- GL.iNet routers

SETUP:
HG8145V5 (bridge mode) → Secondary Router (with SSH)
                          ↑
                    Your app controls this

PROS:
✓ Full automation possible
✓ SSH/API access
✓ Professional solution
✓ Better control

CONS:
✗ Additional hardware cost
✗ More complex setup
✗ Requires technical knowledge


🎯 OUR RECOMMENDATION
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

For PLDT HG8145V5, we recommend:

START WITH: Option 1 (Software-only)
UPGRADE TO: Option 2 (Guest Network) for better control
FUTURE:     Option 3 (Secondary Router) for full automation


🔧 CURRENT CONFIGURATION UPDATE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Your appsettings.json currently has:

  "Router": {
    "Host": "192.168.1.1",
    "Port": 22,                          ← SSH likely not available
    "Username": "adminpldt",             ← Correct
    "Password": "Eiijii@665306",         ← Your password
    "AddScriptPath": "/usr/local/bin/add_whitelist.sh",    ← Won't work
    "RemoveScriptPath": "/usr/local/bin/remove_whitelist.sh" ← Won't work
  }

⚠️  Since SSH is likely disabled on HG8145V5, the router whitelist
    feature won't work. But the WiFi password generation will still
    work perfectly!


✅ WHAT WILL WORK WITHOUT SSH
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

✓ Generate WiFi passwords          → WORKS
✓ Set expiration times             → WORKS
✓ Validate passwords               → WORKS
✓ One-time redemption              → WORKS
✓ Password tracking                → WORKS
✓ Admin portal interface           → WORKS
✓ Copy to clipboard                → WORKS
✓ Toast notifications              → WORKS

❌ Router MAC whitelisting          → WON'T WORK (needs SSH)


📋 IMPLEMENTATION STEPS (SOFTWARE-ONLY)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

STEP 1: Use WiFi Portal As-Is
   - The portal already works!
   - Generate passwords for customers
   - Track who has valid access

STEP 2: Create Captive Portal (Optional)
   - Create a simple landing page
   - Customer opens browser → redirected to your page
   - Enter password → validated → internet access
   - Requires captive portal setup (see below)

STEP 3: Share Main WiFi Credentials
   - SSID: Your current WiFi name
   - Password: Your current WiFi password
   - Customer connects once
   - Then validates via portal

STEP 4: Monitor Usage
   - Track in database
   - See who's connected
   - When passwords expire


🌐 CAPTIVE PORTAL SETUP (OPTIONAL)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

✅ CAPTIVE PORTAL IMPLEMENTED!

A complete customer-facing WiFi portal has been created at:
  → http://localhost:5173/wifi (local)
  → https://your-domain.com/wifi (production)

FEATURES:
✓ Beautiful Starbucks-like UI
✓ Mobile responsive design
✓ Password validation
✓ One-time redemption
✓ Success confirmation
✓ No authentication required

IMPLEMENTATION METHODS:

METHOD 1: QR CODE (RECOMMENDED) ⭐
  Best for PLDT HG8145V5 routers
  
  Steps:
  1. Deploy app to Vercel/Netlify
  2. Create QR code for /wifi page
  3. Print and display signage
  4. Customers scan QR code
  5. Enter access code from staff
  6. Done!
  
  See: QR_CODE_SETUP_GUIDE.md

METHOD 2: ROUTER REDIRECT (IF SUPPORTED)
  Check if your router has captive portal settings:
  
  1. LOGIN TO ROUTER
     http://192.168.1.1

  2. FIND CAPTIVE PORTAL SETTINGS
     Location varies, look for:
     - WLAN → Advanced → Portal
     - WLAN → Guest Network → Portal
     - Security → Captive Portal

  3. IF AVAILABLE:
     - Enable captive portal
     - Set redirect URL: https://your-domain.com/wifi
     - Customers auto-redirected when connecting

  4. IF NOT AVAILABLE:
     - Use Method 1 (QR code)
     - Works perfectly without router changes!

COMPLETE GUIDES:
  → CAPTIVE_PORTAL_IMPLEMENTATION.md (detailed setup)
  → QR_CODE_SETUP_GUIDE.md (signage templates)


🔍 CHECK SSH AVAILABILITY (OPTIONAL TEST)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Want to verify if SSH is available? Try this:

Terminal Command:
  ssh adminpldt@192.168.1.1

Expected Results:
  ✅ Connected → SSH is enabled! (rare)
  ❌ Connection refused → SSH disabled (common)
  ❌ Connection timeout → SSH blocked (common)

If SSH IS available (unlikely):
  1. The router whitelist feature might work
  2. You'd need to install scripts on the router
  3. Requires advanced networking knowledge


🛠️ ALTERNATIVE: HUAWEI WEB API (ADVANCED)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

HG8145V5 has a web interface that uses API calls.
You could potentially:

1. Capture API calls from browser (F12 → Network)
2. Implement HTTP client to call router API
3. Automate guest WiFi password changes

Example API endpoints (may vary by firmware):
  POST http://192.168.1.1/api/login
  POST http://192.168.1.1/api/wlan/guest
  
⚠️  This is reverse-engineered, not officially supported
⚠️  May break with firmware updates
⚠️  Requires significant development time


📊 SUMMARY FOR YOUR SETUP
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Router Model:    PLDT HG8145V5 (Huawei)
SSH Access:      ❌ Likely disabled
Recommended:     Software-only solution
Router Feature:  ⚠️  Will fail (SSH required)
WiFi Portal:     ✅ Works perfectly
Password Gen:    ✅ Works
Validation:      ✅ Works
Redemption:      ✅ Works


🎯 QUICK START FOR YOUR HG8145V5
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

1. USE WIFI PORTAL AS-IS:
   - Don't worry about router SSH
   - Focus on password generation
   - It works great without router integration!

2. WORKFLOW:
   Admin: Generate password in portal
   Admin: Share WiFi credentials + password code
   Customer: Connect to WiFi
   Customer: Visit your portal page
   Customer: Enter password code
   System: Validate and track

3. OPTIONAL ENHANCEMENTS:
   - Setup Guest Network manually
   - Create captive portal redirect
   - Add secondary router later


🔗 HELPFUL LINKS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Router Web Interface:
  http://192.168.1.1

Your WiFi Portal:
  http://localhost:5173/app/admin/wifi

PLDT Support:
  171 (Customer Service)
  https://pldthome.com/support


💡 BOTTOM LINE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Your WiFi Portal is FULLY FUNCTIONAL even without SSH access
to the router. The password generation, validation, and tracking
all work perfectly.

The MAC whitelisting feature is optional and won't work without
SSH, but you don't need it for a great customer WiFi system!

Just use the portal to generate passwords and share them with
customers. Simple and effective! ✅


═══════════════════════════════════════════════════════════════════

Need more help? Check the documentation or ask! 📚

