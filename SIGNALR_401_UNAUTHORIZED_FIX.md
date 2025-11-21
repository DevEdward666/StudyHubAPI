# SignalR 401 Unauthorized Error - Fix

## Error
```
Reconnect attempt failed because of error 'Error: Failed to complete negotiation with the server: Error: : Status code '401''.
```

## Root Cause
The SignalR connection was not including the authentication token when connecting to the hub. The backend `NotificationHub` is marked with `[Authorize]` attribute, which requires a valid JWT token for authentication.

**Problem in code:**
```typescript
this.connection = new signalR.HubConnectionBuilder()
  .withUrl(`${this.baseUrl}/hubs/notifications`, {
    skipNegotiation: false,
    transport: transportOrder,
    withCredentials: false,
    headers: {
      // Add any auth headers if needed  ❌ Empty headers!
    },
    // ...
  })
```

## Solution

### ✅ Added `accessTokenFactory` to SignalR Connection

Updated the SignalR connection builder to include the JWT token from localStorage:

```typescript
// Get auth token from localStorage
const getAuthToken = () => {
  return localStorage.getItem('auth_token');
};

this.connection = new signalR.HubConnectionBuilder()
  .withUrl(`${this.baseUrl}/hubs/notifications`, {
    skipNegotiation: false,
    transport: transportOrder,
    withCredentials: false,
    accessTokenFactory: () => {
      const token = getAuthToken();
      console.log('SignalR: Getting auth token for connection:', token ? 'Token exists' : 'No token found');
      return token || '';
    },
    // Timeout settings optimized for mobile
    timeout: 30000,
    // ...
  })
```

## How It Works

### Authentication Flow

1. **User logs in** → JWT token saved to `localStorage` as `'auth_token'`

2. **SignalR connects** → `accessTokenFactory` is called

3. **Token retrieved** from localStorage

4. **Token sent** to server in Authorization header:
   ```
   Authorization: Bearer <token>
   ```

5. **Server validates** token in NotificationHub

6. **Connection authorized** ✅

### Why `accessTokenFactory` instead of `headers`?

SignalR uses `accessTokenFactory` because:
- ✅ Works with all transport types (WebSockets, LongPolling, SSE)
- ✅ Automatically adds token to every request
- ✅ Token refreshed on each connection attempt
- ✅ Standard SignalR pattern for auth

## Testing

### 1. Check Console Logs

After fix, you should see:
```
Creating SignalR connection to: https://3qrbqpcx-5212.asse.devtunnels.ms/hubs/notifications
SignalR: Getting auth token for connection: Token exists
Client connected: {ConnectionId}, User: {UserId}
User {ConnectionId} joined admins group (Role: Admin)
```

### 2. No More 401 Errors

Before fix:
```
❌ Error: Failed to complete negotiation with the server: Error: : Status code '401'
```

After fix:
```
✅ SignalR setup complete
✅ Joined admins group
```

### 3. Session Notifications Work

When a session expires:
```
🔔 Session ended notification received
📝 Setting session ended data...
🔊 Playing session ended sound...
🚀 Opening session ended modal...
```

## Common Issues & Solutions

### Issue: Still Getting 401

**Possible causes:**
1. Not logged in
2. Token expired
3. Token invalid

**Solution:**
```typescript
// Check if token exists
const token = localStorage.getItem('auth_token');
console.log('Auth token:', token);

// If null or expired, log in again
// Token should be JWT format: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Issue: Token Exists But Still 401

**Possible causes:**
1. Backend not accepting token
2. Token format incorrect
3. Clock skew (token time mismatch)

**Solution:**
1. Check backend logs for auth errors
2. Verify token format (should start with `eyJ`)
3. Check token expiry in JWT debugger (jwt.io)

### Issue: Works Sometimes, Fails Sometimes

**Possible causes:**
1. Token expiring during session
2. Token not being refreshed

**Solution:**
Add token refresh logic:
```typescript
// In api.client.ts response interceptor
if (error.response?.status === 401) {
  // Token expired, refresh or redirect to login
  localStorage.removeItem('auth_token');
  window.location.href = '/login';
}
```

## Backend Configuration

### NotificationHub.cs (Already Configured)

```csharp
[Authorize]  // ✅ Requires authentication
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("Client connected: {ConnectionId}, User: {UserId}", 
            Context.ConnectionId, userId);
        await base.OnConnectedAsync();
    }

    public async Task JoinAdmins()
    {
        // Check if user is admin
        var userRole = Context.User?.FindFirst("role")?.Value;

        if (userRole == "Admin" || userRole == "Super Admin")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
            _logger.LogInformation("User {ConnectionId} joined admins group", 
                Context.ConnectionId);
        }
    }
}
```

### Program.cs (Already Configured)

```csharp
// SignalR Configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
});

// Map SignalR Hub
app.MapHub<Study_Hub.Hubs.NotificationHub>("/hubs/notifications");
```

## Security Best Practices

### ✅ Implemented
- JWT token authentication
- [Authorize] attribute on hub
- Role-based group access
- Connection logging

### ⚠️ Recommendations
1. **Token Expiry**: Set reasonable expiry (e.g., 1-24 hours)
2. **Refresh Tokens**: Implement refresh token mechanism
3. **HTTPS**: Always use HTTPS in production
4. **Token Storage**: Consider using secure httpOnly cookies
5. **Rate Limiting**: Add rate limiting to hub methods

## Files Modified

### `/study_hub_app/src/services/signalr.service.ts`

**Changes:**
1. Added `getAuthToken()` function to retrieve token from localStorage
2. Added `accessTokenFactory` to connection options
3. Added logging to track token retrieval
4. Removed empty `headers` object

**Lines Changed:** ~10 lines

## Verification Checklist

- [ ] No more 401 errors in console
- [ ] "Token exists" message in console
- [ ] "Joined admins group" message appears
- [ ] Session notifications received
- [ ] Modal appears when session expires
- [ ] Sound plays on notification
- [ ] Backend logs show successful connection

## Related Issues

- **SESSION_MODAL_NOT_APPEARING_FIX.md**: NotificationHub implementation
- **SOUND_FIX_QUICK_GUIDE.md**: Audio notification setup
- **SESSION_ENDED_MODAL_IMPLEMENTATION.md**: Modal functionality

## Troubleshooting Commands

### Check if logged in
```javascript
// In browser console
localStorage.getItem('auth_token')
// Should return JWT token or null
```

### Decode JWT token
```javascript
// In browser console
const token = localStorage.getItem('auth_token');
const payload = JSON.parse(atob(token.split('.')[1]));
console.log('Token payload:', payload);
console.log('Expires:', new Date(payload.exp * 1000));
```

### Clear auth and re-login
```javascript
// In browser console
localStorage.removeItem('auth_token');
window.location.href = '/login';
```

## Production Deployment

Before deploying:

1. **Verify token expiry** is set appropriately
2. **Test token refresh** mechanism
3. **Enable HTTPS** for SignalR
4. **Monitor auth failures** in logs
5. **Set up alerts** for 401 errors

---

**Status**: ✅ Fixed
**Date**: November 21, 2025
**Issue**: SignalR 401 Unauthorized during negotiation
**Solution**: Added `accessTokenFactory` to include JWT token
**Result**: SignalR now authenticates successfully and receives notifications

