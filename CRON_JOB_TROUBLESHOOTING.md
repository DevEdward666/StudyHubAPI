# 🔍 CRON JOB NOT RUNNING - TROUBLESHOOTING GUIDE

## ⚠️ Current Status

The SessionExpiryChecker (cron job) is **configured correctly** to run every 1 minute, BUT it only works when **the backend is running**.

## 🐛 Why It's Not Working

### Reason 1: Backend is NOT Running ❌

**The most common reason** - I checked and your backend is currently **NOT running**.

```bash
# I checked this:
ps aux | grep dotnet
# Result: No dotnet processes found
```

**Solution:** Start the backend!

```bash
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet run
```

### Reason 2: Backend Started But Cron Job Crashed

If the backend is running but cron job isn't working, it might have crashed. Check logs.

---

## ✅ How the Cron Job Works

### Configuration (Verified ✅)

**File:** `SessionExpiryChecker.cs`

```csharp
private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);  // ✅ Set to 1 minute

protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    _logger.LogInformation("SessionExpiryChecker started. Checking every {Interval} minutes.", _interval.TotalMinutes);
    
    while (!stoppingToken.IsCancellationRequested)
    {
        try
        {
            await CheckForExpiredSessions(stoppingToken);  // Check sessions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking expired sessions.");
        }

        await Task.Delay(_interval, stoppingToken);  // Wait 1 minute
    }
}
```

### Registration (Verified ✅)

**File:** `Program.cs` (Line 99)

```csharp
builder.Services.AddHostedService<Study_Hub.Services.Background.SessionExpiryChecker>();
```

### What It Checks Every Minute

1. **Subscription Sessions:**
   - If `RemainingHours <= 0` → End session ✅
   - Free table ✅
   - Send notification ✅

2. **Non-Subscription Sessions:**
   - If `EndTime <= now` → End session ✅
   - Charge credits ✅
   - Free table ✅
   - Send notification ✅

---

## 🚀 How to Verify Cron Job is Running

### Step 1: Start the Backend

```bash
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet run
```

### Step 2: Check Startup Logs

You should see this line immediately:

```
info: Study_Hub.Services.Background.SessionExpiryChecker[0]
      SessionExpiryChecker started. Checking every 1 minutes.
```

✅ **If you see this** = Cron job is running!

### Step 3: Watch for Periodic Logs

Every **1 minute**, you should see one of these:

**If no sessions to check:**
```
[14:30:00 INF] No active sessions found at 2025-11-09T14:30:00Z
```

**If sessions found but none expired:**
```
[14:31:00 INF] No sessions need to be ended at 2025-11-09T14:31:00Z
```

**If sessions expired:**
```
[14:32:00 INF] Found 2 sessions to end
[14:32:00 INF] Ending subscription session abc123. Hours used: 2.5h
[14:32:00 INF] Session abc123 ended for table T-001. User: John Doe, Type: Subscription
```

### Step 4: Monitor for Errors

If you see errors:
```
[ERR] Error while checking expired sessions.
System.InvalidOperationException: ...
```

This means the cron job tried to run but encountered an error.

---

## 🧪 How to Test the Cron Job

### Test 1: Create a Subscription Session with Low Hours

1. **Create a user subscription** with only `0.1` hours remaining
2. **Start a session** for that user
3. **Wait 1 minute**
4. **Check logs** - should see session ended automatically

### Test 2: Create a Non-Subscription Session with Past EndTime

1. **Manually insert** a TableSession with:
   ```sql
   EndTime = NOW() - INTERVAL '5 minutes'  -- 5 minutes ago
   Status = 'active'
   SubscriptionId = NULL
   ```
2. **Wait up to 1 minute**
3. **Check logs** - should see session ended

### Test 3: Watch Real-Time

```bash
# Start backend with verbose logging
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet run

# In another terminal, watch logs
tail -f /path/to/logs/*.log

# Or just watch the console output
```

