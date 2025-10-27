# WiFi Captive Portal System - Setup Guide

## Overview
This system provides a Starbucks-style WiFi access portal where users can request temporary WiFi passwords. The system generates random passwords, stores them with expiration times, and can integrate with your router for automatic access control.

## Features
- Generate random WiFi passwords with configurable expiration
- Validate passwords
- Redeem passwords (one-time use)
- Automatic cleanup of expired passwords
- Router integration via SSH for MAC address whitelisting
- Beautiful web portal interface

## Components

### Backend (C# .NET)
1. **WifiAccess Model** - Stores password credentials with expiry
2. **WifiService** - Manages password generation and validation
3. **SshRouterManager** - Connects to router via SSH for access control
4. **WifiCleanupService** - Background service that removes expired passwords
5. **WifiController** - API endpoints for the system
6. **RouterMgmtController** - API for router management

### Frontend
- `wifi-portal.html` - Beautiful captive portal page

## Installation Steps

### 1. Backend Setup

The backend has been fully implemented. All files are created and the database migration has been applied.

#### Configuration (appsettings.json)

Update your router credentials in `Study-Hub/appsettings.json`:

```json
"Router": {
  "Host": "192.168.1.1",        // Your router's IP address
  "Port": 22,                    // SSH port
  "Username": "admin",           // Router admin username
  "Password": "your_password",   // Router admin password
  "AddScriptPath": "/usr/local/bin/add_whitelist.sh",
  "RemoveScriptPath": "/usr/local/bin/remove_whitelist.sh"
}
```

### 2. Router Setup (For PLDT Router)

#### Option A: SSH Access Method (Recommended if your router supports SSH)

