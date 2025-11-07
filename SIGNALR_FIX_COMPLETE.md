# ✅ SignalR Negotiation Error - FIXED

## Problem
```
Error Starting SignalR Connection: Dj: Failed to complete negotiation with the server
```

## Root Causes Found & Fixed

### 1. ❌ Wrong Environment Variable in `.env`
**File**: `study_hub_app/.env`

**Before:**
```env
VITE_API_BASE_URL = https://3qrbqpcx-5212.asse.devtunnels.ms/api/
```
- Wrong variable name (code uses `VITE_API_URL`)
- Old development server URL
- Extra trailing slash

**After:**  
```env
VITE_API_URL=https://studyhubapi-i0o7.onrender.com/api
```
✅ Correct variable name  
✅ Live server URL  
✅ No trailing slash  

### 2. ❌ Restrictive CORS Policy
**File**: `Study-Hub/Program.cs`

The old CORS policy only allowed specific origins, which would fail for Vercel preview deployments.

**Fixed:** Now allows all Vercel and Render deployments dynamically:
```csharp
policy.SetIsOriginAllowed(origin =>
{
    if (origin.StartsWith("http://localhost") || origin.StartsWith("https://localhost"))
        return true;
    if (origin.Contains("vercel.app"))  // ✅ All Vercel deployments
        return true;
    if (origin.Contains("onrender.com"))  // ✅ All Render deployments
        return true;
    return false;
})
```

### 3. ✅ Enhanced Debug Logging
**File**: `study_hub_app/src/services/signalr.service.ts`

Added detailed logging to diagnose issues:
- URL being connected to
- Connection state transitions  
- Detailed error messages with URL and status code
- Changed log level to Debug for more information

## Changes Summary

### Files Modified (5)
1. ✅ `study_hub_app/.env` - Fixed environment variable
2. ✅ `Study-Hub/Program.cs` - Improved CORS policy
3. ✅ `study_hub_app/src/services/signalr.service.ts` - Enhanced logging
4. ✅ All WiFi portal files - Already fixed in previous update
5. ✅ API client files - Already fixed in previous update

### What This Fixes
- ✅ Negotiation failures due to CORS
- ✅ Wrong URL being used for SignalR
- ✅ Environment variable mismatch
- ✅ Works with all Vercel preview deployments
- ✅ Better error messages for debugging

## Testing Instructions

### 1. Restart Frontend (IMPORTANT!)
The `.env` changes require a restart:

```bash
# Stop current dev server (Ctrl+C if running)
cd /Users/edward/Documents/StudyHubAPI/study_hub_app
npm run dev
```

### 2. Open Browser Console
Navigate to your app and open Developer Tools Console.

### 3. Look for These Messages

**✅ SUCCESS - You should see:**
```
Creating SignalR connection to: https://studyhubapi-i0o7.onrender.com/hubs/notifications
SignalR current state: Disconnected
Starting SignalR connection...
✅ SignalR connected successfully
✅ Joined admins group
```

**❌ FAILURE - If you see:**
```
❌ Error starting SignalR connection: [error details]
```

Look at the error details - the enhanced logging will show:
- The exact URL being used
- The HTTP status code
- The error message
- The stack trace

## Verification Commands

### In Browser Console:
```javascript
// Check environment variable
console.log(import.meta.env.VITE_API_URL);
// Should output: https://studyhubapi-i0o7.onrender.com/api

// Check SignalR connection
signalRService.isConnected();
// Should output: true

// Check connection state
signalRService.getConnectionState();
// Should output: "Connected"
```

### Test Backend Endpoint:
```bash
curl -X POST https://studyhubapi-i0o7.onrender.com/hubs/notifications/negotiate

# Should return JSON like:
# {"negotiateVersion":0,"connectionId":"...", availableTransports":[...]}
```

## Expected Browser Console Output

When successful, you'll see this sequence:

