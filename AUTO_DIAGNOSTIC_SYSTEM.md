# Auto-Diagnostic System for Expired Sessions

## Overview
An automatic diagnostic system that runs when sessions expire to help troubleshoot why the session ended modal might not appear.

## Components

### 1. Backend Logging (SessionExpiryChecker.cs)

**Enhanced Logging When Session Expires:**
```csharp
_logger.LogInformation(
    "📡 Sending SessionEnded notification to 'admins' group - Table {TableNumber}, User: {UserName}", 
    session.Table?.TableNumber, 
    session.User?.Name ?? "Guest");

await hubContext.Clients.Group("admins").SendAsync("SessionEnded", signalRPayload, ct);

_logger.LogInformation("✅ SessionEnded notification sent successfully");
```

**Backend Console Will Show:**
```
[15:30:00] Subscription session abc-123 ended for table 5. User: John Doe, Final remaining hours: 0h
[15:30:00] 📡 Sending SessionEnded notification to 'admins' group - Table 5, User: John Doe
[15:30:00] ✅ SessionEnded notification sent successfully
```

### 2. Frontend Auto-Diagnostics (TabsLayout.tsx)

**Features:**
- Automatically runs diagnostics when SignalR connection fails
- Exposes `window.runSignalRDiagnostics()` for manual testing
- Loads diagnostic script from `/signalr-diagnostic.js`
- Provides inline fallback diagnostics if script fails to load

**When SignalR Fails to Connect:**
```
❌ Failed to setup SignalR: [error]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
⚠️  SIGNALR CONNECTION FAILED
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Session notifications will NOT work until this is fixed.
Running diagnostics to help identify the issue...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[Diagnostics output follows...]
```

### 3. Diagnostic Script (signalr-diagnostic.js)

**Auto-runs in these scenarios:**
1. When explicitly called: `window.runSignalRDiagnostics()`
2. When SignalR connection fails
3. When script is loaded in development mode

**Checks Performed:**
1. ✅ Authentication token exists and is valid
2. ✅ User role is Admin or Super Admin
3. ✅ Token hasn't expired
4. ✅ User is on admin page
5. ✅ Browser capabilities (WebSocket, Audio, etc.)
6. ✅ Network connection status
7. ✅ Environment configuration

## How It Works

### Scenario 1: Session Expires, Modal Appears ✅

**Backend:**
```
[15:30:00] Subscription session ended for table 5
[15:30:00] 📡 Sending SessionEnded notification to 'admins' group
[15:30:00] ✅ SessionEnded notification sent successfully
```

**Frontend:**
```
🔔 Session ended notification received: {tableNumber: "5", ...}
📝 Setting session ended data...
🔊 Playing session ended sound...
✅ Session ended sound played successfully
🚀 Opening session ended modal...
```

**Result:** Modal appears with sound ✅

### Scenario 2: Session Expires, No Modal ❌

**Backend:**
```
[15:30:00] Subscription session ended for table 5
[15:30:00] 📡 Sending SessionEnded notification to 'admins' group
[15:30:00] ✅ SessionEnded notification sent successfully
```

**Frontend:** (No notification received)
```
[Nothing in console about session ended]
```

**Diagnostics Triggered:** (if SignalR failed to connect)
```
❌ Failed to setup SignalR: [error]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
⚠️  SIGNALR CONNECTION FAILED
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Running diagnostics to help identify the issue...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔍 Starting SignalR Diagnostics...

1️⃣ AUTHENTICATION CHECK
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ Auth token exists
   User ID: abc-123
   Role: Admin
   Expires: Nov 21, 2025 6:00 PM
   Is Expired: ❌ NO

2️⃣ LOCATION CHECK
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Current URL: https://...
   Current Path: /app/admin/dashboard
   Is Admin Path: ✅ YES

[... more checks ...]

9️⃣ DIAGNOSTIC SUMMARY
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Issues found:
   ❌ [Specific issue identified]

Suggestions:
   • [Specific solution]
```

## Manual Testing

### Test 1: Run Diagnostics Manually

In browser console:
```javascript
window.runSignalRDiagnostics()
```

### Test 2: Simulate Session Expiry

1. Create subscription with 0.02 hours (1.2 minutes)
2. Start session
3. Wait 2-3 minutes
4. **Check backend logs** for:
   ```
   📡 Sending SessionEnded notification to 'admins' group
   ✅ SessionEnded notification sent successfully
   ```

