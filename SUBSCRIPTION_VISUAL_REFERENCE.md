# 🎯 Subscription System - Visual Quick Reference

## New Admin Menu Structure

```
┌─────────────────────────────────────────┐
│ 📊 DASHBOARD                            │
│    Overview & Statistics                │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ 🔵 MAIN WORKSPACE                       │
├─────────────────────────────────────────┤
│ 👥 User & Sessions ⭐                   │
│    ↳ Assign tables                      │
│    ↳ Start sessions                     │
│    ↳ Pause sessions                     │
│    ↳ See active users                   │
│    ↳ Track hours                        │
│                                         │
│ 💡 USE THIS PAGE ALL DAY!               │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ 📦 SUBSCRIPTION SETUP                   │
├─────────────────────────────────────────┤
│ 📦 Subscription Packages                │
│    ↳ Create packages                    │
│    ↳ Edit packages                      │
│    ↳ Set availability                   │
│                                         │
│ 💳 Purchase for Users                   │
│    ↳ Buy packages for customers         │
│    ↳ Process payments                   │
│    ↳ View all subscriptions             │
│                                         │
│ 💵 Rate Management                      │
│    ↳ Set hourly rates                   │
│    ↳ Define day/week/month pricing      │
│    ↳ Manage pricing structure           │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ ⚙️  SYSTEM                              │
├─────────────────────────────────────────┤
│ 🖥️  Table Setup                         │
│    ↳ Add/remove tables                  │
│    ↳ Configure capacity                 │
│    ↳ Set table properties               │
│                                         │
│ 📋 Transaction History                  │
│    ↳ View all purchases                 │
│    ↳ Search transactions                │
│    ↳ Export records                     │
│                                         │
│ 👤 User Accounts                        │
│    ↳ Manage users                       │
│    ↳ Set permissions                    │
│    ↳ View user details                  │
│                                         │
│ 📊 Reports                              │
│    ↳ Daily/weekly/monthly               │
│    ↳ Revenue reports                    │
│    ↳ Usage statistics                   │
│                                         │
│ 🔔 Notifications                        │
│    ↳ Session alerts                     │
│    ↳ System messages                    │
│                                         │
│ ⚙️  Settings                            │
│    ↳ System configuration               │
│    ↳ Global settings                    │
│                                         │
│ 👤 Profile                              │
│    ↳ Admin profile                      │
│    ↳ Account settings                   │
└─────────────────────────────────────────┘
```

---

## Daily Workflow Map

```
START YOUR DAY
      ↓
┌─────────────────┐
│  📊 Dashboard   │ ← Check stats
└─────────────────┘
      ↓
┌─────────────────────────┐
│ 👥 User & Sessions ⭐   │ ← STAY HERE!
└─────────────────────────┘
      ↓
   Keep open all day
      ↓
┌─────────────────────────────┐
│ Customer Arrives            │
├─────────────────────────────┤
│ 1. Search name              │
│ 2. See remaining hours      │
│ 3. [Assign Table]           │
│ 4. Select table             │
│ 5. [Start Session]          │
└─────────────────────────────┘
      ↓
┌─────────────────────────────┐
│ Customer Active             │
├─────────────────────────────┤
│ → Shows in "Active Sessions"│
│ → Hours counting down       │
│ → Table marked occupied     │
└─────────────────────────────┘
      ↓
┌─────────────────────────────┐
│ Customer Leaves             │
├─────────────────────────────┤
│ 1. Find in active list      │
│ 2. [Pause & Save]           │
│ 3. Hours saved              │
│ 4. Table freed              │
└─────────────────────────────┘
      ↓
   Repeat for each customer
```

---

## Setup Tasks (Do Once)

```
FIRST TIME SETUP
      ↓
┌──────────────────────┐
│ 💵 Rate Management   │
├──────────────────────┤
│ Set base prices:     │
│ • Hourly: ₱50        │
│ • Daily: ₱1,000      │
│ • Weekly: ₱5,000     │
│ • Monthly: ₱15,000   │
└──────────────────────┘
      ↓
┌──────────────────────────┐
│ 📦 Subscription Packages │
├──────────────────────────┤
│ Create packages:         │
│ • 1 Hour                 │
│ • 3 Hours                │
│ • 1 Day                  │
│ • 1 Week                 │
│ • 1 Month                │
└──────────────────────────┘
      ↓
┌──────────────────────┐
│ 🖥️  Table Setup      │
├──────────────────────┤
│ Add tables:          │
│ • Table 1 (2 seats)  │
│ • Table 2 (4 seats)  │
│ • Table 3 (2 seats)  │
│ • etc.               │
└──────────────────────┘
      ↓
✅ READY TO OPERATE!
```

---

## Customer Journey Flowchart

