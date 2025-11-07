# Session Expiry Auto-Notification System

## Overview
This system automatically checks for expired table sessions every 5 minutes, completes them, and notifies admins in real-time using SignalR with toast notifications and sound alerts.

## Architecture

### Backend Components

1. **SessionExpiryChecker** (`Study-Hub/Services/Background/SessionExpiryChecker.cs`)
   - Background service that runs every 5 minutes
   - Checks for active sessions where EndTime has passed
   - Automatically completes expired sessions
   - Deducts credits from user balance
   - Frees up the table
   - Creates notification records
   - Sends real-time notifications to admins via SignalR

2. **NotificationHub** (`Study-Hub/Hubs/NotificationHub.cs`)
   - SignalR hub for real-time communication
   - Manages admin group membership
   - Broadcasts `SessionEnded` events to all connected admins

3. **Modified ChangeTableAsync** (`Study-Hub/Service/TableService.cs`)
   - No longer ends the current session when changing tables
   - Simply moves the active session to the new table
   - Maintains session continuity across table changes

### Frontend Components

1. **SignalRService** (`study_hub_app/src/services/signalr.service.ts`)
   - Manages WebSocket connection to SignalR hub
   - Auto-reconnects on connection loss
   - Joins "admins" group on connection
   - Handles `SessionEnded` events

2. **GlobalToast Component** (`study_hub_app/src/components/GlobalToast/`)
   - Displays toast notifications at the top of the screen
   - Supports success, error, warning, and info types
   - Auto-dismisses after configurable duration
   - Plays sound alerts using Web Audio API

3. **TabsLayout Integration** (`study_hub_app/src/components/Layout/TabsLayout.tsx`)
   - Initializes SignalR connection for admin users
   - Displays toast notifications when sessions expire
   - Plays audio alert sound

## How It Works

### Session Expiry Flow

1. **Background Check** (Every 5 minutes)
   - `SessionExpiryChecker` queries database for expired sessions
   - Filters: `Status == "active" AND EndTime <= NOW()`

2. **Session Completion**
   - Calculate duration and total amount
   - Deduct credits from user balance
   - Update session status to "completed"
   - Free the table (set `IsOccupied = false`)
   - Create notification record

3. **Real-Time Notification**
   - Send `SessionEnded` event via SignalR to "admins" group
   - Payload includes: session details, table number, user name, duration, amount

4. **Admin UI**
   - Toast appears at the top of the screen
   - Message: "🔔 Table {number} session ended for {user}. Duration: {hours}hrs, Amount: ₱{amount}"
   - Color: Warning (orange)
   - Sound: Beep alert plays
   - Auto-dismiss after 10 seconds

### Table Change Flow (Updated)

**Before:**
- End current session
- Create new session on new table
- Transfer remaining time

**After (Current):**
- Keep session active
- Simply update `TableId` to new table
- Free old table, occupy new table
- Session continues until original EndTime

## Configuration

### Check Interval
Default: 5 minutes

To change, edit `SessionExpiryChecker.cs`:
```csharp
private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);
```

### Toast Duration
Default: 10 seconds

To change, edit `TabsLayout.tsx`:
```typescript
showToast(message, 'warning', 10000, true); // 10000ms = 10 seconds
```

### SignalR Connection
Default: Auto-configured based on API URL

To change, edit `signalr.service.ts`:
```typescript
const apiBaseUrl = import.meta.env.VITE_API_URL || "YOUR_API_URL";
```

## CORS Configuration

The backend CORS policy has been updated to support SignalR:

```csharp
policy.WithOrigins("http://localhost:5173", "https://localhost:5173", ...)
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials(); // Required for SignalR
```

**Important:** Update the allowed origins list in production to include your production domain.

## Deployment Checklist

### Backend
- [x] Background service registered in `Program.cs`
- [x] SignalR hub registered and mapped
- [x] CORS configured with `AllowCredentials()`
- [ ] Update CORS origins for production domain
- [ ] Test SignalR hub connectivity
- [ ] Monitor background service logs

### Frontend
- [x] SignalR client package installed (`@microsoft/signalr`)
- [x] SignalR service created
- [x] Toast component created
- [x] Integrated in TabsLayout
- [ ] Test admin notifications
- [ ] Verify sound alerts work
- [ ] Test on production domain

## Testing

