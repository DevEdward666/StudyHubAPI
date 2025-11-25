# Dual Expiration Logic Implementation

## Overview
Sessions now end based on **EITHER** condition being met:
1. **Hours Consumed**: Total hours used up (existing logic)
2. **Expiry Date Reached**: Subscription period ended (new logic)

## Example Scenario

### Monthly Package - 1 Month
- **Purchase Date**: November 25, 2025 09:16 PM
- **Expiry Date**: December 25, 2025 09:16 PM (auto-calculated)
- **Total Hours**: 100 hours
- **Package Type**: Monthly

### Expiration Scenarios

#### Scenario A: Hours Run Out First
- Customer uses 100 hours in 2 weeks (December 9, 2025)
- **Session Ends**: December 9, 2025 (when hours = 0)
- **Reason**: "Hours depleted"
- **Expiry Date**: Still December 25, but irrelevant since hours are gone

#### Scenario B: Date Expires First
- Customer uses only 30 hours by December 25, 2025 09:16 PM
- **Session Ends**: December 25, 2025 09:16 PM (exact expiry time)
- **Reason**: "Subscription period expired"
- **Remaining Hours**: 70 hours (unused, lost)

#### Scenario C: Both Expire Simultaneously
- Customer uses exactly 100 hours at exactly December 25, 2025 09:16 PM
- **Session Ends**: December 25, 2025 09:16 PM
- **Reason**: "Hours depleted & Subscription expired"

## Implementation Details

### Backend Changes

#### 1. SubscriptionService.cs - Auto-Calculate Expiry Date

When purchasing a subscription, expiry date is automatically set:

```csharp
switch (package.PackageType.ToLower())
{
    case "daily":
        expiryDate = purchaseDate.AddDays(package.DurationValue);
        break;
    case "weekly":
        expiryDate = purchaseDate.AddDays(package.DurationValue * 7);
        break;
    case "monthly":
        expiryDate = purchaseDate.AddMonths(package.DurationValue);
        break;
    case "hourly":
        // Hourly packages don't have expiry date, only hours limit
        expiryDate = null;
        break;
}
```

**Examples**:
- **1 Week Package**: PurchaseDate + 7 days
- **2 Weeks Package**: PurchaseDate + 14 days
- **1 Month Package**: PurchaseDate + 1 month (handles different month lengths)
- **3 Months Package**: PurchaseDate + 3 months

#### 2. SessionExpiryChecker.cs - Dual Check Logic

The background job (runs every 1 minute) now checks BOTH conditions:

```csharp
bool shouldEndSession = false;
string endReason = "";

// Check 1: Hours consumed
var effectiveRemainingHours = subscription.RemainingHours - sessionElapsedHours;
if (effectiveRemainingHours <= 0)
{
    shouldEndSession = true;
    endReason = "Hours depleted";
}

// Check 2: Expiry date reached
if (subscription.ExpiryDate.HasValue && now >= subscription.ExpiryDate.Value)
{
    shouldEndSession = true;
    endReason = shouldEndSession && endReason != "" 
        ? "Hours depleted & Subscription expired" 
        : "Subscription period expired";
}

// End session if EITHER condition is met
if (shouldEndSession)
{
    // End the session and send notification
}
```

#### 3. Enhanced Notifications

Notifications now show the specific reason:
- "Subscription Session Ended - Hours Depleted"
- "Subscription Session Ended - Period Expired"
- "Subscription Session Ended" (both conditions)

### Frontend Changes

#### 1. UserSubscriptionManagement.tsx - Display Expiry Date

Shows expiry date with color coding:
- 🔵 Blue: Future expiry (Expires on: Dec 25, 2025 9:16 PM)
- 🔴 Red: Past expiry (Expired on: Dec 25, 2025 9:16 PM)

```tsx
{sub.expiryDate && (
  <p style={{ color: new Date(sub.expiryDate) < new Date() ? "#d32f2f" : "#1976d2" }}>
    {new Date(sub.expiryDate) < new Date() ? "Expired on: " : "Expires on: "}
    {new Date(sub.expiryDate).toLocaleDateString()} 
    {new Date(sub.expiryDate).toLocaleTimeString()}
  </p>
)}
```

#### 2. UserSessionManagement.tsx - Show Expiry in Sessions

Active sessions now display:
- User name
- Table number
- Package name
- Start time
- **Expiry date** (if applicable)
- Real-time countdown timer

## Package Type Behaviors

### Hourly Packages
- **Expiry Date**: NULL (no time limit)
- **Ends When**: Hours consumed = Total hours
- **Example**: 10-hour package ends when 10 hours used (could take days/weeks)

### Daily Packages
- **Expiry Date**: Purchase date + N days
- **Ends When**: Hours consumed = 0 OR Current time >= Expiry date
- **Example**: 1-day package (24 hours) purchased Nov 25 9:16 PM
  - Expires: Nov 26 9:16 PM
  - If user uses 24 hours in 12 hours → Ends at 9:16 AM Nov 26
  - If user uses 10 hours → Ends at 9:16 PM Nov 26 (date limit)

