# Quick Start - WiFi Captive Portal

## 🚀 Get Started in 3 Steps

### Step 1: Start the Backend
```bash
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet run
```

Wait for the message: `Now listening on: http://localhost:5143`

### Step 2: Open the Portal
Double-click or open in browser:
```
/Users/edward/Documents/StudyHubAPI/wifi-portal.html
```

### Step 3: Generate Your First Password!
1. Select duration (e.g., "1 hour")
2. Click "Get WiFi Password"
3. Copy the password shown
4. Done! ✅

---

## 🧪 Test Everything Works

Run the automated test:
```bash
cd /Users/edward/Documents/StudyHubAPI
./test-wifi-system.sh
```

This will test all API endpoints and confirm everything is working.

---

## 📖 API Examples

### Generate Password
```bash
curl -X POST http://localhost:5143/api/wifi/request \
  -H "Content-Type: application/json" \
  -d '{"validMinutes": 60, "note": "My Device", "passwordLength": 8}'
```

### Validate Password
```bash
curl "http://localhost:5143/api/wifi/validate?password=YOUR_PASSWORD"
```

### Redeem Password (Mark as Used)
```bash
curl -X POST "http://localhost:5143/api/wifi/redeem?password=YOUR_PASSWORD"
```

---

## ⚙️ Router Integration (Optional)

### If You Have SSH Access to Your Router:

1. **Upload scripts to router:**
   ```bash
   scp RouterScripts/*.sh admin@192.168.1.1:/usr/local/bin/
   ssh admin@192.168.1.1 "chmod +x /usr/local/bin/*.sh"
   ```

2. **Update router credentials in:**
   ```
   Study-Hub/appsettings.json
   ```
   Change:
   - Host: Your router IP
   - Username: Your admin username
   - Password: Your admin password

3. **Test router integration:**
   ```bash
   curl -X POST http://localhost:5143/api/router/whitelist \
     -H "Content-Type: application/json" \
     -d '{"macAddress": "00:11:22:33:44:55", "ttlSeconds": 3600}'
   ```

### If You DON'T Have SSH (Most PLDT Routers):

**Option A: Use for Password Generation Only**
- System generates secure passwords
- Manually configure your router to accept them
- Or use a shared WiFi password and generate access codes

**Option B: Raspberry Pi Captive Portal**
- See full guide in `WIFI_SETUP_GUIDE.md`
- Best solution for full automation
- Works with any router

---

## 🎯 Common Scenarios

### Scenario 1: Coffee Shop / Study Hub
1. Customer connects to open WiFi (or asks for password)
2. They're redirected to portal page
3. They request access for desired duration
4. System generates password
5. They connect using that password

### Scenario 2: Event / Conference
1. Print QR code linking to portal
2. Attendees scan QR code
3. Portal generates time-limited password
4. Access expires after event

### Scenario 3: Guest WiFi in Office
1. Reception staff opens portal
2. Generates password for visitor
3. Visitor gets access for specified time
4. Password auto-expires when visitor leaves

---

## 📱 What You Can Do Now

✅ **Generate WiFi Passwords** - Instantly create secure, time-limited passwords  
✅ **Validate Passwords** - Check if a password is still valid  
✅ **Set Expiration** - From 30 minutes to 12 hours  
✅ **One-Time Use** - Passwords can be redeemed only once  
✅ **Track Usage** - Optional notes for each password  
✅ **Auto Cleanup** - Expired passwords removed automatically  

---

## 🔧 Customization

### Change Password Length
In the portal HTML, update the request:
```javascript
passwordLength: 8  // Change to 6, 10, 12, etc.
```

### Change Cleanup Frequency
In `Service/Background/WifiCleanupService.cs`:
```csharp
private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);  // Change frequency
```

### Change API URL
In `wifi-portal.html`:
```javascript
const API_BASE = 'http://localhost:5143/api';  // Change to your production URL
```

---

## 🆘 Troubleshooting

### Backend won't start?
```bash
# Check database connection
cd Study-Hub
dotnet ef database update

# Check for port conflicts
lsof -i :5143
```

### Portal can't connect to API?
1. Make sure backend is running
2. Check browser console for CORS errors
3. Verify API_BASE URL in wifi-portal.html

### Passwords not expiring?
- Check WifiCleanupService is running (should log every 5 minutes)
- Verify UTC timestamps in database

---

## 📚 More Information

- **Full Setup Guide:** `WIFI_SETUP_GUIDE.md`
- **Implementation Details:** `WIFI_SYSTEM_COMPLETE.md`
- **Test Script:** `test-wifi-system.sh`

---

## 🎉 That's It!

Your WiFi captive portal system is ready to use. Start generating passwords and managing WiFi access like Starbucks does!

For questions or advanced configuration, check the detailed guides in the repository.

**Happy WiFi Managing! 📶**

