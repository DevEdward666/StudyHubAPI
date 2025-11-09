# ✅ SESSION EXPIRY CHECKER FIXED - Now Handles Subscription Sessions

## 🐛 Original Problem

The **SessionExpiryChecker** background service was **NOT working** for subscription-based sessions because it was explicitly excluding them:

```csharp
// ❌ OLD CODE - Only checked non-subscription sessions
var expiredSessions = await context.TableSessions
    .Where(s => s.Status == "active" 
        && s.EndTime.HasValue 
        && s.EndTime <= now
        && s.SubscriptionId == null)  // ❌ Excluded subscription sessions!
    .ToListAsync(ct);
```

**Result:** Subscription sessions would NEVER be automatically ended, even when hours were depleted!

---

## ✅ Fix Applied

Updated `/Users/edward/Documents/StudyHubAPI/Study-Hub/Services/Background/SessionExpiryChecker.cs` to handle **BOTH** types of sessions:

### 1. Check ALL Active Sessions

```csharp
// ✅ NEW CODE - Check ALL active sessions
var activeSessions = await context.TableSessions
    .Include(s => s.Table)
    .Include(s => s.User)
    .Include(s => s.Rate)
    .Include(s => s.Subscription)
        .ThenInclude(sub => sub.Package)
    .Where(s => s.Status == "active")  // ✅ No filtering by SubscriptionId
    .ToListAsync(ct);
```

### 2. Check Each Session Type Differently

```csharp
foreach (var session in activeSessions)
{
    // ✅ For subscription sessions: Check if hours depleted
    if (session.SubscriptionId.HasValue && session.Subscription != null)
    {
        if (session.Subscription.RemainingHours <= 0)
        {
            sessionsToEnd.Add(session);
            // Log and end this session
        }
    }
    // ✅ For non-subscription sessions: Check if EndTime passed
    else if (session.EndTime.HasValue && session.EndTime <= now)
    {
        sessionsToEnd.Add(session);
        // Log and end this session
    }
}
```

### 3. Handle Each Session Type Properly

```csharp
foreach (var session in sessionsToEnd)
{
    // ✅ Subscription sessions: No credits charged, just end the session
    if (session.SubscriptionId.HasValue && session.Subscription != null)
    {
        var sessionDuration = (now - session.StartTime).TotalHours;
        creditsUsed = 0;  // ✅ No credit deduction for subscriptions
        
        _logger.LogInformation(
            "Ending subscription session {SessionId}. Hours used: {Hours}h",
            session.Id,
            sessionDuration);
    }
    // ✅ Non-subscription sessions: Charge credits based on hours
    else
    {
        var duration = session.EndTime.Value - session.StartTime;
        hoursUsed = Math.Ceiling(duration.TotalHours);
        var tableRate = session.Rate?.Price ?? session.Table?.HourlyRate ?? 50;
        creditsUsed = (decimal)(hoursUsed * (double)tableRate);

        // Deduct from user credits
        var userCredits = await context.UserCredit
            .FirstOrDefaultAsync(uc => uc.UserId == session.UserId, ct);
        
        if (userCredits != null)
        {
            userCredits.Balance -= creditsUsed;
            userCredits.TotalSpent += creditsUsed;
        }
    }

    // ✅ Update session status and free table
    session.Status = "completed";
    session.EndTime = now;
    session.Table.IsOccupied = false;
    session.Table.CurrentUserId = null;

    // ✅ Create notification for admin
    var notification = new Notification { ... };
    
    // ✅ Send SignalR notification
    await hubContext.Clients.Group("admins").SendAsync("SessionEnded", ...);
}
```

---

## 🎯 How It Works Now

### Every 1 Minute, the Service Checks:

#### For Subscription-Based Sessions:
```
1. Get active subscription sessions
2. Check if RemainingHours <= 0
3. If yes:
   ✅ End session
   ✅ Free table
   ✅ Create notification
   ✅ Send SignalR alert to admins
   ✅ NO credit deduction (already paid via subscription)
```

#### For Non-Subscription Sessions:
```
1. Get active non-subscription sessions
2. Check if EndTime <= now
3. If yes:
   ✅ Calculate hours used
   ✅ Calculate credits to charge
   ✅ Deduct from user credits
   ✅ End session
   ✅ Free table
   ✅ Create notification
   ✅ Send SignalR alert to admins
```

---

## 📊 Comparison

### Before (Broken):

| Session Type | Checked? | Auto-Ends? | Notification? |
|--------------|----------|------------|---------------|
| Subscription | ❌ No | ❌ Never | ❌ No |
| Non-Subscription | ✅ Yes | ✅ Yes | ✅ Yes |

**Problem:** Subscription sessions would run forever even with 0 hours!

### After (Fixed):

| Session Type | Checked? | Auto-Ends? | Notification? |
|--------------|----------|------------|---------------|
| Subscription | ✅ Yes | ✅ When hours depleted | ✅ Yes |
| Non-Subscription | ✅ Yes | ✅ When time expires | ✅ Yes |

**Result:** Both types work correctly! ✅

---

## 🔔 Notifications

The system now creates different notifications for each type:

### Subscription Session Ended:
```json
{
  "Title": "Subscription Session Ended - Hours Depleted",
  "Message": "Subscription session ended for table T-001 - User ran out of hours",
  "Type": "Session",
  "Priority": "High",
  "Data": {
    "SessionId": "...",
    "TableNumber": "T-001",
    "UserName": "John Doe",
    "Duration": 2.5,
    "Amount": 0,  // No charge
    "IsSubscription": true,
    "RemainingHours": 0
  }
}
```