### Weekly Packages
- **Expiry Date**: Purchase date + (N × 7) days
- **Ends When**: Hours consumed = 0 OR Current time >= Expiry date
- **Example**: 2-week package purchased Nov 25 9:16 PM
  - Expires: Dec 9 9:16 PM
  - 168 hours total (7 days × 24 hours)

### Monthly Packages
- **Expiry Date**: Purchase date + N months
- **Ends When**: Hours consumed = 0 OR Current time >= Expiry date
- **Example**: 1-month package purchased Nov 25 9:16 PM
  - Expires: Dec 25 9:16 PM
  - Handles month length differences (30/31 days)

## Database Fields

### UserSubscription Table
- `purchase_date`: When subscription was bought
- `expiry_date`: When subscription period ends (NULL for hourly)
- `total_hours`: Total hours in package
- `remaining_hours`: Hours left to use
- `hours_used`: Hours already consumed
- `status`: Active/Expired/Cancelled

## Background Job Logic

### Every 1 Minute
1. Query all active subscription sessions
2. For each session:
   - Calculate hours used in current session
   - Calculate effective remaining hours
   - Check if hours depleted: `effectiveRemainingHours <= 0`
   - Check if date expired: `now >= expiryDate`
   - If EITHER is true → End session
3. Update subscription status
4. Free table
5. Send notification with specific reason

## User Experience

### When Creating Subscription
Admin sees calculated expiry date in purchase confirmation:
- "1 Month Package - Expires: Dec 25, 2025 9:16 PM"

### During Active Session
User/Admin sees:
- Real-time countdown timer (hours:minutes:seconds)
- Expiry date countdown
- Both indicators update every second

### When Session Ends
Notification shows:
- Which condition triggered the end
- Final hours used
- Remaining hours (if date expired first)

## Edge Cases Handled

### 1. Leap Year
`AddMonths()` handles February correctly:
- Purchase: Jan 31, 2024 → Expires: Feb 29, 2024 (leap year)
- Purchase: Jan 31, 2025 → Expires: Feb 28, 2025 (non-leap)

### 2. Month Length Differences
`AddMonths()` handles:
- Jan 31 + 1 month = Feb 28/29 (last day of February)
- Jan 30 + 1 month = Feb 28/29
- Jan 15 + 1 month = Feb 15

### 3. Daylight Saving Time
UTC timestamps prevent DST issues:
- All dates stored in UTC
- Converted to local time for display

### 4. Session Pause/Resume
When user pauses:
- Hours consumed are deducted from remaining hours
- Expiry date unchanged (time keeps ticking)
- When resumed, expiry date check still applies

### 5. Zero Hours Remaining
If hours hit exactly 0:
- Session ends immediately
- Don't wait for next cron job cycle
- Frontend timer shows "No Hours Left"

### 6. Expired Subscription, Hours Remaining
If date expired but hours remain:
- Session still ends (date takes priority)
- Unused hours are lost
- User notified of expiry

## Testing Scenarios

### Test 1: Hours Depleted First
1. Create 1-month package with 1 hour
2. Start session
3. Wait 1 hour
4. Session ends due to hours depleted
5. Expiry date still 1 month away

### Test 2: Date Expired First
1. Create 1-day package with 1000 hours
2. Start session
3. Wait 24 hours
4. Session ends due to date expiry
5. ~976 hours unused (lost)

### Test 3: Short Monthly Package
1. Create 1-month package purchased today (Nov 25, 2025 9:16 PM)
2. Use 50 hours in first week
3. Leave remaining hours unused
4. On Dec 25, 2025 9:16 PM → Session auto-ends
5. Notification: "Subscription period expired"

## Benefits

### 1. Fair Time Limits
- Monthly packages truly last 1 month
- Prevents indefinite subscription extension

### 2. Flexible Usage
- Hourly packages: No time pressure, pay for hours only
- Monthly packages: Use anytime within the month

### 3. Clear Expectations
- Users see exact expiry date when purchasing
- No confusion about when access ends

### 4. Accurate Billing
- Hours tracked precisely
- Time limits enforced automatically

## Migration Notes

### Existing Subscriptions
- Current subscriptions have NULL expiry date
- Will only expire when hours consumed
- To add expiry date: Update record manually

### New Subscriptions
- Auto-calculate expiry date on purchase
- Both conditions apply immediately

## Files Modified

### Backend
- `Study-Hub/Service/SubscriptionService.cs` - Auto-calculate expiry date
- `Study-Hub/Services/Background/SessionExpiryChecker.cs` - Dual check logic

### Frontend
- `study_hub_app/src/pages/UserSubscriptionManagement.tsx` - Display expiry date
- `study_hub_app/src/pages/UserSessionManagement.tsx` - Display expiry date

## Summary

✅ **Hours-based expiration**: Prevents overuse
✅ **Date-based expiration**: Ensures fair time limits
✅ **Dual condition**: Session ends when EITHER is met
✅ **Auto-calculated**: No manual date entry needed
✅ **Clear notifications**: Users know why session ended
✅ **Flexible packages**: Hourly (no date) vs Monthly (has date)

The system now supports **true monthly/weekly subscriptions** where the package expires on a specific date, regardless of hours remaining! 🎉

