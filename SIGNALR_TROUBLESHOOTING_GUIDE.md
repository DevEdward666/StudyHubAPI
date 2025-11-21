# SignalR Connection Troubleshooting Guide

## Issue: SignalR Not Working

If you're experiencing issues with SignalR (session notifications not appearing), follow this comprehensive troubleshooting guide.

## Quick Diagnostics

### Step 1: Check Browser Console

Open browser console (F12) and look for these messages:

**✅ Good Signs:**
```
Setting up SignalR for admin...
SignalR: Getting auth token for connection: Token exists
🔌 Starting SignalR connection...
✅ SignalR connected successfully
Joined admins group
✅ SignalR setup complete
```

**❌ Problem Signs:**
```
❌ Failed to setup SignalR: [error message]
Status code '401'
Failed to start the transport 'ServerSentEvents'
SignalR negotiation timeout
SignalR negotiation failed
```

### Step 2: Verify Prerequisites

Run these checks in browser console:

```javascript
// 1. Check if logged in
console.log('Auth token:', localStorage.getItem('auth_token') ? 'EXISTS' : 'MISSING');

// 2. Check if admin
const token = localStorage.getItem('auth_token');
if (token) {
  const payload = JSON.parse(atob(token.split('.')[1]));
  console.log('User role:', payload.role);
  console.log('Token expires:', new Date(payload.exp * 1000));
}

// 3. Check current path
console.log('Current path:', window.location.pathname);
console.log('Is admin path:', window.location.pathname.includes('/admin'));
```

**Expected Results:**
- Auth token: EXISTS
- User role: "Admin" or "Super Admin"
- Token expires: Future date
- Is admin path: true

## Common Issues & Solutions

### Issue 1: "401 Unauthorized"

**Symptoms:**
```
Failed to complete negotiation with the server: Error: : Status code '401'
```

**Cause:** Missing or invalid authentication token

**Solution:**
1. **Log out and log in again**
   ```javascript
   localStorage.removeItem('auth_token');
   window.location.href = '/login';
   ```

2. **Verify token exists**
   ```javascript
   localStorage.getItem('auth_token') // Should not be null
   ```

3. **Check token hasn't expired**
   ```javascript
   const token = localStorage.getItem('auth_token');
   const payload = JSON.parse(atob(token.split('.')[1]));
   console.log('Expired:', Date.now() > payload.exp * 1000);
   ```

### Issue 2: "ServerSentEvents Failed"

**Symptoms:**
```
Failed to start the transport 'ServerSentEvents': Error: EventSource failed to connect
```

**Cause:** Transport compatibility issue (already fixed in latest code)

**Solution:**
1. **Refresh page** (Ctrl+F5 / Cmd+Shift+R)
2. **Clear cache** and reload
3. **Verify you have the latest code** with `withCredentials: true`

### Issue 3: "SignalR Already Initialized"

**Symptoms:**
```
SignalR already initialized, skipping...
```

**Cause:** Component re-rendering, but this is normal behavior

**Solution:**
- ✅ This is **expected** - SignalR prevents duplicate connections
- No action needed if you see "✅ SignalR setup complete" before this message

### Issue 4: Backend Not Running

**Symptoms:**
```
SignalR negotiation timeout - backend may not be available
SignalR negotiation failed - check if backend hub is running
```

**Cause:** Backend server is not running or not accessible

**Solution:**
1. **Start backend:**
   ```bash
   cd Study-Hub
   dotnet run
   ```

2. **Check backend URL:**
   ```javascript
   // In browser console
   console.log('API URL:', import.meta.env.VITE_API_URL);
   ```

3. **Test backend manually:**
   - Open: `https://3qrbqpcx-5212.asse.devtunnels.ms/api/health` (or your backend URL)
   - Should return a success response

### Issue 5: CORS Errors

**Symptoms:**
```
Access to XMLHttpRequest has been blocked by CORS policy
```

**Cause:** Backend CORS not configured for your frontend URL

**Solution:**
1. **Check backend CORS configuration** in `Program.cs`
2. **Verify your frontend URL is allowed**
3. **Restart backend** after CORS changes

### Issue 6: Not Admin User

**Symptoms:**
- No console logs about SignalR
- "SignalR already initialized" appears but no connection

**Cause:** You're logged in as a Customer, not Admin

**Solution:**
1. **Check user role:**
   ```javascript
   const token = localStorage.getItem('auth_token');
   const payload = JSON.parse(atob(token.split('.')[1]));
   console.log('Role:', payload.role);
   ```

2. **Log in with admin account**

### Issue 7: Not on Admin Page

