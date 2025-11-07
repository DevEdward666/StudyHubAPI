# Session Expiry System - Quick Reference

## 🚀 Quick Start

### Backend
The system is **auto-configured** and ready to use. No manual setup needed!

**What happens automatically:**
- Every 5 minutes, checks for expired sessions
- Completes expired sessions
- Sends notifications to admins

### Frontend (Admin Panel)
**What you'll see:**
- 🔔 Toast notification at top of screen
- 🔊 Beep sound alert
- Message: "Table X session ended for [User]. Duration: Xhrs, Amount: ₱X"

**Connection Status:**
- Open browser console
- Look for: `"SignalR connected successfully"`
- Also shows: `"Joined admins group"`

## 📊 Testing

### Quick Test (SQL)
```sql
-- Make a session expire immediately
UPDATE table_sessions
SET end_time = NOW() - INTERVAL '1 minute'
WHERE status = 'active' AND id = 'your-session-id';

-- Check if it gets processed (wait 5 min max)
SELECT * FROM table_sessions WHERE id = 'your-session-id';
-- Status should become 'completed'
```

### Expected Logs

**Backend Console:**
```
SessionExpiryChecker started. Checking every 5 minutes.
Found 1 expired sessions to process
Session abc-123 expired for table Table 1. User: user-id
```

**Frontend Console (Admin):**
```
SignalR connected successfully
Joined admins group
Session ended notification: { tableNumber: "Table 1", ... }
```

## ⚙️ Configuration

### Change Check Interval
**File:** `Study-Hub/Services/Background/SessionExpiryChecker.cs`
```csharp
private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Change this
```

### Change Toast Duration
**File:** `study_hub_app/src/components/Layout/TabsLayout.tsx`
```typescript
showToast(message, 'warning', 10000, true); // 10000 = 10 seconds
```

### Disable Sound
```typescript
showToast(message, 'warning', 10000, false); // false = no sound
```

## 🔍 Troubleshooting

### No notifications appearing?

**Check 1: Is user admin?**
```
Only admin users receive notifications
```

**Check 2: SignalR connected?**
```javascript
// In browser console
console.log(signalRService.getConnectionState());
// Should show: "Connected"
```

**Check 3: CORS issue?**
```
Backend CORS must allow credentials
Check Program.cs: .AllowCredentials()
```

**Check 4: Backend running?**
```bash
# Check if hub is accessible
curl https://your-api/hubs/notifications
# Should return 200 or 404 (not connection error)
```

### Sound not playing?

**Browser Restriction:**
- User must interact with page first (click/tap)
- Some browsers block audio until user gesture

**Check:**
```javascript
// In console after page interaction
const audio = new AudioContext();
console.log(audio.state); // Should be "running"
```

### Sessions not expiring?

**Check 1: Background service registered?**
```csharp
// In Program.cs, should have:
builder.Services.AddHostedService<Study_Hub.Services.Background.SessionExpiryChecker>();
```

**Check 2: Check logs**
```
Look for "SessionExpiryChecker started" in backend logs
If missing, service is not running
```

**Check 3: Database query**
```sql
-- Are there sessions to expire?
SELECT COUNT(*) 
FROM table_sessions 
WHERE status = 'active' AND end_time < NOW();
```

## 📱 Table Change Behavior

### Old Behavior (Before)
```
User changes table → Current session ENDS → New session STARTS
```

### New Behavior (Now)
```
User changes table → Session MOVES to new table → Continues until EndTime
```

**Why?**
- Session time is preserved
- No need to recalculate credits
- Cleaner for reporting

## 🎯 Key Files

### Backend
```
Study-Hub/
├── Services/Background/SessionExpiryChecker.cs  ← Background worker
├── Hubs/NotificationHub.cs                      ← SignalR hub
└── Program.cs                                   ← Registration
```

### Frontend
```
study_hub_app/src/
├── services/signalr.service.ts                  ← SignalR client
├── components/GlobalToast/GlobalToast.tsx      ← Toast UI
└── components/Layout/TabsLayout.tsx             ← Integration
```

## 🔐 Security Notes

**Current Setup:**
- ✅ Only admins receive notifications (client-side check)
- ✅ CORS configured with credentials
- ⚠️  SignalR hub is unauthenticated

**For Production (Optional):**
Add authentication to hub:
```csharp
public override async Task OnConnectedAsync()
{
    if (Context.User?.IsInRole("Admin") == true)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
    }
    await base.OnConnectedAsync();
}
```

## 📈 Monitoring

### Health Check
```bash
# Check if service is running
curl https://your-api/health

# Check active sessions
curl -H "Authorization: Bearer $TOKEN" \
  https://your-api/api/admin/transactions/pending
```

### Database Queries
```sql
-- Recent notifications
SELECT * FROM notifications 
ORDER BY created_at DESC 
LIMIT 10;

-- Active sessions count
SELECT COUNT(*) FROM table_sessions WHERE status = 'active';

-- Expired but not processed (should be 0 after background run)
SELECT COUNT(*) FROM table_sessions 
WHERE status = 'active' AND end_time < NOW();
```

## 🎨 Customization

### Change Toast Color
**File:** `study_hub_app/src/components/GlobalToast/GlobalToast.css`
```css
.global-toast-warning {
  --background: #f59e0b; /* Change this color */
  --color: white;
}
```

### Change Notification Sound
**File:** `study_hub_app/src/components/GlobalToast/GlobalToast.tsx`
```typescript
// In playNotificationSound function
oscillator.frequency.value = 800; // Change frequency (Hz)
// Higher = higher pitch, Lower = lower pitch
```

### Change Message Format
**File:** `study_hub_app/src/components/Layout/TabsLayout.tsx`
```typescript
const message = `🔔 Table ${notification.tableNumber} session ended...`;
// Customize this message format
```

## 📞 Support Commands

```bash
# Build backend
cd Study-Hub && dotnet build

# Run backend (dev)
cd Study-Hub && dotnet run

# Install frontend deps
cd study_hub_app && npm install

# Run frontend (dev)
cd study_hub_app && npm run dev

# Test session expiry
./test-session-expiry.sh
```

## ✅ Checklist

**Backend Running:**
- [ ] `dotnet run` shows "SessionExpiryChecker started"
- [ ] No errors in console
- [ ] `/hubs/notifications` endpoint accessible

**Frontend Running:**
- [ ] Admin logged in
- [ ] Browser console shows "SignalR connected successfully"
- [ ] No CORS errors in console

**Ready to Test:**
- [ ] Create a test session
- [ ] Expire it using SQL
- [ ] Wait up to 5 minutes
- [ ] Toast notification appears
- [ ] Sound plays
- [ ] Session marked completed

---

**Need help?** Check the full documentation in `SESSION_EXPIRY_NOTIFICATION_SYSTEM.md`

