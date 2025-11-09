# Subscription System - Quick Start Guide

## 🚀 Getting Started in 5 Minutes

### Step 1: Apply Migration
```bash
cd Study-Hub
dotnet ef database update
```

### Step 2: Seed Initial Packages (Optional)
```bash
cd ..
./seed-subscription-packages.sh
```

Or manually via SQL:
```sql
INSERT INTO subscription_packages (id, name, package_type, duration_value, total_hours, price, description, is_active, display_order, created_at, updated_at)
VALUES 
  (gen_random_uuid(), '1 Week Unlimited', 'Weekly', 1, 168, 5000, 'One week of study time', true, 1, NOW(), NOW()),
  (gen_random_uuid(), '1 Month Premium', 'Monthly', 1, 720, 15000, 'One month unlimited', true, 2, NOW(), NOW());
```

### Step 3: Test the API

#### 3.1 Create a Package (Admin)
```bash
curl -X POST http://localhost:5212/api/subscriptions/packages \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "1 Week Package",
    "packageType": "Weekly",
    "durationValue": 1,
    "totalHours": 168,
    "price": 5000,
    "description": "Perfect for exam week",
    "displayOrder": 1
  }'
```

#### 3.2 Get All Packages
```bash
curl http://localhost:5212/api/subscriptions/packages?activeOnly=true
```

#### 3.3 Admin Purchases Subscription for User
```bash
curl -X POST http://localhost:5212/api/subscriptions/admin/purchase \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "USER_GUID",
    "packageId": "PACKAGE_GUID",
    "paymentMethod": "Cash",
    "cash": 5000,
    "change": 0,
    "notes": "Customer wants 1 week access"
  }'
```

#### 3.4 Check User's Subscriptions
```bash
curl http://localhost:5212/api/subscriptions/admin/user/USER_GUID \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

#### 3.5 Start Subscription Session
```bash
curl -X POST http://localhost:5212/api/tables/sessions/start-subscription \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "tableId": "TABLE_GUID",
    "subscriptionId": "SUBSCRIPTION_GUID",
    "userId": "USER_GUID"
  }'
```

#### 3.6 End Session (After User Leaves)
```bash
curl -X POST http://localhost:5212/api/tables/sessions/end-subscription \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '"SESSION_GUID"'
```

## 📱 Real-World Example

### Scenario: Sarah buys a weekly package

**Day 1 - Purchase:**
```bash
# Admin creates subscription for Sarah
POST /api/subscriptions/admin/purchase
{
  "userId": "sarah-guid",
  "packageId": "weekly-package-guid",
  "paymentMethod": "GCash",
  "transactionReference": "GCash-12345"
}

# Response: Subscription created
{
  "id": "sub-guid",
  "totalHours": 168,
  "remainingHours": 168,
  "status": "Active"
}
```

**Day 1 - First Session (9 AM - 2 PM, 5 hours):**
```bash
# Start session
POST /api/tables/sessions/start-subscription
{
  "tableId": "table-1-guid",
  "subscriptionId": "sub-guid",
  "userId": "sarah-guid"
}

# ... Sarah studies for 5 hours ...

# End session
POST /api/tables/sessions/end-subscription
"session-guid"

# Result: 5 hours deducted, 163 hours remaining
```

**Day 2 - Second Session (10 AM - 4 PM, 6 hours):**
```bash
# Start new session on different table
POST /api/tables/sessions/start-subscription
{
  "tableId": "table-3-guid",  // Different table!
  "subscriptionId": "sub-guid",
  "userId": "sarah-guid"
}

# ... Sarah studies for 6 hours ...

# End session
POST /api/tables/sessions/end-subscription
"session-guid"

# Result: 6 hours deducted, 157 hours remaining
```

**Check Remaining Hours:**
```bash
GET /api/subscriptions/sub-guid
# Response: 157 hours remaining
```

## 🎯 Common Operations

### Check Active Subscriptions
```bash
GET /api/subscriptions/admin/status/Active
```

### View All Packages
```bash
GET /api/subscriptions/packages
```

### Update Package Price
```bash
PUT /api/subscriptions/packages/PACKAGE_GUID
{
  "price": 4500
}
```

### Cancel Subscription
```bash
POST /api/subscriptions/SUBSCRIPTION_GUID/cancel
```

### Get Usage Stats
```bash
GET /api/subscriptions/SUBSCRIPTION_GUID/usage
```

## 📊 Database Queries

### Check User's Remaining Hours
```sql
SELECT 
  u.name,
  u.email,
  sp.name as package_name,
  us.total_hours,
  us.hours_used,
  us.remaining_hours,
  us.status
FROM user_subscriptions us
JOIN users u ON us.user_id = u.id
JOIN subscription_packages sp ON us.package_id = sp.id
WHERE u.id = 'USER_GUID'
  AND us.status = 'Active';
```

### View All Active Subscriptions
```sql
SELECT 
  u.name,
  sp.name as package,
  us.remaining_hours,
  us.activation_date
FROM user_subscriptions us
JOIN users u ON us.user_id = u.id
JOIN subscription_packages sp ON us.package_id = sp.id
WHERE us.status = 'Active'
ORDER BY us.remaining_hours ASC;
```

### Track Daily Usage
```sql
SELECT 
  u.name,
  DATE(ts.start_time) as session_date,
  SUM(ts.hours_consumed) as total_hours_used
FROM table_sessions ts
JOIN users u ON ts.user_id = u.id
WHERE ts.is_subscription_based = true
  AND ts.start_time >= NOW() - INTERVAL '7 days'
GROUP BY u.name, DATE(ts.start_time)
ORDER BY session_date DESC;
```

## ✅ Verification Checklist

After implementation, verify:

- [ ] Packages are created and visible
- [ ] Can purchase subscription (admin)
- [ ] Subscription shows in user's list
- [ ] Can start subscription session
- [ ] Can end session
- [ ] Hours are correctly deducted
- [ ] Remaining hours update properly
- [ ] Status changes to "Expired" when depleted
- [ ] Can have multiple active subscriptions
- [ ] Can use different tables per session
- [ ] Receipt prints correctly
- [ ] Tables are freed after session ends

## 🔧 Troubleshooting

### "Package not found"
- Ensure you've seeded packages or created them via API
- Check `is_active` is `true`
- Verify GUID is correct

### "Subscription not found"
- Check subscription belongs to the user
- Verify subscription status is "Active"
- Ensure GUID is correct

### "No remaining hours"
- Check `remaining_hours` field
- User might need to purchase new package
- Consider offering renewal

### Hours not deducting
- Verify session was properly ended
- Check `hours_consumed` field in session
- Review calculation: duration = end_time - start_time

## 📚 Resources

- **Full Guide**: `SUBSCRIPTION_SYSTEM_GUIDE.md`
- **Quick Reference**: `SUBSCRIPTION_QUICK_REFERENCE.md`
- **Implementation Summary**: `SUBSCRIPTION_IMPLEMENTATION_SUMMARY.md`
- **Swagger Docs**: `http://localhost:5212/swagger`

## 🎉 You're Ready!

The subscription system is fully functional. Start by:
1. Creating a few packages
2. Testing a purchase
3. Starting and ending a session
4. Verifying hour tracking

Good luck! 🚀

