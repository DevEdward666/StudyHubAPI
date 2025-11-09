# 🔍 SUBSCRIPTION HOURS DEDUCTION - DEBUGGING GUIDE

## ✅ System Status Check

The code is **correctly implemented** to deduct hours. Here's what should happen:

### Backend (Already Correct ✅):
```csharp
// In EndTableSessionAsync for subscription sessions:
var hoursUsed = (decimal)duration.TotalHours; // Precise calculation
subscription.HoursUsed += hoursUsed; // Add to total used
subscription.RemainingHours = Math.Max(0, subscription.TotalHours - subscription.HoursUsed);
// ✅ Hours ARE being deducted
```

### Frontend (Already Correct ✅):
```typescript
// In handlePauseSession:
await tableService.endSession(session.id); // Calls backend
await refetchSubs(); // Refreshes subscription data
await refetchTables(); // Refreshes table data
// ✅ Data IS being refreshed
```

---

## 🧪 Quick Test Steps

### Test to Verify It's Working:

**Step 1: Check Starting Hours**
```
1. Go to "User & Sessions"
2. Find user with subscription
3. Note the "Remaining" hours (e.g., 168.00h)
```

**Step 2: Start Session**
```
1. Click [Assign Table]
2. Select table
3. Click [Start Session]
4. Note the countdown timer (should start from 168:00:00)
```

**Step 3: Wait 1 Minute**
```
1. Watch the timer count down
2. After 1 minute, timer should show ~167:59:00
3. Session should show ~0.02h
```

**Step 4: Pause Session**
```
1. Click [Pause & Save]
2. Wait for success message
3. User should appear back in "Available" list
```

**Step 5: Verify Deduction**
```
1. Look at user's "Remaining" hours
2. Should now show: 167.98h (or similar)
3. If still 168.00h → PROBLEM! ❌
4. If shows 167.98h → WORKING! ✅
```

**Step 6: Start New Session**
```
1. Click [Assign Table] again for same user
2. Click [Start Session]
3. Check countdown timer
4. Should start from ~167:58:00 (NOT 168:00:00!)
5. If starts from 168:00:00 → PROBLEM! ❌
6. If starts from 167:58:00 → WORKING! ✅
```

---

## 🔧 If Hours Are NOT Being Deducted

### Check 1: Backend Response

**Open Browser DevTools:**
1. Press F12
2. Go to Network tab
3. Click [Pause & Save]
4. Find request: `POST /api/tables/sessions/end`
5. Click on it
6. Check Response

**What to look for:**
```json
{
  "success": true,
  "data": {
    "sessionId": "...",
    "hours": 0.02,  // ← Should show actual hours used
    "duration": 120000  // ← Should show milliseconds
  }
}
```

**If you see this, backend IS working! ✅**

### Check 2: Subscription Refetch

**Still in Network tab:**
1. After pause, look for: `GET /api/subscriptions/user`
2. Click on it
3. Check Response

**What to look for:**
```json
{
  "success": true,
  "data": [
    {
      "id": "...",
      "remainingHours": 167.98,  // ← Should be LESS than before!
      "hoursUsed": 0.02,  // ← Should show accumulated usage
      "totalHours": 168.00
    }
  ]
}
```

**If remainingHours is LESS, backend IS deducting! ✅**

### Check 3: Frontend Display

**Add console logs to verify:**

Open `UserSessionManagement.tsx` and add logging:

```typescript
// In handlePauseSession, after refetchSubs:
await refetchSubs();
await refetchTables();

// Add this:
console.log('Subscriptions after refetch:', subscriptions);
```

**What to look for in console:**
```
Subscriptions after refetch: [
  {
    remainingHours: 167.98,  // ← Should be updated!
    hoursUsed: 0.02
  }
]
```

---

## 🎯 Common Issues & Fixes

### Issue 1: Hours Deducted in DB but UI Not Updating

**Symptom:**
- Backend response shows correct hours
- But UI still shows old hours

**Cause:**
- React Query cache not invalidating
- Component not re-rendering

**Fix:**
```typescript
// Make sure refetchSubs returns a promise
const { data: subscriptions, refetch: refetchSubs } = useAllUserSubscriptions();

// Await it properly
await refetchSubs();

// Force re-render if needed
const [refreshKey, setRefreshKey] = useState(0);
// After refetch:
setRefreshKey(prev => prev + 1);
```

### Issue 2: Timer Shows Old Hours

**Symptom:**
- User has 167.5h remaining
- But timer starts from 168:00:00