**Step 1: Enable SSH on your PLDT router**
1. Log into router admin panel (usually http://192.168.1.1)
2. Look for "Remote Management" or "SSH" settings
3. Enable SSH access
4. Note: Most consumer PLDT routers don't support SSH by default

**Step 2: Upload scripts to router**
Copy the scripts from `RouterScripts/` folder to your router:
```bash
# Upload via SCP (if SSH is available)
scp RouterScripts/add_whitelist.sh admin@192.168.1.1:/usr/local/bin/
scp RouterScripts/remove_whitelist.sh admin@192.168.1.1:/usr/local/bin/

# Make them executable
ssh admin@192.168.1.1 "chmod +x /usr/local/bin/add_whitelist.sh /usr/local/bin/remove_whitelist.sh"
```

#### Option B: Alternative Methods (For routers without SSH)

**1. MAC Address Filtering via Web Interface**
- Most PLDT routers support MAC filtering via web interface
- You'll need to implement a web scraping solution or use router API (if available)
- This is more complex and router-specific

**2. Use a Secondary Router/Access Point**
- Set up a Raspberry Pi or old computer as a captive portal gateway
- Install software like:
  - **NoDogSplash** (lightweight captive portal)
  - **CoovaChilli** (full-featured hotspot solution)
  - **pfSense** (router OS with captive portal built-in)

**3. Simple Access Point with Captive Portal (Easiest)**
If your PLDT router doesn't support SSH, here's the simplest approach:

1. **Set up a Raspberry Pi as WiFi Access Point:**
   ```bash
   # Install hostapd and dnsmasq
   sudo apt-get install hostapd dnsmasq iptables-persistent
   
   # Configure hostapd for WiFi AP
   # Configure dnsmasq for DHCP and DNS
   # Set up iptables rules for captive portal
   ```

2. **Use your backend API:**
   - The portal page validates passwords via API
   - On successful validation, add MAC to iptables ACCEPT rule
   - Schedule removal after expiry

### 3. API Endpoints

Once the backend is running, you'll have these endpoints:

#### Request WiFi Access
```http
POST http://localhost:5143/api/wifi/request
Content-Type: application/json

{
  "validMinutes": 60,
  "note": "John's iPhone",
  "passwordLength": 8
}

Response:
{
  "password": "Abc3Xy9Z",
  "expiresAtUtc": "2025-10-27T14:30:00Z",
  "message": "WiFi Password generated. Valid for 60 minutes."
}
```

#### Validate Password
```http
GET http://localhost:5143/api/wifi/validate?password=Abc3Xy9Z

Response:
{
  "valid": true,
  "redeemed": false,
  "expiresAtUtc": "2025-10-27T14:30:00Z",
  "message": "Password is valid"
}
```

#### Redeem Password (One-time use)
```http
POST http://localhost:5143/api/wifi/redeem?password=Abc3Xy9Z

Response:
{
  "redeemed": true,
  "message": "Password redeemed successfully"
}
```

#### Add MAC to Whitelist (Router Management)
```http
POST http://localhost:5143/api/router/whitelist
Content-Type: application/json

{
  "macAddress": "00:11:22:33:44:55",
  "ttlSeconds": 3600
}
```

#### Remove MAC from Whitelist
```http
DELETE http://localhost:5143/api/router/whitelist/00:11:22:33:44:55
```

### 4. Frontend Setup

1. Open `wifi-portal.html` in a text editor
2. Update the API_BASE URL to match your backend:
   ```javascript
   const API_BASE = 'http://localhost:5143/api'; // Change to your server URL
   ```
3. Host the HTML file:
   - Simple: Open directly in browser for testing
   - Production: Host on your web server or serve via your .NET app

### 5. Testing Without Router Integration

You can test the system without router integration:

1. **Start the backend:**
   ```bash
   cd Study-Hub
   dotnet run
   ```

2. **Open wifi-portal.html in browser**

3. **Test the flow:**
   - Click "Get WiFi Password"
   - Select duration (e.g., 1 hour)
   - Click the button to generate password
   - Copy the generated password
   - Use "Validate Password" section to check if it's valid
   - Try validating after the expiry time (or after redemption)

### 6. Production Deployment

#### For Raspberry Pi Captive Portal Solution:

1. **Install prerequisites:**
   ```bash
   sudo apt-get update
   sudo apt-get install hostapd dnsmasq iptables-persistent nginx
   ```

2. **Configure WiFi Access Point:**
   ```bash
   # Configure /etc/hostapd/hostapd.conf
   interface=wlan0
   driver=nl80211
   ssid=StudyHub-WiFi
   hw_mode=g
   channel=7
   wmm_enabled=0
   macaddr_acl=0
   auth_algs=1
   ignore_broadcast_ssid=0
   wpa=2
   wpa_passphrase=YourNetworkPassword
   wpa_key_mgmt=WPA-PSK
   wpa_pairwise=TKIP
   rsn_pairwise=CCMP
   ```

3. **Configure DHCP and DNS:**
   ```bash
   # /etc/dnsmasq.conf
   interface=wlan0
   dhcp-range=192.168.4.2,192.168.4.20,255.255.255.0,24h
   address=/#/192.168.4.1  # Redirect all DNS to captive portal
   ```

4. **Set up iptables for captive portal:**
   ```bash
   # Redirect HTTP to portal
   sudo iptables -t nat -A PREROUTING -i wlan0 -p tcp --dport 80 -j DNAT --to-destination 192.168.4.1:80
   
   # Default deny all forwarding
   sudo iptables -A FORWARD -i wlan0 -j DROP
   
   # Save rules
   sudo netfilter-persistent save
   ```

5. **Host portal page with nginx:**
   ```bash
   sudo cp wifi-portal.html /var/www/html/index.html
   sudo systemctl restart nginx
   ```

6. **Integration script** (Python example for adding MACs):
   ```python
   #!/usr/bin/env python3
   import subprocess
   import sys
   
   def add_whitelist(mac, ttl):
       # Add iptables rule
       subprocess.run(['sudo', 'iptables', '-I', 'FORWARD', '1', 
                      '-m', 'mac', '--mac-source', mac, '-j', 'ACCEPT'])
       
       # Schedule removal
       subprocess.Popen(['bash', '-c', 
                        f'sleep {ttl} && sudo iptables -D FORWARD -m mac --mac-source {mac} -j ACCEPT'])
   
   if __name__ == '__main__':
       add_whitelist(sys.argv[1], int(sys.argv[2]))
   ```

## Security Considerations

1. **Use HTTPS in production** - Update API_BASE to use https://
2. **Secure router credentials** - Use environment variables, not appsettings.json
3. **Rate limiting** - Add rate limiting to prevent password generation abuse
4. **MAC spoofing** - Be aware that MAC addresses can be spoofed
5. **Firewall rules** - Ensure only whitelisted MACs can access internet

## Testing Checklist

- [ ] Backend builds and runs without errors
- [ ] Database migration applied successfully
- [ ] Can request WiFi password via API
- [ ] Can validate password via API
- [ ] Can redeem password (becomes invalid after)
- [ ] Expired passwords are automatically cleaned up
- [ ] Portal page loads and connects to API
- [ ] Portal displays generated password
- [ ] Router whitelist script works (if using SSH method)

## Troubleshooting

### Backend won't start
- Check database connection string
- Ensure migrations are applied: `dotnet ef database update`
- Check for port conflicts (default: 5143)

### Can't connect to router
- Verify router IP, username, password
- Test SSH manually: `ssh admin@192.168.1.1`
- Check if router supports SSH
- Consider alternative methods if SSH unavailable

### Portal can't reach API
- Check CORS settings in Program.cs (should allow your domain)
- Verify API_BASE URL in wifi-portal.html
- Check browser console for errors
- Test API directly with curl or Postman

### Passwords not expiring
- Check WifiCleanupService is registered and running
- Check logs for cleanup service errors
- Verify database time zone settings (should use UTC)

## Next Steps

1. Test the basic functionality without router integration
2. If you have a compatible router, set up SSH access
3. For PLDT routers without SSH, consider the Raspberry Pi solution
4. Add authentication for admin functions
5. Implement usage analytics and reporting
6. Add email/SMS verification for password requests
7. Integrate with payment system for paid access

## Need Help?

If your PLDT router doesn't support SSH or advanced features, I recommend:
1. Use the Raspberry Pi captive portal solution
2. Purchase a compatible router (TP-Link, Ubiquiti, MikroTik)
3. Use the system for password generation only (manual MAC filtering)

The system is now fully functional for password generation and validation. The router integration is optional and depends on your hardware capabilities.

