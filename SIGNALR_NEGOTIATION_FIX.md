# SignalR Negotiation Error Fix

## Error
```
Error Starting SignalR Connection: Dj: Failed to complete negotiation with the server
```

## Root Cause
The negotiation error occurs when:
1. CORS policy blocks the negotiate request
2. Frontend environment variable not set correctly
3. Origin mismatch between frontend and backend

## Fixes Applied

### 1. Updated `.env` File
**File**: `study_hub_app/.env`

Changed from:
```env
VITE_API_BASE_URL = https://3qrbqpcx-5212.asse.devtunnels.ms/api/
```

To:
```env
VITE_API_URL=https://studyhubapi-i0o7.onrender.com/api
```

### 2. Improved CORS Policy
**File**: `Study-Hub/Program.cs`

Updated CORS to be more permissive for all Vercel deployments:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            // Allow localhost
            if (origin.StartsWith("http://localhost") || origin.StartsWith("https://localhost"))
                return true;
            
            // Allow Vercel deployments  
            if (origin.Contains("vercel.app"))
                return true;
            
            // Allow Render deployments
            if (origin.Contains("onrender.com"))
                return true;
            
            // Allow specific production domains
            if (origin == "https://study-hub-app-nu.vercel.app")
                return true;
            
            return false;
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials(); // Required for SignalR
    });
});
```

### 3. Enhanced SignalR Logging
**File**: `study_hub_app/src/services/signalr.service.ts`

Added better debug logging:
- Changed log level from Information to Debug
- Added URL logging when creating connection  
- Added detailed error logging with URL and status code

## Testing Steps

### 1. Restart Frontend Dev Server
```bash
cd study_hub_app
# Stop current server (Ctrl+C)
npm run dev
```

### 2. Check Browser Console
Open browser console and look for:
```
Creating SignalR connection to: https://studyhubapi-i0o7.onrender.com/hubs/notifications
SignalR current state: Disconnected
Starting SignalR connection...
✅ SignalR connected successfully
✅ Joined admins group
```

### 3. If Still Failing, Check for:

**A. CORS Errors**
```
Access to fetch at 'https://studyhubapi-i0o7.onrender.com/hubs/notifications/negotiate' 
from origin 'http://localhost:5173' has been blocked by CORS policy
```

**B. URL Construction Issues**
Look for the URL being logged - it should be:
```
https://studyhubapi-i0o7.onrender.com/hubs/notifications
```

NOT:
```
https://studyhubapi-i0o7.onrender.com/api/hubs/notifications  ❌ (double path)
http://studyhubapi-i0o7.onrender.com/hubs/notifications  ❌ (http instead of https)
```

**C. Network Tab**
Check the negotiate request:
- URL: `https://studyhubapi-i0o7.onrender.com/hubs/notifications/negotiate?negotiateVersion=1`
- Method: POST
- Status: Should be 200
- Response: Should contain `connectionId` and `availableTransports`

## Deployment Steps

### For Local Development
```bash
# Make sure you're using the local .env file
cd study_hub_app
cat .env  # Should show VITE_API_URL

# Restart dev server
npm run dev
```

### For Production (Vercel)
Set environment variable in Vercel dashboard:
```
VITE_API_URL=https://studyhubapi-i0o7.onrender.com/api
```

Then redeploy.

### For Backend (Render.com)
The backend changes require a redeploy. If connected to Git, Render will auto-deploy on push.

## Troubleshooting Commands

### Test SignalR Endpoint
```bash
# Test negotiate endpoint
curl -X POST https://studyhubapi-i0o7.onrender.com/hubs/notifications/negotiate

# Should return JSON with connectionId
```

### Check Backend Logs (Render.com)
1. Go to https://dashboard.render.com
2. Select your service
3. Click "Logs"
4. Look for:
   - CORS errors
   - SignalR connection attempts
   - Hub registration messages

### Frontend Debug
Open browser console and run:
```javascript
// Check what URL is being used
import.meta.env.VITE_API_URL

// Check SignalR state
signalRService.getConnectionState()
signalRService.isConnected()
```

## Next Steps

1. ✅ `.env` file updated
2. ✅ CORS policy improved  
3. ✅ Enhanced logging added
4. ⏳ Restart frontend dev server
5. ⏳ Test connection in browser console
6. ⏳ Deploy if working locally

## Expected Console Output (Success)

```
Creating SignalR connection to: https://studyhubapi-i0o7.onrender.com/hubs/notifications
[HUB] Debug: Starting HubConnection.
[HUB] Debug: Starting connection with transfer format 'Text'.
[HUB] Debug: Sending negotiation request: https://studyhubapi-i0o7.onrender.com/hubs/notifications/negotiate?negotiateVersion=1
[HUB] Debug: Selecting transport 'WebSockets'.
SignalR current state: Disconnected
Starting SignalR connection...
[HUB] Information: WebSocket connected to wss://studyhubapi-i0o7.onrender.com/hubs/notifications
✅ SignalR connected successfully  
[HUB] Debug: Invoking 'JoinAdmins'.
✅ Joined admins group
```

## Common Issues

| Error | Cause | Solution |
|-------|-------|----------|
| "Failed to complete negotiation" | CORS blocking | Updated CORS policy (done) |
| "NetworkError" | Backend not reachable | Check Render.com status |
| "401 Unauthorized" | Missing auth token | SignalR doesn't require auth in current setup |
| "404 Not Found" | Wrong URL | Check environment variable |
| "Timeout" | Backend sleeping (free tier) | Wake up with curl request |

---

**Status**: ✅ Fixes Applied  
**Action Required**: Restart frontend dev server and test

