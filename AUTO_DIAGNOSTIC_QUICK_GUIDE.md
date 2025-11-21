# Auto-Diagnostic System - Quick Guide

## ✅ What Was Implemented

An automatic diagnostic system that runs when sessions expire to help troubleshoot SignalR connection issues.

## 🎯 When It Runs

### Automatically:
1. **When SignalR fails to connect**
   - Shows detailed error message
   - Runs diagnostics after 1 second
   - Identifies the issue

2. **When loaded in development** (localhost)
   - Makes diagnostics available
   - Doesn't auto-run on load

### Manually:
```javascript
// In browser console
window.runSignalRDiagnostics()
```

## 📊 What It Checks

1. ✅ Authentication token (exists, valid, not expired)
2. ✅ User role (Admin or Super Admin)
3. ✅ Current location (on admin page?)
4. ✅ Browser capabilities (WebSocket, Audio, etc.)
5. ✅ Network status (online/offline)
6. ✅ Environment configuration

## 🔍 Expected Flow

### Backend (when session expires):
```
[15:30:00] Subscription session ended for table 5
[15:30:00] 📡 Sending SessionEnded notification to 'admins' group - Table 5, User: John
[15:30:00] ✅ SessionEnded notification sent successfully
```

### Frontend (should receive):
```
🔔 Session ended notification received: {tableNumber: "5", ...}
📝 Setting session ended data...
🔊 Playing session ended sound...
🚀 Opening session ended modal...
```

### If Frontend Doesn't Receive:
```
❌ Failed to setup SignalR: [error]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
⚠️  SIGNALR CONNECTION FAILED
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Running diagnostics to help identify the issue...
[Diagnostic output...]
```

## 🧪 How to Test

### Test 1: Manual Run
```javascript
// In browser console
window.runSignalRDiagnostics()
```

### Test 2: Create Session & Wait
1. Create subscription (0.02 hours)
2. Start session
3. Wait 2-3 minutes
4. Check backend logs for:
   ```
   📡 Sending SessionEnded notification
   ✅ SessionEnded notification sent successfully
   ```
5. Check frontend console for notification

### Test 3: Check Connection
```javascript
// In browser console
console.log('Token:', localStorage.getItem('auth_token') ? 'EXISTS' : 'MISSING');
console.log('Is Admin:', window.location.pathname.includes('/admin'));
console.log('Online:', navigator.onLine);
```

## 🎨 Example Output

```
🔍 Starting SignalR Diagnostics...

1️⃣ AUTHENTICATION CHECK
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ Auth token exists
   User ID: abc-123
   Role: Admin
   Expires: Nov 21, 2025 6:00 PM
   Is Expired: ✅ NO

2️⃣ LOCATION CHECK
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Current URL: https://...
   Current Path: /app/admin/dashboard
   Is Admin Path: ✅ YES

[... more checks ...]

9️⃣ DIAGNOSTIC SUMMARY
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ All checks passed!
```

## 🔧 Files Modified

1. **Backend**: `SessionExpiryChecker.cs`
   - Added `📡` emoji before sending notification
   - Added `✅` confirmation after sending
   - Better logging for debugging

2. **Frontend**: `TabsLayout.tsx`
   - Added `runDiagnostics()` function
   - Auto-runs on SignalR failure
   - Exposed to `window` for manual use
   - Loads `/signalr-diagnostic.js`

3. **Diagnostic Script**: `signalr-diagnostic.js`
   - Wrapped in function
   - Auto-runs when loaded
   - Exported to window
   - Comprehensive health checks

## 💡 Quick Actions

### If Token Expired:
```javascript
localStorage.removeItem('auth_token');
window.location.href = '/login';
```

### If Not on Admin Page:
```javascript
window.location.href = '/app/admin/dashboard';
```

### If Need to Clear All:
```javascript
localStorage.clear();
window.location.reload();
```

## 📚 Full Documentation

- **Complete Guide**: `AUTO_DIAGNOSTIC_SYSTEM.md`
- **Troubleshooting**: `SIGNALR_TROUBLESHOOTING_GUIDE.md`
- **Fix History**: `SIGNALR_401_UNAUTHORIZED_FIX.md`, `SIGNALR_TRANSPORT_ERROR_FIX.md`

---

**Status**: ✅ Implemented & Ready
**Trigger**: Automatic on SignalR failure
**Manual**: `window.runSignalRDiagnostics()`
**Location**: Available on all admin pages