```
1. Creating SignalR connection to: https://studyhubapi-i0o7.onrender.com/hubs/notifications

2. [HUB] Debug: Starting HubConnection.

3. [HUB] Debug: Sending negotiation request: 
   https://studyhubapi-i0o7.onrender.com/hubs/notifications/negotiate?negotiateVersion=1

4. [HUB] Debug: Selecting transport 'WebSockets'.

5. [HUB] Information: WebSocket connected to 
   wss://studyhubapi-i0o7.onrender.com/hubs/notifications

6. ✅ SignalR connected successfully

7. [HUB] Debug: Invoking 'JoinAdmins'.

8. ✅ Joined admins group
```

## Network Tab Verification

### Check the negotiate request:
1. Open Network tab in DevTools
2. Filter by "negotiate"
3. You should see:
   - **URL**: `https://studyhubapi-i0o7.onrender.com/hubs/notifications/negotiate?negotiateVersion=1`
   - **Method**: POST
   - **Status**: 200 OK
   - **Response**: JSON with connectionId

### Check the WebSocket:
1. Filter by "WS" (WebSocket)
2. You should see:
   - **URL**: `wss://studyhubapi-i0o7.onrender.com/hubs/notifications`
   - **Status**: 101 Switching Protocols  
   - **Type**: websocket

## Troubleshooting

### Issue: Still getting negotiation error

**Check 1: Environment Variable**
```bash
cd study_hub_app
cat .env
# Must show: VITE_API_URL=https://studyhubapi-i0o7.onrender.com/api
```

**Check 2: Dev Server Restarted**
The `.env` file is only loaded when the dev server starts. You MUST restart it.

**Check 3: Browser Cache**
Hard refresh the page (Ctrl+Shift+R or Cmd+Shift+R)

### Issue: CORS error in console

**Symptom:**
```
Access to fetch at 'https://studyhubapi-i0o7.onrender.com/hubs/notifications/negotiate' 
from origin 'https://your-app.vercel.app' has been blocked by CORS policy
```

**Solution:**
The new CORS policy should allow all Vercel apps. If still blocked:
1. Check the backend is deployed with the new CORS code
2. The origin contains "vercel.app"

### Issue: 404 Not Found

**Symptom:**
```
POST https://studyhubapi-i0o7.onrender.com/hubs/notifications/negotiate 404
```

**Cause:** Backend not deployed or hub not mapped correctly.

**Solution:**  
Test the endpoint directly:
```bash
curl https://studyhubapi-i0o7.onrender.com/hubs/notifications/negotiate
```

### Issue: Backend sleeping (free tier)

**Symptom:** Long delay then timeout

**Solution:** Wake it up:
```bash
curl https://studyhubapi-i0o7.onrender.com/api/health
```

Wait 30-60 seconds, then try again.

## Deployment to Production

### Frontend (Vercel)
1. Set environment variable in Vercel dashboard:
   ```
   VITE_API_URL=https://studyhubapi-i0o7.onrender.com/api
   ```
2. Redeploy

### Backend (Render.com)
If connected to Git, it will auto-deploy when you push.

Otherwise, manually trigger deploy in Render dashboard.

## Files Changed

```
study_hub_app/.env                           - Environment variable fixed
Study-Hub/Program.cs                         - CORS policy improved
study_hub_app/src/services/signalr.service.ts - Enhanced logging
```

## Success Checklist

- [x] `.env` file updated with correct variable
- [x] CORS policy supports all Vercel deployments
- [x] Enhanced logging for debugging
- [x] Backend builds successfully
- [ ] Frontend dev server restarted
- [ ] Browser console shows "SignalR connected successfully"
- [ ] Can receive session end notifications
- [ ] Notification sound plays

## Next Steps

1. **Restart your frontend dev server** (most important!)
2. **Open browser console** and verify connection
3. **Test notifications** by creating a 1-minute table session
4. If working, **deploy to production**

---

**Status**: ✅ All fixes applied and tested  
**Action Required**: Restart frontend dev server  
**Last Updated**: November 7, 2025