### Non-Subscription Session Ended:
```json
{
  "Title": "Session Expired",
  "Message": "Session ended for table T-001",
  "Type": "Session",
  "Priority": "High",
  "Data": {
    "SessionId": "...",
    "TableNumber": "T-001",
    "UserName": "Jane Smith",
    "Duration": 3.0,
    "Amount": 150,  // Credits charged
    "IsSubscription": false
  }
}
```

---

## 🎯 Example Scenarios

### Scenario 1: Subscription Session with Hours Depleted

**Setup:**
- User: John Doe
- Subscription: 1 Week Premium (168 hours total)
- Remaining: 0.5 hours
- Session started: 1 hour ago
- Table: T-001

**What Happens:**

```
Minute 0:   Session starts
Minute 30:  SessionExpiryChecker runs - session.RemainingHours = 0.5h (still active)
Minute 31:  (User continues using table)
Minute 60:  SessionExpiryChecker runs - session.RemainingHours = 0h (DEPLETED!)
            ✅ Session marked as "completed"
            ✅ Table T-001 freed
            ✅ Notification created
            ✅ SignalR alert sent to admins
            ✅ Admin sees toast: "Table T-001 session ended for John Doe"
            🔊 Doorbell sound plays
```

### Scenario 2: Non-Subscription Session Time Expired

**Setup:**
- User: Jane Smith
- Session type: Hourly rate (50 credits/hour)
- Start time: 2:00 PM
- End time: 4:00 PM (2 hours)
- Current time: 4:01 PM
- Table: T-002

**What Happens:**

```
4:00 PM:  Session EndTime reached
4:01 PM:  SessionExpiryChecker runs
          ✅ Detects EndTime <= now
          ✅ Calculates: 2 hours × 50 credits = 100 credits
          ✅ Deducts 100 credits from Jane's balance
          ✅ Session marked as "completed"
          ✅ Table T-002 freed
          ✅ Notification created
          ✅ SignalR alert sent to admins
          ✅ Admin sees toast: "Table T-002 session ended"
          🔊 Doorbell sound plays
```

---

## ⏱️ Timing

The SessionExpiryChecker runs **every 1 minute**:

```csharp
private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);
```

**Schedule:**
```
00:00 - Check all sessions
00:01 - Check all sessions
00:02 - Check all sessions
...
```

**Maximum Delay:** A session could run up to 59 seconds longer than it should, but then it will be auto-ended on the next check.

---

## 🚀 To Apply This Fix

### You Need to Restart the Backend

The background service is loaded at startup, so restart is required:

```bash
# Stop current backend (Ctrl+C if running)

# Start backend again
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet run
```

### After Restart:

✅ SessionExpiryChecker will start automatically  
✅ It will log: "SessionExpiryChecker started. Checking every 1 minutes."  
✅ Every minute it will check and log findings  
✅ Subscription sessions with depleted hours will auto-end  
✅ Non-subscription sessions past EndTime will auto-end  
✅ Admins will receive real-time notifications  

---

## 📝 Logs You'll See

### Normal Operation (No Sessions to End):
```
[14:30:00 INF] SessionExpiryChecker started. Checking every 1 minutes.
[14:31:00 INF] No active sessions found at 2025-11-08T14:31:00Z
[14:32:00 INF] No sessions need to be ended at 2025-11-08T14:32:00Z
```

### When Subscription Session Depletes:
```
[14:33:00 INF] Subscription session abc123 has depleted hours. User: John Doe, Remaining: 0h
[14:33:00 INF] Found 1 sessions to end
[14:33:00 INF] Ending subscription session abc123. Hours used: 2.5h, Remaining before: 0h
[14:33:00 INF] Session abc123 ended for table T-001. User: John Doe, Type: Subscription
```

### When Non-Subscription Session Expires:
```
[14:34:00 INF] Non-subscription session def456 has expired. EndTime: 2025-11-08T14:00:00Z
[14:34:00 INF] Found 1 sessions to end
[14:34:00 INF] Ending non-subscription session def456. Hours: 2, Credits: 100
[14:34:00 INF] Session def456 ended for table T-002. User: Jane Smith, Type: Non-Subscription
```

---

## ✅ Summary

**What Was Broken:**
- ❌ Subscription sessions NEVER auto-ended (even with 0 hours)
- ❌ Only non-subscription sessions were checked
- ❌ Admins wouldn't get notified when subscription hours depleted

**What's Fixed:**
- ✅ Both subscription and non-subscription sessions are checked
- ✅ Subscription sessions auto-end when RemainingHours <= 0
- ✅ Non-subscription sessions auto-end when EndTime passes
- ✅ Admins get notifications for both types
- ✅ SignalR alerts work for both types
- ✅ Tables are freed automatically
- ✅ Credits are only charged for non-subscription sessions
- ✅ Runs every 1 minute

**Files Modified:**
1. ✅ `/Users/edward/Documents/StudyHubAPI/Study-Hub/Services/Background/SessionExpiryChecker.cs`

**Build Status:** ✅ Compiles successfully (only warnings, no errors)

**Action Required:** **Restart backend server** to activate the fix

---

**Date:** November 8, 2025  
**File:** `SessionExpiryChecker.cs`  
**Issue:** Cron job not working for subscription sessions  
**Resolution:** Updated to check and handle both subscription and non-subscription sessions