```
NEW CUSTOMER
      ↓
┌─────────────────────────┐
│ 💳 Purchase for Users   │
├─────────────────────────┤
│ 1. Select customer      │
│ 2. Choose package       │
│ 3. Process payment      │
│ 4. ✅ Package purchased │
└─────────────────────────┘
      ↓
┌─────────────────────────┐
│ Hours saved to account  │
│ Status: Active          │
│ Remaining: Full hours   │
└─────────────────────────┘
      ↓
┌─────────────────────────┐
│ 👥 User & Sessions      │
├─────────────────────────┤
│ Customer now appears in │
│ "Available" list        │
└─────────────────────────┘
      ↓
┌─────────────────────────┐
│ Assign → Start → Use    │
│ Pause → Save → Leave    │
│ Return → Assign → Resume│
└─────────────────────────┘
      ↓
   Repeat until hours consumed
      ↓
┌─────────────────────────┐
│ Hours depleted          │
│ → Buy new package       │
│ → Repeat cycle          │
└─────────────────────────┘
```

---

## Feature Matrix

| Feature | Location | Purpose | Use When |
|---------|----------|---------|----------|
| **Assign Table** | User & Sessions | Start customer session | Customer arrives |
| **Pause Session** | User & Sessions | Save hours, free table | Customer leaves |
| **Create Package** | Subscription Packages | Define offering | Setting up system |
| **Buy for User** | Purchase for Users | Sell package | Walk-in customer |
| **Set Rates** | Rate Management | Define pricing | Setting up system |
| **Add Table** | Table Setup | Expand capacity | Adding furniture |
| **View History** | Transaction History | See purchases | Reporting/audit |
| **Manage Users** | User Accounts | Account admin | User management |
| **Generate Report** | Reports | Financial data | End of day/week/month |

---

## Color Coding Guide

```
🔵 Blue Section = MAIN WORKSPACE
   → Use daily, keep open
   → Primary operations

📦 Orange Section = SUBSCRIPTION SETUP
   → Configuration tasks
   → Set up once, adjust as needed

⚙️  Gray Section = SYSTEM
   → Supporting functions
   → Administrative tasks
```

---

## Common Scenarios

### Scenario 1: Morning Rush (10 customers)
```
1. Open "User & Sessions"
2. For each customer:
   • Search name
   • [Assign Table]
   • Select table
   • [Start Session]
3. All 10 seated in < 3 minutes
```

### Scenario 2: Lunch Break (5 leaving)
```
1. See 5 in "Active Sessions"
2. For each:
   • [Pause & Save]
3. All 5 paused in < 1 minute
4. 5 tables now free
```

### Scenario 3: New Walk-in Customer
```
1. "Purchase for Users"
2. Select customer
3. Choose "1 Week" package
4. Payment: ₱5,000
5. [Purchase]
6. Back to "User & Sessions"
7. [Assign Table]
8. [Start Session]
9. Customer active!
```

### Scenario 4: Returning Customer
```
1. Customer: "Hi, I'm John"
2. Search: "John"
3. See: 143 hours remaining
4. [Assign Table]
5. Choose Table 3
6. [Start Session]
7. Continues from 143 hours!
```

---

## Page Purpose Quick Guide

| Page | Answer This | Primary Action |
|------|-------------|----------------|
| **User & Sessions** | Who's here? Who's using tables? | Assign/Pause |
| **Subscription Packages** | What packages do we offer? | Create/Edit |
| **Purchase for Users** | Buy package for customer | Purchase |
| **Rate Management** | How much do we charge? | Set Prices |
| **Table Setup** | What tables do we have? | Add/Configure |
| **Transaction History** | What was sold? | View/Search |
| **User Accounts** | Who are our customers? | Manage |
| **Reports** | How much revenue? | Generate |

---

## Navigation Shortcuts

### Fastest Path to Common Tasks:

**Start a session:**
```
User & Sessions → Search → Assign → Start
```

**Pause a session:**
```
User & Sessions → Active Sessions → Pause
```

**Buy package:**
```
Purchase for Users → Select → Purchase
```

**Add table:**
```
Table Setup → Add New → Save
```

**View sales:**
```
Transaction History → Filter → View
```

**Create package:**
```
Subscription Packages → Add → Define → Save
```

---

## Tips for Efficiency

### ⭐ Keep "User & Sessions" Open
- Bookmark it
- Default page for your day
- All customer operations here

### 📱 Use Search
- Faster than scrolling
- Type partial name
- Instant results

### 🎯 One-Click Actions
- [Assign Table] → Quick assign
- [Pause & Save] → Instant pause
- Minimal clicks needed

### 📊 Check Stats
- Top of User & Sessions
- Active users count
- Tables available
- Total hours remaining

---

**🎯 REMEMBER:**

**Main Workspace = User & Sessions**
- Stay here all day!
- All customer operations
- Quick and efficient

**Subscription Setup = Configuration**
- Set up once
- Adjust as needed
- Define offerings

**System = Supporting Tasks**
- Administrative work
- Reporting
- Management

---

**Date:** November 8, 2025  
**Version:** 2.0  
**Focus:** Subscription-Based Workflow