Every 60 seconds, you should see activity.

---

## 📊 Expected Log Timeline

Here's what you should see when cron job is working:

```
[00:00:00 INF] SessionExpiryChecker started. Checking every 1 minutes.
[00:00:00 INF] No active sessions found at 2025-11-09T00:00:00Z

... (60 seconds pass) ...

[00:01:00 INF] No active sessions found at 2025-11-09T00:01:00Z

... (60 seconds pass) ...

[00:02:00 INF] Found 1 sessions to end
[00:02:00 INF] Subscription session abc123 has depleted hours. User: John, Remaining: 0h
[00:02:00 INF] Ending subscription session abc123. Hours used: 2.5h, Remaining before: 0h
[00:02:00 INF] Session abc123 ended for table T-001. User: John, Type: Subscription

... (60 seconds pass) ...

[00:03:00 INF] No active sessions found at 2025-11-09T00:03:00Z
```

**Notice:** Logs appear approximately every **60 seconds** ✅

---

## 🔍 Debugging: Why Cron Job Might Not Be Checking

### Issue 1: Backend Not Running

**Symptom:** No logs at all

**Check:**
```bash
ps aux | grep dotnet
```

**Solution:**
```bash
cd /Users/edward/Documents/StudyHubAPI/Study-Hub
dotnet run
```

### Issue 2: Cron Job Crashed

**Symptom:** Saw "SessionExpiryChecker started" but then no more logs

**Check backend console for:**
```
fail: Study_Hub.Services.Background.SessionExpiryChecker[0]
      Error while checking expired sessions.
```

**Common causes:**
- Database connection lost
- Exception in CheckForExpiredSessions method
- Unhandled null reference

**Solution:** Check exception details in logs and fix the error

### Issue 3: Time Zone Issues

**Symptom:** Sessions not being detected as expired

**Check:**
```csharp
var now = DateTime.UtcNow;  // ✅ Should be UTC
```

**Verify your database times are also UTC:**
```sql
SELECT "EndTime", "Status", NOW() 
FROM "TableSessions" 
WHERE "Status" = 'active';
```

### Issue 4: Interval Too Long

**Symptom:** Cron job runs but not every minute

**Check:**
```csharp
private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);  // Should be 1
```

**If it's set to something else:**
```csharp
TimeSpan.FromMinutes(5)   // ❌ Would run every 5 minutes
TimeSpan.FromHours(1)     // ❌ Would run every hour
```

### Issue 5: Cron Job Registered But Not Started

**Check Program.cs:**
```csharp
builder.Services.AddHostedService<SessionExpiryChecker>();  // ✅ Should be present
```

**If missing, add it before `var app = builder.Build();`**

---

## 🎯 Quick Verification Checklist

Run through this checklist:

- [ ] Backend is running (`dotnet run`)
- [ ] Saw "SessionExpiryChecker started" in startup logs
- [ ] Waiting at least 60 seconds to see next log
- [ ] Checking backend console (not frontend)
- [ ] Have at least one active session in database
- [ ] Session has either:
  - [ ] `RemainingHours <= 0` (for subscription)
  - [ ] `EndTime <= NOW()` (for non-subscription)

---

## 🔧 Manual Test Script

If you want to force a check immediately (for testing):

**Add this endpoint to TablesController.cs:**

```csharp
[HttpPost("test-expiry-check")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> TestExpiryCheck()
{
    // Get the SessionExpiryChecker service
    var checker = HttpContext.RequestServices
        .GetRequiredService<IHostedService>() as SessionExpiryChecker;
    
    if (checker != null)
    {
        // This would trigger a check (but we can't access ExecuteAsync)
        return Ok(new { message = "Check the logs for SessionExpiryChecker activity" });
    }
    
    return NotFound("SessionExpiryChecker not found");
}
```

**Or better, check the database directly:**

