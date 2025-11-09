# Subscription System - Quick Reference

## Common Scenarios & Solutions

### Scenario 1: Customer wants to study 1 week straight
**Solution:** Weekly Package
- Package: "1 Week Package" 
- Hours: 168 hours (7 days × 24 hours)
- Price: ₱5,000
- They can use any amount per day until hours run out

### Scenario 2: Student studying for board exams (2 months)
**Solution:** 2× Monthly Packages or Custom Package
- Package: "1 Month Package" × 2
- Hours: 1,440 hours (60 days × 24 hours)
- Price: ₱30,000 (or discounted rate)
- Can spread usage across 2+ months

### Scenario 3: Freelancer who comes 3-4 times a week
**Solution:** Monthly Package with flexible usage
- Package: "1 Month Package"
- Hours: 720 hours
- Usage: ~8 hours × 4 days × 4 weeks = 128 hours/month
- Remaining hours carry over to next month

### Scenario 4: Customer comes back after 2 days
**Admin Actions:**
1. Check customer's active subscriptions
2. Select available table
3. Start subscription session
4. System automatically deducts time from remaining hours

## Quick API Reference

### Admin: Add Subscription to User
```bash
curl -X POST https://your-api.com/api/subscriptions/admin/purchase \
  -H "Authorization: Bearer {admin_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "customer-guid",
    "packageId": "weekly-package-guid",
    "paymentMethod": "Cash",
    "cash": 5000,
    "change": 0
  }'
```

### Admin: Start Subscription Session for User
```bash
curl -X POST https://your-api.com/api/tables/sessions/start-subscription \
  -H "Authorization: Bearer {admin_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "tableId": "table-1-guid",
    "subscriptionId": "user-subscription-guid",
    "userId": "customer-guid"
  }'
```

### Admin: End Session
```bash
curl -X POST https://your-api.com/api/tables/sessions/end-subscription \
  -H "Authorization: Bearer {admin_token}" \
  -H "Content-Type: application/json" \
  -d '"session-guid"'
```

### Check Customer's Remaining Hours
```bash
curl -X GET https://your-api.com/api/subscriptions/admin/user/{userId} \
  -H "Authorization: Bearer {admin_token}"
```

## Package Recommendations

### For Regular Customers (Daily Visits)
- **Monthly Package**: Best value
- **Usage**: 8 hours/day × 22 days = 176 hours/month
- **Remaining**: ~544 hours for extra days or longer sessions

### For Exam Cramming (1-2 weeks)
- **Weekly Package**: Perfect fit
- **Usage**: 12 hours/day × 7 days = 84 hours
- **Remaining**: 84 hours for second week

### For Occasional Users
- **10-Hour Package**: Entry-level option
- **Usage**: 2-3 visits
- **When depleted**: Can purchase another or upgrade

### For Long-term Projects (3+ months)
- **Multiple Monthly Packages**: Most economical
- **Option**: Create custom 3-month or 6-month packages
- **Discount**: 10-15% off for bulk purchases

## Time Calculation Reference

| Package Type | Duration | Total Hours | Example Price |
|--------------|----------|-------------|---------------|
| 10 Hours     | N/A      | 10          | ₱500          |
| 1 Day        | 1 day    | 24          | ₱1,000        |
| 1 Week       | 7 days   | 168         | ₱5,000        |
| 2 Weeks      | 14 days  | 336         | ₱9,000        |
| 1 Month      | 30 days  | 720         | ₱15,000       |
| 3 Months     | 90 days  | 2,160       | ₱40,000       |

## Common Questions

**Q: Can a user have multiple active subscriptions?**
A: Yes! They can purchase multiple packages and choose which one to use per session.

**Q: What happens if user doesn't finish their hours in the package "duration"?**
A: Hours never expire unless you set an expiry_date. A "1 Month Package" means 720 hours of usage, not 1 calendar month.

**Q: Can user switch tables mid-session?**
A: Yes! Use the change-table endpoint. Time continues counting.

**Q: How is time tracked?**
A: By actual session duration. Start session → End session → Calculate hours → Deduct from remaining.

**Q: What if user leaves without ending session?**
A: Background service checks for expired sessions and auto-ends them after scheduled time.

**Q: Can admin manually adjust remaining hours?**
A: Not directly via API yet. You can update database directly or create a new subscription with adjusted hours.

## Sample Package Setup

### Budget Package
```json
{
  "name": "10 Hours Starter",
  "packageType": "Hourly",
  "durationValue": 10,
  "totalHours": 10,
  "price": 500,
  "description": "Perfect for trying out our service",
  "displayOrder": 1
}
```

### Popular Package (Best Value)
```json
{
  "name": "1 Week Unlimited",
  "packageType": "Weekly",
  "durationValue": 1,
  "totalHours": 168,
  "price": 5000,
  "description": "Most popular! 7 days of study time",
  "displayOrder": 3
}
```

### Premium Package
```json
{
  "name": "1 Month Premium",
  "packageType": "Monthly",
  "durationValue": 1,
  "totalHours": 720,
  "price": 15000,
  "description": "Best for serious students and professionals",
  "displayOrder": 4
}
```

## Troubleshooting

### Issue: "No remaining hours"
- Check subscription status
- Verify hours_used vs total_hours
- Consider offering package renewal

### Issue: "Subscription not found"
- Ensure subscription belongs to the user
- Check if subscription status is "Active"
- Verify subscriptionId is correct

### Issue: "Table already occupied"
- Check table's is_occupied status
- Look for active sessions on that table
- Assign a different available table

### Issue: Time not deducting correctly
- Verify session end was called
- Check session.hours_consumed field
- Review subscription.hours_used calculation

## Pro Tips

1. **Offer Package Bundles**: Buy 2 months, get 10% off
2. **Loyalty Rewards**: 5th package gets 15% discount
3. **Referral Bonus**: Refer a friend, both get +10 hours
4. **Peak/Off-Peak**: Different rates for daytime vs nighttime hours
5. **Study Group Packages**: 4-person package with shared hours