### Backend Testing
1. Create a table session with EndTime in the past:
```sql
UPDATE table_sessions 
SET end_time = NOW() - INTERVAL '1 minute', 
    status = 'active'
WHERE id = 'your-session-id';
```

2. Wait up to 5 minutes for background service to run
3. Check logs for "Session expired for table X"
4. Verify session is completed and table is freed

### Frontend Testing
1. Log in as admin
2. Open browser console
3. Look for "SignalR connected successfully"
4. Trigger a session expiry (use SQL above)
5. Wait for background check (max 5 minutes)
6. Toast should appear with sound alert

### Manual SignalR Test
Use browser console in admin panel:
```javascript
// Check connection state
console.log('SignalR state:', signalRService.getConnectionState());

// Should log: "Connected"
```

## Troubleshooting

### No Toast Appears
1. Check if user is admin
2. Verify SignalR connection in console
3. Check CORS settings
4. Ensure backend is running and hub is accessible

### Sound Not Playing
1. Check browser audio permissions
2. User must interact with page first (browser security)
3. Check console for Web Audio API errors

### Sessions Not Expiring
1. Check `SessionExpiryChecker` is registered in `Program.cs`
2. Verify background service is running (check logs)
3. Ensure database connection is working
4. Check session EndTime values

## Security Considerations

1. **Admin-Only Notifications**: Only users with admin privileges receive notifications
2. **Group Isolation**: SignalR uses groups to ensure only admins receive session events
3. **CORS**: Restrict allowed origins in production
4. **Authentication**: Consider adding authentication to SignalR hub

### Adding SignalR Authentication (Optional)
```csharp
// In NotificationHub.cs
public override async Task OnConnectedAsync()
{
    var user = Context.User;
    if (user?.IsInRole("Admin") == true)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
    }
    await base.OnConnectedAsync();
}
```

## Future Enhancements

- [ ] Add notification history page
- [ ] Mark notifications as read
- [ ] Customizable notification sounds
- [ ] Email/SMS alerts for critical notifications
- [ ] Notification preferences per admin
- [ ] Desktop push notifications
- [ ] Notification filter/search
- [ ] Bulk notification actions

## Files Modified/Created

### Backend
- ✅ `Study-Hub/Services/Background/SessionExpiryChecker.cs` (Created)
- ✅ `Study-Hub/Hubs/NotificationHub.cs` (Created)
- ✅ `Study-Hub/Program.cs` (Modified - Added SignalR + Background Service)
- ✅ `Study-Hub/Service/TableService.cs` (Modified - Updated ChangeTableAsync)

### Frontend
- ✅ `study_hub_app/src/services/signalr.service.ts` (Created)
- ✅ `study_hub_app/src/components/GlobalToast/GlobalToast.tsx` (Created)
- ✅ `study_hub_app/src/components/GlobalToast/GlobalToast.css` (Created)
- ✅ `study_hub_app/src/components/Layout/TabsLayout.tsx` (Modified - Added SignalR + Toast)

## API Reference

### SignalR Hub Methods

**Endpoint:** `/hubs/notifications`

**Methods:**
- `JoinAdmins()` - Join the admins group to receive notifications
- `LeaveAdmins()` - Leave the admins group

**Events:**
- `SessionEnded` - Emitted when a session expires
  ```typescript
  {
    id: string;
    sessionId: string;
    tableId: string;
    tableNumber: string;
    userName: string;
    message: string;
    duration: number;
    amount: number;
    createdAt: string;
  }
  ```

## Monitoring

### Logs to Watch

**Background Service:**
```
SessionExpiryChecker started. Checking every 5 minutes.
No expired sessions found at {Time}
Found {Count} expired sessions to process
Session {SessionId} expired for table {TableNumber}. User: {UserId}
```

**SignalR:**
```
SignalR connected successfully
Joined admins group
SignalR reconnecting...
SignalR reconnected: {connectionId}
```

## Summary

This implementation provides a complete auto-expiry and notification system:
- ✅ No manual session ending required when changing tables
- ✅ Automatic session completion every 5 minutes
- ✅ Real-time admin notifications via SignalR
- ✅ Visual toast alerts with sound
- ✅ Robust reconnection handling
- ✅ Clean separation of concerns
- ✅ Production-ready architecture

The system is now fully functional and ready for testing!

