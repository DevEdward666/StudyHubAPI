# Session Expiry Checker Refactoring

## Problem
The `CheckForExpiredSessions` cron job was not detecting sessions that should be ended, even when the frontend `SessionTimer` component showed "No Hours Left" or "Time's Up".

### Root Cause
The cron job was only checking the static `RemainingHours` value in the database, which only gets updated when a session ends. It wasn't accounting for the **currently active session's elapsed time**.

**Example Scenario:**
- User has 2 hours remaining in their subscription
- User starts a session at 10:00 AM
- At 12:30 PM (2.5 hours later):
  - Frontend calculates: 2 hours - 2.5 hours elapsed = **-0.5 hours** → Shows "No Hours Left"
  - Backend checks: `RemainingHours` = **2 hours** (hasn't been updated yet) → "No sessions need to be ended"

## Solution
Refactored `CheckForExpiredSessions` to calculate the **effective remaining hours** by subtracting the current session's elapsed time from the subscription's `RemainingHours`.

### Key Changes

#### 1. Calculate Effective Remaining Hours
```csharp
// Calculate hours used in THIS session so far
var sessionElapsedHours = (decimal)(now - session.StartTime).TotalHours;

// Calculate effective remaining hours after accounting for current session
var effectiveRemainingHours = session.Subscription.RemainingHours - sessionElapsedHours;
```

#### 2. Check Against Effective Hours
```csharp
// If effective remaining hours <= 0, session should end
if (effectiveRemainingHours <= 0)
{
    sessionsToEnd.Add(session);
}
```

#### 3. Update Subscription Hours When Ending
```csharp
if (session.Subscription != null)
{
    session.Subscription.HoursUsed += hoursUsedInSession;
    session.Subscription.RemainingHours = Math.Max(0, session.Subscription.TotalHours - session.Subscription.HoursUsed);
    session.Subscription.Status = "Expired"; // Mark as expired since hours are depleted
    session.Subscription.UpdatedAt = DateTime.UtcNow;
}
```

## How It Works Now

### Every 1 Minute
1. **Fetch** all active subscription sessions
2. For each session:
   - Calculate how long it has been running (`sessionElapsedHours`)
   - Calculate effective remaining hours: `RemainingHours - sessionElapsedHours`
   - If effective remaining hours ≤ 0, mark for termination
3. **End** flagged sessions:
   - Update subscription's `HoursUsed` and `RemainingHours`
   - Mark subscription as "Expired" if depleted
   - Complete the session and free the table
   - Send notification to admins via SignalR

## Benefits
- ✅ Accurately detects when subscriptions run out of hours **during** active sessions
- ✅ Matches frontend timer behavior
- ✅ Prevents over-usage of subscription hours
- ✅ Real-time session termination when hours are depleted
- ✅ Proper audit trail with updated `HoursUsed` and `RemainingHours`

## Testing Recommendations
1. Create a subscription with a small amount of hours (e.g., 0.1 hours = 6 minutes)
2. Start a session
3. Wait for the cron job to run (every 1 minute)
4. Verify the session automatically ends when hours are depleted
5. Check logs for debug messages showing:
   - `RemainingHours`
   - `SessionElapsed`
   - `Effective` remaining hours

## Files Modified
- `/Users/edward/Documents/StudyHubAPI/Study-Hub/Services/Background/SessionExpiryChecker.cs`

