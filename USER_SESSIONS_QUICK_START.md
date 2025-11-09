# ✅ User & Sessions - Quick Start Guide

## Problem Fixed
❌ **Before:** Clicking "Start Session" and "Pause & Save" did nothing  
✅ **Now:** Fully functional - sessions start/pause and sync with backend

---

## How to Use (Admin)

### Step 1: Access the Page
```
Login as Admin → Sidebar → "User & Sessions" (2nd item, highlighted)
```

### Step 2: Start a Session

**When customer arrives:**
1. Search for customer name in the search bar
2. Find the customer in "Users with Active Hours" section
3. Click **[Assign Table]**
4. Select an available table from dropdown
5. Click **[Start Session]**

**Result:**
- ✅ Customer appears in "Active Sessions" (green section at top)
- ✅ Table marked as occupied
- ✅ Hours start counting down
- ✅ Success message shown

### Step 3: Pause a Session

**When customer leaves:**
1. Find customer in "Active Sessions" section (green cards at top)
2. Click **[Pause & Save]**

**Result:**
- ✅ Hours used calculated and deducted
- ✅ Remaining hours saved to customer account
- ✅ Customer moves back to "Available" list
- ✅ Table freed for others
- ✅ Success message: "Session paused! Hours saved for {name}"

### Step 4: Customer Returns Later

**When same customer comes back:**
1. Search for customer name
2. See their updated remaining hours
3. Click **[Assign Table]** (can assign different table)
4. Click **[Start Session]**

**Result:**
- ✅ Session continues from saved hours
- ✅ No time lost!

---

## Real Example

### Day 1 - Morning
```
Customer: John Doe buys "1 Week" (168 hours) for ₱5,000
         Remaining: 168 hours

Admin: User & Sessions → Find "John Doe"
       [Assign Table] → Select "Table 1" → [Start Session]
       ✅ Session started!

10:00 AM - Session starts
12:30 PM - Customer leaves (2.5 hours used)

Admin: Find in "Active Sessions" → [Pause & Save]
       ✅ Hours saved! Remaining: 165.5 hours
       ✅ Table 1 now free
```

### Day 1 - Afternoon
```
Customer: John Doe returns
         Remaining: 165.5 hours

Admin: Search "John" → Shows 165.5 hours remaining
       [Assign Table] → Select "Table 5" (Table 1 taken)
       [Start Session]
       ✅ Session continues!

3:00 PM - Session starts
5:00 PM - Customer leaves (2 hours used)

Admin: [Pause & Save]
       ✅ Remaining: 163.5 hours saved
```

### Day 2
```
Customer: John Doe returns
         Remaining: 163.5 hours

Admin: Repeat assign → start → pause cycle
       Hours continue counting down
       Table can be different each time
       
... continues until 168 hours consumed
```

---

## Visual Guide

### Before Starting Session:
```
👥 USERS WITH ACTIVE HOURS
┌──────────────────────────────────┐
│ 👤 John Doe                       │
│    📦 1 Week Premium              │
│    💰 168.0 / 168 hours left      │
│    [████████████] 0% used        │
│    [▶️ Assign Table]              │  ← Click here
└──────────────────────────────────┘
```

### After Starting Session:
```
🟢 ACTIVE SESSIONS
┌──────────────────────────────────┐
│ 🟢 John Doe - Table 1             │
│    Started: 10:00 AM              │
│    Subscription: 1 Week Premium   │
│    [⏸️ Pause & Save]              │  ← Click here to pause
└──────────────────────────────────┘
```

### After Pausing:
```
👥 USERS WITH ACTIVE HOURS
┌──────────────────────────────────┐
│ 👤 John Doe                       │
│    📦 1 Week Premium              │
│    💰 165.5 / 168 hours left      │  ← Hours updated!
│    [██████████░░] 1.5% used      │
│    [▶️ Assign Table]              │  ← Can assign again
└──────────────────────────────────┘
```

---

## Common Questions

**Q: What if all tables are occupied?**
A: The "Assign Table" button will be disabled. Customer must wait for a table to become free.

**Q: Can I assign the same table again?**
A: Yes! If Table 1 becomes free, you can assign it to anyone (same customer or different).

**Q: What happens to hours when paused?**
A: Hours used are calculated and deducted from subscription. Remaining hours saved to customer account.

**Q: Can customer use different tables?**
A: Yes! Each time you assign, you can choose any available table.

**Q: How do I know which customers have hours left?**
A: Look at "Users with Active Hours" section - shows everyone with remaining hours.

**Q: What if customer's hours run out?**
A: They won't appear in the available list. They need to purchase a new package.

---

## Tips

### Efficient Workflow:
1. Keep "User & Sessions" page open all day
2. Use search to quickly find returning customers
3. Check "Active Sessions" section to see who's using tables
4. Click "Pause & Save" when customers leave
5. Stats at top show: Active Users, In Session, Tables Free

### Best Practices:
- ✅ Always pause when customer leaves (saves their hours!)
- ✅ Search by name for returning customers
- ✅ Check remaining hours before assigning
- ✅ Use different tables if preferred ones are occupied
- ✅ Keep an eye on "Tables Available" stat

---

## Troubleshooting

**Issue: "Start Session" button does nothing**
- Check: Is backend running? (`dotnet run`)
- Check: Browser console for errors (F12)
- Check: Network tab shows API call

**Issue: "No tables available"**
- Solution: Wait for customer to leave, then pause their session
- Or: Add more tables in Table Management

**Issue: Customer not showing in list**
- Check: Do they have an active subscription?
- Check: Do they have remaining hours > 0?
- Solution: Purchase a package for them first

**Issue: Hours not updating after pause**
- Solution: Refresh page (Cmd/Ctrl + R)
- Check: Network tab - did API call succeed?

---

## Status Indicators

**🟢 Active Sessions** - Customers currently using tables  
**👥 Users with Active Hours** - Customers available to assign  
**[▶️ Assign Table]** - Green button - ready to assign  
**[⏸️ Pause & Save]** - Yellow button - pause active session  
**Progress Bar Colors:**
- Green (0-80% used) - Plenty of time
- Red (80-100% used) - Running low

---

**🎉 You're Ready!**

The system is fully functional. Just:
1. Open "User & Sessions"
2. Assign tables when customers arrive
3. Pause when they leave
4. That's it!

---

**Date:** November 8, 2025  
**Status:** ✅ FULLY FUNCTIONAL  
**Location:** Admin → User & Sessions