```sql
-- Check for sessions that should be ended
SELECT 
    ts."Id",
    ts."Status",
    ts."StartTime",
    ts."EndTime",
    ts."SubscriptionId",
    us."RemainingHours",
    NOW() as "CurrentTime",
    CASE 
        WHEN ts."SubscriptionId" IS NOT NULL AND us."RemainingHours" <= 0 
            THEN 'Should be ended (subscription hours depleted)'
        WHEN ts."SubscriptionId" IS NULL AND ts."EndTime" <= NOW() 
            THEN 'Should be ended (time expired)'
        ELSE 'Still active'
    END as "ShouldBeEnded"
FROM "TableSessions" ts
LEFT JOIN "UserSubscriptions" us ON ts."SubscriptionId" = us."Id"
WHERE ts."Status" = 'active';
```

---

## 📝 Common Scenarios

### Scenario 1: Everything Looks Right But Not Working

**Steps:**
1. Stop backend (Ctrl+C)
2. Restart backend (`dotnet run`)
3. Watch console carefully for startup messages
4. Wait exactly 60 seconds
5. Check if new log appears

### Scenario 2: Cron Job Runs But Doesn't End Sessions

**Check:**
- Are there actually any sessions to end?
- Run the SQL query above
- Verify RemainingHours or EndTime conditions

### Scenario 3: Sessions End But No Notifications

**Check:**
- SignalR connection (separate issue)
- Notifications table in database
- Admin user in "admins" SignalR group

---

## ✅ Expected Behavior Summary

| Time | Action | Expected Log |
|------|--------|--------------|
| 0s | Backend starts | "SessionExpiryChecker started. Checking every 1 minutes." |
| 0s | First check runs | "No active sessions found" or "Found X sessions to end" |
| 60s | Second check runs | Similar log message |
| 120s | Third check runs | Similar log message |
| ... | Every 60s | New log entry |

**If you're NOT seeing logs every 60 seconds, the cron job is not running!**

---

## 🚀 Final Solution

### The Fix is Simple:

1. **Make sure backend is running:**
   ```bash
   cd /Users/edward/Documents/StudyHubAPI/Study-Hub
   dotnet run
   ```

2. **Look for this line in startup:**
   ```
   SessionExpiryChecker started. Checking every 1 minutes.
   ```

3. **Wait 60 seconds and look for activity:**
   ```
   No active sessions found at ...
   ```
   OR
   ```
   Found X sessions to end
   ```

4. **If you see these logs every ~60 seconds** = ✅ **Cron job is working!**

5. **If you DON'T see logs** = ❌ **Backend not running or crashed**

---

## 📊 Monitoring Dashboard Idea

To make it easier to verify the cron job is working, you could add an endpoint:

```csharp
[HttpGet("admin/cron-status")]
[Authorize(Roles = "Admin")]
public IActionResult GetCronStatus()
{
    return Ok(new {
        cronJobRunning = true,  // Check if SessionExpiryChecker is registered
        lastCheckTime = "2025-11-09T14:30:00Z",  // Store this in a static variable
        intervalMinutes = 1,
        totalChecksRun = 150,  // Counter
        totalSessionsEnded = 45  // Counter
    });
}
```

Then display this in admin dashboard to monitor health.

---

## 🎉 Summary

**Cron Job Configuration:** ✅ CORRECT (set to 1 minute)  
**Registration:** ✅ CORRECT (registered as hosted service)  
**Current Status:** ❌ NOT RUNNING (backend not started)  

**Action Required:**
1. **Start backend:** `dotnet run`
2. **Watch logs:** Should see activity every 60 seconds
3. **Verify:** Create test session and watch it get auto-ended

**The cron job WILL work once backend is running!** 🚀

---

**Date:** November 9, 2025  
**Issue:** Cron job not checking every minute  
**Root Cause:** Backend not running  
**Resolution:** Start backend with `dotnet run`  
**Configuration Status:** ✅ Verified correct (1 minute interval)