5. **Check frontend console** for:
   ```
   🔔 Session ended notification received
   ```

6. **If frontend doesn't receive notification**, diagnostics will auto-run

### Test 3: Force SignalR Failure

Temporarily break SignalR to test auto-diagnostics:

```typescript
// In signalr.service.ts, temporarily change:
baseUrl: string = "http://invalid-url";
```

Refresh page and check console for auto-diagnostics.

## Diagnostic Output

### Example: All Checks Pass ✅

```
9️⃣ DIAGNOSTIC SUMMARY
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ All checks passed!

If SignalR still not working:
1. Refresh the page (Ctrl+F5 / Cmd+Shift+R)
2. Check backend is running
3. Check browser console for error messages
4. See SIGNALR_TROUBLESHOOTING_GUIDE.md for more help
```

### Example: Issues Found ❌

```
9️⃣ DIAGNOSTIC SUMMARY
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Issues found:
   ❌ Token expired
   ⚠️ Not on admin page

Suggestions:
   • Log out and log in again
   • Navigate to /app/admin/dashboard
```

## Integration Points

### Frontend (TabsLayout.tsx)
```typescript
// 1. Load diagnostic script when needed
const runDiagnostics = () => {
  const script = document.createElement('script');
  script.src = '/signalr-diagnostic.js';
  document.head.appendChild(script);
};

// 2. Auto-run on SignalR failure
catch (error) {
  console.error('❌ Failed to setup SignalR:', error);
  setTimeout(() => {
    runDiagnostics();
  }, 1000);
}

// 3. Expose to window for manual use
window.runSignalRDiagnostics = runDiagnostics;
```

### Backend (SessionExpiryChecker.cs)
```csharp
// Enhanced logging
_logger.LogInformation(
    "📡 Sending SessionEnded notification to 'admins' group - Table {TableNumber}", 
    tableNumber);

await hubContext.Clients.Group("admins").SendAsync("SessionEnded", payload, ct);

_logger.LogInformation("✅ SessionEnded notification sent successfully");
```

## Files Modified

1. **`TabsLayout.tsx`**
   - Added `runDiagnostics()` function
   - Added auto-run on SignalR failure
   - Exposed diagnostics to window
   - Added inline fallback diagnostics

2. **`SessionExpiryChecker.cs`**
   - Added detailed logging before sending notification
   - Added success confirmation after sending
   - Better variable naming for clarity

3. **`signalr-diagnostic.js`**
   - Wrapped in `runSignalRDiagnostics()` function
   - Exported to window scope
   - Auto-runs when loaded
   - Accepts `autoTriggered` parameter

## Benefits

### For Developers
- ✅ Automatic issue detection
- ✅ Detailed diagnostic information
- ✅ Quick troubleshooting
- ✅ Easy to run manually

### For Users
- ✅ Transparent debugging process
- ✅ Clear error messages
- ✅ Actionable suggestions
- ✅ Self-service troubleshooting

### For Debugging
- ✅ Backend logs show notification sent
- ✅ Frontend logs show notification received (or not)
- ✅ Auto-diagnostics identify the gap
- ✅ Comprehensive health checks

## Common Issues Detected

### 1. Token Expired
```
❌ Token expired

Suggestion: Log out and log in again
```

### 2. Not Admin
```
❌ Not logged in as admin

Suggestion: Log in with an admin account
```

### 3. Wrong Page
```
⚠️ Not on admin page

Suggestion: Navigate to /app/admin/dashboard
```

### 4. Network Issue
```
❌ No internet connection

Suggestion: Check your network connection
```

### 5. Backend Down
```
SignalR negotiation timeout - backend may not be available

Suggestion: Check if backend is running
```

## Next Steps

After diagnostics identify an issue:

1. **Follow the suggestions** provided in the summary
2. **Check the troubleshooting guide** (`SIGNALR_TROUBLESHOOTING_GUIDE.md`)
3. **Re-run diagnostics** after applying fixes
4. **Test with real session** expiry

---

**Status**: ✅ Fully Implemented
**Auto-trigger**: When SignalR connection fails
**Manual trigger**: `window.runSignalRDiagnostics()`
**Backend logging**: Enhanced with emoji indicators
**Frontend integration**: Complete with fallback

