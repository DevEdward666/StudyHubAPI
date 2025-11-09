# Rate Management - Quick User Guide

## Creating Different Rate Types

### Hourly Rates
**Best for:** Short study sessions, drop-ins

1. Duration Type: **Hourly**
2. Duration Value: **1** (or 2, 3, etc.)
3. Hours: Auto-calculates (1, 2, 3...)
4. Price: Your hourly rate

**Example:**
- Duration Type: Hourly
- Value: 3
- **Result:** "3 Hours" @ 168 total hours
- Price: ₱120

---

### Daily Rates
**Best for:** Full day sessions, exam preparation

1. Duration Type: **Daily**
2. Duration Value: **1** (or 2, 3, etc.)
3. Hours: Auto-calculates (24, 48, 72...)
4. Price: Your daily rate

**Example:**
- Duration Type: Daily
- Value: 1
- **Result:** "1 Day" @ 24 total hours
- Price: ₱1,000

---

### Weekly Rates
**Best for:** Extended study periods, exam weeks

1. Duration Type: **Weekly**
2. Duration Value: **1** (or 2, etc.)
3. Hours: Auto-calculates (168, 336...)
4. Price: Your weekly rate

**Example:**
- Duration Type: Weekly
- Value: 1
- **Result:** "1 Week" @ 168 total hours
- Price: ₱5,000

---

### Monthly Rates
**Best for:** Long-term students, regular customers

1. Duration Type: **Monthly**
2. Duration Value: **1** (or 2, 3, etc.)
3. Hours: Auto-calculates (720, 1440, 2160...)
4. Price: Your monthly rate

**Example:**
- Duration Type: Monthly
- Value: 1
- **Result:** "1 Month" @ 720 total hours
- Price: ₱15,000

---

## Quick Reference Table

| Type | Value | Total Hours | Example Price |
|------|-------|-------------|---------------|
| **Hourly** | 1 | 1 | ₱50 |
| **Hourly** | 3 | 3 | ₱120 |
| **Daily** | 1 | 24 | ₱1,000 |
| **Daily** | 3 | 72 | ₱2,500 |
| **Weekly** | 1 | 168 | ₱5,000 |
| **Weekly** | 2 | 336 | ₱9,000 |
| **Monthly** | 1 | 720 | ₱15,000 |
| **Monthly** | 3 | 2,160 | ₱40,000 |

---

## Tips for Pricing

### Encourage Longer Stays:
- Hourly: ₱50/hour = ₱50 per hour
- Daily: ₱1,000/day = ₱41.67 per hour (17% savings)
- Weekly: ₱5,000/week = ₱29.76 per hour (40% savings)
- Monthly: ₱15,000/month = ₱20.83 per hour (58% savings)

### Recommended Display Order:
1. Hourly rates (display order 1-9)
2. Daily rates (display order 10-19)
3. Weekly rates (display order 20-29)
4. Monthly rates (display order 30-39)

---

## Common Questions

**Q: What if I change duration type?**
A: Hours automatically recalculate. Just pick the new type and value.

**Q: Can I have multiple weekly rates?**
A: Yes! For example: 1 week @ ₱5,000 and 2 weeks @ ₱9,000

**Q: What's the maximum duration?**
A: Up to 365 for any type (365 hours, 365 days, etc.)

**Q: Do I need to calculate hours manually?**
A: No! The system calculates automatically based on your selection.

---

**Need Help?** Check `FRONTEND_RATE_DURATION_IMPLEMENTATION.md` for full details