**Symptoms:**
- Console shows: "Leaving admin area, stopping SignalR..."

**Cause:** You navigated away from admin pages

**Solution:**
- Navigate to any admin page (e.g., `/app/admin/dashboard`)
- SignalR will automatically reconnect

## Advanced Debugging

### Enable SignalR Debug Logging

The SignalR connection already has `LogLevel.Debug` enabled. Check console for detailed logs:

```
[timestamp] Information: WebSocket connected to wss://...
[timestamp] Information: LongPolling connected
[timestamp] Debug: Sending message...
```

### Test SignalR Connection Manually

```javascript
// In browser console
import { signalRService } from './services/signalr.service';

// Check connection state
console.log('SignalR state:', signalRService.connection?.state);

// Try to start manually
await signalRService.start();

// Check if joined admins group
console.log('Check backend logs for "joined admins group" message');
```

### Backend Logs to Check

**Backend console should show:**
```
Client connected: {ConnectionId}, User: {UserId}
User {ConnectionId} joined admins group (Role: Admin)
```

**If missing:**
- Backend hub not receiving connection
- Authentication failing on backend
- User not recognized as admin

## Testing Checklist

Use this checklist to verify everything is working:

- [ ] Backend is running (`dotnet run`)
- [ ] Frontend is running (`npm run dev`)
- [ ] Logged in as Admin user
- [ ] On admin page (e.g., `/app/admin/dashboard`)
- [ ] Browser console shows no errors
- [ ] Console shows "SignalR setup complete"
- [ ] Console shows "Joined admins group"
- [ ] Backend logs show connection
- [ ] Test notification works:
  - [ ] Create subscription (0.02 hours)
  - [ ] Start session
  - [ ] Wait 2-3 minutes
  - [ ] Modal appears with sound

## Network Diagnostics

### Check Network Tab

1. Open DevTools → Network tab
2. Filter: WS (WebSockets) or XHR
3. Look for:
   - `/hubs/notifications/negotiate` (Status 200)
   - WebSocket connection or Long polling requests

**Expected:**
- Negotiate: 200 OK
- WebSocket: Status 101 (Switching Protocols)

**Problem:**
- Negotiate: 401 (Token issue)
- Negotiate: 404 (Backend not running)
- Negotiate: 500 (Backend error)

### Test Endpoint Manually

```bash
# Test negotiate endpoint
curl -X POST \
  https://3qrbqpcx-5212.asse.devtunnels.ms/hubs/notifications/negotiate \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json"
```

**Expected:** JSON response with connection info

## Configuration Verification

### Frontend Configuration

**File:** `.env.local`
```env
VITE_API_BASE_URL=https://3qrbqpcx-5212.asse.devtunnels.ms/api/
VITE_API_URL=https://3qrbqpcx-5212.asse.devtunnels.ms/api
```

**File:** `signalr.service.ts`
- ✅ `withCredentials: true`
- ✅ `accessTokenFactory` defined
- ✅ Transport: WebSockets | LongPolling (no ServerSentEvents)

### Backend Configuration

**File:** `Program.cs`
- ✅ SignalR added: `AddSignalR()`
- ✅ CORS configured: `AllowCredentials()`
- ✅ Hub mapped: `MapHub<NotificationHub>("/hubs/notifications")`

**File:** `NotificationHub.cs`
- ✅ `[Authorize]` attribute
- ✅ `JoinAdmins()` method exists
- ✅ Role checking implemented

## Emergency Workarounds

### Force LongPolling (Debug Only)

If WebSockets keep failing:

```typescript
// In signalr.service.ts, temporarily change:
transport: signalR.HttpTransportType.LongPolling
```

This forces the most compatible transport.

### Disable SignalR (Last Resort)

If SignalR is blocking the app:

```typescript
// In TabsLayout.tsx, comment out SignalR setup:
// setupSignalR();
```

**Note:** Session notifications won't work, but app will function.

## Getting Help

If still not working after all troubleshooting:

1. **Collect information:**
   - Browser console logs (full)
   - Backend console logs
   - Network tab screenshot
   - Your role (from token)
   - Backend URL

2. **Share:**
   - Console error messages
   - Backend logs
   - Steps to reproduce
   - What you've already tried

3. **Check:**
   - Are you using latest code?
   - Did you refresh after code changes?
   - Did you restart backend?

---

**Last Updated:** November 21, 2025
**Status:** Complete troubleshooting guide
**Related Files:**
- `SIGNALR_401_UNAUTHORIZED_FIX.md`
- `SIGNALR_TRANSPORT_ERROR_FIX.md`
- `SESSION_MODAL_NOT_APPEARING_FIX.md`

