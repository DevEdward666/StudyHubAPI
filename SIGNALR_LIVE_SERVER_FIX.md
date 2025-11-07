# SignalR Live Server Fix

## Problem
SignalR was not working on the live server (https://studyhubapi-i0o7.onrender.com) while it worked locally.

## Root Causes

1. **Environment Variable Mismatch**: Frontend `.env` had `VITE_API_BASE_URL` but code was looking for `VITE_API_URL`
2. **WebSocket Transport Configuration**: Missing transport fallback options for production environments
3. **WebSocket Middleware**: Not properly configured before routing in Program.cs
4. **Reconnection Strategy**: Default reconnection didn't have proper retry intervals

## Changes Made

### Frontend Changes (`study_hub_app`)

#### 1. `.env` File
```env
# Before
VITE_API_BASE_URL = https://3qrbqpcx-5212.asse.devtunnels.ms/api/

# After
VITE_API_URL=https://studyhubapi-i0o7.onrender.com/api
```

#### 2. `signalr.service.ts`

**Updated URL Construction:**
```typescript
const apiBaseUrl = import.meta.env.VITE_API_URL || "http://localhost:5212/api";
const baseUrl = apiBaseUrl.endsWith("/api") 
  ? apiBaseUrl.substring(0, apiBaseUrl.length - 4) 
  : apiBaseUrl.replace("/api/", "");
```

**Enhanced Connection Configuration:**
```typescript
this.connection = new signalR.HubConnectionBuilder()
  .withUrl(`${this.baseUrl}/hubs/notifications`, {
    skipNegotiation: false,
    transport: signalR.HttpTransportType.WebSockets | 
               signalR.HttpTransportType.ServerSentEvents | 
               signalR.HttpTransportType.LongPolling,
    withCredentials: false,
  })
  .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
  .configureLogging(signalR.LogLevel.Information)
  .build();
```

**Key Improvements:**
- Multiple transport fallbacks (WebSockets → SSE → Long Polling)
- Custom reconnection intervals: 0ms, 2s, 5s, 10s, 30s
- Better logging for debugging

### Backend Changes (`Study-Hub`)

#### 1. `Program.cs` - SignalR Configuration
```csharp
// SignalR Configuration with production-ready settings
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});
```

#### 2. `Program.cs` - WebSocket Middleware
```csharp
// Enable WebSockets for SignalR (before CORS)
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
});
```

**Middleware Order:**
1. HttpsRedirection
2. **WebSockets** ← Added
3. CORS
4. Authentication
5. Authorization
6. Controllers
7. SignalR Hubs

## Render.com Specific Configuration

### WebSocket Support
Render.com supports WebSockets by default, but requires:

1. **HTTPS/WSS**: Use secure WebSocket connections (wss://)
2. **Keep-Alive**: Maintain connection with regular heartbeats
3. **Transport Fallback**: Support for SSE and Long Polling as backup

### Environment Variables on Render.com
Make sure these are set in Render.com dashboard:
- Connection strings
- JWT secrets
- Any other sensitive configuration

## Testing Steps

### Local Testing
```bash
# Terminal 1: Start backend
cd Study-Hub
dotnet run

# Terminal 2: Start frontend
cd study_hub_app
npm run dev
```

### Production Testing
1. Deploy backend to Render.com
2. Deploy frontend to Vercel
3. Update frontend `.env` or environment variables:
   ```
   VITE_API_URL=https://studyhubapi-i0o7.onrender.com/api
   ```
4. Test SignalR connection in browser console:
   ```javascript
   // Check connection state
   console.log(signalRService.isConnected());
   
   // View logs
   // SignalR logs will appear in console with "SignalR" prefix
   ```

## Troubleshooting

### Check SignalR Connection State
In browser console:
```javascript
// Import service (if using dev tools)
import { signalRService } from './services/signalr.service';

// Check state
console.log('Connected:', signalRService.isConnected());
console.log('State:', signalRService.getConnectionState());
```

### Common Issues

#### 1. "Cannot start a HubConnection that is not in 'Disconnected' state"
**Solution**: The service now handles this automatically by checking state before starting.

#### 2. WebSocket Connection Failed
**Solution**: Transport fallback will automatically use SSE or Long Polling.

#### 3. CORS Errors
**Verify CORS policy includes your frontend URL:**
```csharp
policy.WithOrigins(
    "http://localhost:5173",
    "https://localhost:5173",
    "https://study-hub-app-nu.vercel.app",  // ✓ Vercel frontend
    "https://studyhubapi-i0o7.onrender.com"  // ✓ Render backend
)
```

#### 4. Connection Drops on Render.com
**Cause**: Render.com may have connection timeouts or the free tier may sleep.
**Solution**: 
- Keep-alive intervals are set (15s client, 120s websocket)
- Auto-reconnect is enabled with progressive delays
- Consider upgrading Render.com plan if on free tier

### Backend Logs (Render.com)
Check Render.com logs for:
```
SignalR connection started
Client connected: [connectionId]
Client added to admins group
```

### Frontend Logs (Browser Console)
Look for:
```
SignalR connected successfully
Joined admins group
Session ended notification received: [data]
```

## Performance Considerations

### Render.com Free Tier Limitations
- **Cold Starts**: Service sleeps after 15 minutes of inactivity
- **Reconnection**: Frontend will auto-reconnect when service wakes up
- **First Request Delay**: May take 30-60 seconds to wake up

### Recommended for Production
- Upgrade to paid Render.com plan to avoid sleep
- Use dedicated WebSocket server if needed
- Monitor connection health with logging

## File Changes Summary

### Modified Files
1. `/study_hub_app/.env` - Updated environment variable
2. `/study_hub_app/src/services/signalr.service.ts` - Enhanced connection handling
3. `/Study-Hub/Program.cs` - Added WebSocket middleware and SignalR config

### No Changes Required
- `NotificationHub.cs` - Already correct
- `SessionExpiryChecker.cs` - Already correct
- CORS configuration - Already includes all needed origins

## Deployment Checklist

- [x] Update frontend `.env` with production API URL
- [x] Configure SignalR transport fallbacks
- [x] Add WebSocket middleware
- [x] Configure reconnection strategy
- [x] Verify CORS includes frontend URL
- [x] Test SignalR negotiate endpoint (✅ Working!)
- [ ] Deploy backend to Render.com (appears to be deployed)
- [ ] Deploy frontend to Vercel (rebuild with new env)
- [ ] Test SignalR connection in production browser
- [ ] Verify session end notifications work
- [ ] Test reconnection after backend restart

## Live Server Test Results

✅ **Backend is reachable** - HTTP 404 (expected for root)
✅ **SignalR negotiate endpoint working** - Returns connectionId and WebSocket transport
✅ **WebSocket support available** - Server offers WebSocket, SSE, and Long Polling

**Test Command:**
```bash
./test-signalr-live.sh
```

## Next Steps

1. **Commit Changes**:
   ```bash
   git add .
   git commit -m "Fix SignalR for live server with WebSocket support"
   git push
   ```

2. **Deploy Backend** (Render.com will auto-deploy if connected to Git)

3. **Deploy Frontend**:
   ```bash
   cd study_hub_app
   # Make sure VITE_API_URL is set in Vercel dashboard
   git push
   ```

4. **Verify** - Open browser console and check for:
   - "SignalR connected successfully"
   - "Joined admins group"

## Success Criteria

✅ SignalR connects on page load  
✅ Connection survives page navigation  
✅ Auto-reconnects after network issues  
✅ Session end notifications appear  
✅ Notification sound plays  
✅ Toast messages display correctly  

---

**Last Updated**: 2025-11-07  
**Status**: Ready for deployment