**Cause:**
- SubscriptionTimer not receiving updated prop
- Or prop update not triggering recalculation

**Fix:**
```typescript
// Make sure we're passing the RIGHT subscription
const userSubscription = subscriptions?.find(s => s.id === sessionData?.subscriptionId);

// Log to verify:
console.log('Timer getting hours:', userSubscription?.remainingHours);

// Should log: 167.5 (updated value)
```

### Issue 3: Multiple Sessions Not Accumulating

**Symptom:**
- Session 1: Use 0.5h → Still shows 168h
- Session 2: Use 1h → Still shows 168h

**Cause:**
- Backend not saving properly
- Transaction rollback
- Database not committing

**Fix:**
Check backend logs for errors:
```
dotnet run

Look for:
✅ "Session ended successfully"
❌ "Transaction rolled back"
❌ "Failed to save changes"
```

---

## 💾 Database Verification

**If you want to check the database directly:**

### Query to Check Subscription:
```sql
SELECT 
    Id,
    UserId,
    TotalHours,
    HoursUsed,
    RemainingHours,
    Status,
    UpdatedAt
FROM UserSubscriptions
WHERE UserId = 'user-guid-here'
ORDER BY UpdatedAt DESC;
```

**After first pause, should see:**
```
TotalHours: 168.00
HoursUsed: 0.50  (or whatever was used)
RemainingHours: 167.50
UpdatedAt: (recent timestamp)
```

### Query to Check Sessions:
```sql
SELECT 
    Id,
    UserId,
    StartTime,
    EndTime,
    HoursConsumed,
    Status,
    SubscriptionId
FROM TableSessions
WHERE UserId = 'user-guid-here'
ORDER BY StartTime DESC;
```

**Should see:**
```
Session 1:
  StartTime: 2024-11-08 10:00:00
  EndTime: 2024-11-08 10:30:00
  HoursConsumed: 0.50
  Status: completed
  
Session 2:
  StartTime: 2024-11-08 14:00:00
  EndTime: NULL  (if still active)
  HoursConsumed: 0
  Status: active
```

---

## 🎬 Screen Recording Test

**Record your screen while doing this:**

1. Start with clean state (168h)
2. Start session
3. Wait 1 minute
4. Pause
5. Check "Remaining" hours (should be ~167.98h)
6. Start new session
7. Check timer (should start from ~167:58:00)

**If ANY step fails, you've found the issue!**

---

## 📱 Quick Verification Commands

### In Browser Console:

**Check subscription data:**
```javascript
// After any action, check:
console.log('Current subs:', 
  JSON.parse(localStorage.getItem('subscriptions') || '[]')
);
```

**Force refetch:**
```javascript
// Manually trigger refetch
window.location.reload();
```

---

## ✅ Expected Results Summary

| Action | Backend DB | Frontend Display | Timer |
|--------|-----------|------------------|-------|
| Start (168h) | 168h | 168.00h | 168:00:00 |
| After 30 min | 168h | 168.00h | 167:30:00 |
| Pause | 167.5h ✅ | 167.50h ✅ | N/A |
| Start again | 167.5h | 167.50h ✅ | 167:30:00 ✅ |
| After 1 hour | 167.5h | 167.50h | 166:30:00 |
| Pause | 166.5h ✅ | 166.50h ✅ | N/A |

**All should match! If any don't match, that's where the problem is.**

---

## 🚨 Most Likely Issue

Based on your description, the most likely issue is:

**The timer is starting from the subscription's ORIGINAL total hours instead of CURRENT remaining hours.**

**Fix Location:** `UserSessionManagement.tsx` line ~256

**Current Code:**
```typescript
<SubscriptionTimer
  startTime={sessionData.startTime}
  remainingHours={userSubscription?.remainingHours}
  // ↑ This should be the UPDATED value
```

**Debug:** Add console.log right before:
```typescript
console.log('Starting timer with:', {
  startTime: sessionData.startTime,
  remainingHours: userSubscription?.remainingHours,
  subscriptionId: userSubscription?.id
});
```

**Expected output:**
```
Starting timer with: {
  startTime: "2024-11-08T14:00:00Z",
  remainingHours: 167.5,  ← Should be UPDATED value, not 168
  subscriptionId: "subscription-guid"
}
```

**If it shows 168 instead of 167.5, the subscription data is not being refreshed properly!**

---

**Date:** November 8, 2025  
**Purpose:** Debug hours deduction issues  
**Expected:** Hours deducted on pause, new session uses updated hours  
**Tools:** DevTools Network tab, Console logs, Database queries

