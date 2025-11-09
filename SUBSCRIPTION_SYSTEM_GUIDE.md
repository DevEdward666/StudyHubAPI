# Subscription System Implementation Guide

## Overview
The subscription system allows users to purchase time packages (hourly, daily, weekly, monthly) and use them flexibly across multiple sessions. Users can come and go, and their remaining time is always tracked and preserved.

## Key Features

### 1. **Flexible Time Packages**
- Hourly packages (e.g., 10 hours for ₱500)
- Daily packages (e.g., 1 day = 24 hours for ₱1000)
- Weekly packages (e.g., 1 week = 168 hours for ₱5000)
- Monthly packages (e.g., 1 month = 720 hours for ₱15000)

### 2. **Persistent Time Tracking**
- Users retain remaining hours/days/weeks/months in their account
- Time is deducted only when actually used
- Can stop and resume sessions without losing time
- Multiple sessions can be created until all hours are consumed

### 3. **Table Flexibility**
- Tables are not permanently assigned to subscription users
- Admin can assign any available table when user arrives
- Table becomes available for others when user leaves
- User can use different tables on different days

## Database Schema

### New Tables

#### `subscription_packages`
```sql
CREATE TABLE subscription_packages (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    package_type VARCHAR(50) NOT NULL, -- Hourly, Daily, Weekly, Monthly
    duration_value INT NOT NULL, -- 1, 2, 3, etc.
    total_hours DECIMAL(10,2) NOT NULL, -- Total hours in package
    price DECIMAL(10,2) NOT NULL,
    description VARCHAR(500),
    is_active BOOLEAN DEFAULT true,
    display_order INT,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    created_by UUID
);
```

#### `user_subscriptions`
```sql
CREATE TABLE user_subscriptions (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    package_id UUID NOT NULL REFERENCES subscription_packages(id),
    total_hours DECIMAL(10,2) NOT NULL,
    remaining_hours DECIMAL(10,2) NOT NULL,
    hours_used DECIMAL(10,2) DEFAULT 0,
    purchase_date TIMESTAMP DEFAULT NOW(),
    activation_date TIMESTAMP, -- First session start
    expiry_date TIMESTAMP, -- Optional
    status VARCHAR(50) DEFAULT 'Active',
    purchase_amount DECIMAL(10,2),
    payment_method VARCHAR(50),
    transaction_reference VARCHAR(255),
    notes VARCHAR(1000),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    created_by UUID
);
```

### Updated Tables

#### `table_sessions` (added fields)
```sql
ALTER TABLE table_sessions ADD COLUMN subscription_id UUID REFERENCES user_subscriptions(id);
ALTER TABLE table_sessions ADD COLUMN hours_consumed DECIMAL(10,2);
ALTER TABLE table_sessions ADD COLUMN is_subscription_based BOOLEAN DEFAULT false;
```

## API Endpoints

### Package Management

#### Get All Packages
```http
GET /api/subscriptions/packages?activeOnly=true
```
Response:
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "1 Week Package",
      "packageType": "Weekly",
      "durationValue": 1,
      "totalHours": 168,
      "price": 5000,
      "description": "Access for 1 week",
      "isActive": true,
      "displayOrder": 1
    }
  ]
}
```

#### Create Package (Admin Only)
```http
POST /api/subscriptions/packages
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "name": "1 Month Package",
  "packageType": "Monthly",
  "durationValue": 1,
  "totalHours": 720,
  "price": 15000,
  "description": "Access for 1 month",
  "displayOrder": 4
}
```

#### Update Package (Admin Only)
```http
PUT /api/subscriptions/packages/{packageId}
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "price": 14500,
  "description": "Updated description"
}
```

### User Subscription Management

#### Get My Subscriptions
```http
GET /api/subscriptions/my-subscriptions?activeOnly=true
Authorization: Bearer {user_token}
```
Response:
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "packageName": "1 Week Package",
      "packageType": "Weekly",
      "totalHours": 168,
      "remainingHours": 142.5,
      "hoursUsed": 25.5,
      "percentageUsed": 15.18,
      "status": "Active",
      "purchaseDate": "2025-11-01T10:00:00Z",
      "activationDate": "2025-11-01T14:00:00Z"
    }
  ]
}
```

#### Purchase Subscription (User)
```http
POST /api/subscriptions/purchase
Authorization: Bearer {user_token}
Content-Type: application/json

{
  "packageId": "guid",
  "paymentMethod": "Cash",
  "cash": 5000,
  "change": 0,
  "notes": "Customer purchased 1 week package"
}
```

#### Admin Purchase for User
```http
POST /api/subscriptions/admin/purchase
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "userId": "user-guid",
  "packageId": "package-guid",
  "paymentMethod": "Cash",
  "cash": 5000,
  "change": 0,
  "notes": "Admin added subscription for customer"
}
```

#### Get Subscription Usage
```http
GET /api/subscriptions/{subscriptionId}/usage
Authorization: Bearer {user_token}
```
Response:
```json
{
  "success": true,
  "data": {
    "subscriptionId": "guid",
    "hoursConsumed": 25.5,
    "remainingHours": 142.5,
    "sessionStartTime": "2025-11-08T09:00:00Z",
    "sessionEndTime": "2025-11-08T12:30:00Z"
  }
}
```

### Session Management

#### Start Subscription Session
```http
POST /api/tables/sessions/start-subscription
Authorization: Bearer {user_token}
Content-Type: application/json

{
  "tableId": "table-guid",
  "subscriptionId": "subscription-guid",
  "userId": "user-guid" // Optional, for admin assignment
}
```

#### End Subscription Session
```http
POST /api/tables/sessions/end-subscription
Authorization: Bearer {user_token}
Content-Type: application/json

"session-guid"
```
Response:
```json
{
  "success": true,
  "data": {
    "sessionId": "guid",
    "amount": 0,
    "duration": 12600000, // milliseconds
    "hours": 4,
    "tableNumber": "Table 1",
    "customerName": "John Doe",
    "startTime": "2025-11-08T09:00:00Z",
    "endTime": "2025-11-08T12:30:00Z",
    "paymentMethod": "Subscription"
  }
}
```

## Workflow Examples

### Example 1: Weekly Package User

**Day 1:**
1. Customer purchases "1 Week Package" (168 hours) for ₱5000
2. Subscription created with status: "Active", remaining: 168 hours
3. Customer starts session on Table 1 at 9:00 AM
4. Customer leaves at 2:00 PM (5 hours used)
5. Subscription updated: remaining 163 hours

**Day 2:**
1. Customer returns at 10:00 AM
2. Admin assigns Table 3 (different table)
3. Starts new session using same subscription
4. Leaves at 4:00 PM (6 hours used)
5. Subscription updated: remaining 157 hours

**Continues until all 168 hours are consumed or subscription expires**

### Example 2: Monthly Package for Regular Customer

**Purchase:**
- Customer: "Sarah Chen"
- Package: "1 Month Package" (720 hours = 30 days × 24 hours)
- Price: ₱15,000
- Payment: GCash

**Usage Pattern:**
- Week 1: 35 hours used (studying for exams)
- Week 2: 20 hours used (lighter week)
- Week 3: 40 hours used (project deadline)
- Week 4: 30 hours used (regular study)
- **Remaining: 595 hours** (can continue using for more weeks)

## Admin Functions

### View All User Subscriptions
```http
GET /api/subscriptions/admin/all
Authorization: Bearer {admin_token}
```

### View by Status
```http
GET /api/subscriptions/admin/status/Active
Authorization: Bearer {admin_token}
```

### View User's Subscriptions
```http
GET /api/subscriptions/admin/user/{userId}
Authorization: Bearer {admin_token}
```

## Business Rules

### 1. **Subscription Activation**
- Subscription is created as "Active" upon purchase
- `activation_date` is set when first session starts
- Can optionally set `expiry_date` for time-limited packages

### 2. **Time Tracking**
- Hours are tracked with decimal precision (e.g., 2.5 hours)
- `hours_used` = sum of all completed session durations
- `remaining_hours` = `total_hours` - `hours_used`
- When `remaining_hours` reaches 0, status changes to "Expired"

### 3. **Session Management**
- Sessions linked to subscription via `subscription_id`
- `is_subscription_based` flag distinguishes from pay-per-use sessions
- No additional charges for subscription sessions (`amount` = 0)
- Receipt shows "Subscription: {Package Name}" as payment method

### 4. **Table Assignment**
- Tables are NOT reserved for subscription users
- Admin manually assigns available table when user arrives
- Table is freed when session ends
- Same user can use different tables on different visits

### 5. **Subscription Status**
- **Active**: Has remaining hours and can be used
- **Expired**: No remaining hours
- **Cancelled**: User or admin cancelled
- **Suspended**: Temporarily disabled (admin action)

## Migration Steps

1. **Create Migration**
```bash
cd Study-Hub
dotnet ef migrations add AddSubscriptionSystem
```

2. **Apply Migration**
```bash
dotnet ef database update
```

3. **Seed Sample Packages** (Optional)
```sql
INSERT INTO subscription_packages (id, name, package_type, duration_value, total_hours, price, description, display_order, is_active)
VALUES 
  (gen_random_uuid(), '10 Hours Package', 'Hourly', 10, 10, 500, 'Perfect for occasional study sessions', 1, true),
  (gen_random_uuid(), '1 Day Package', 'Daily', 1, 24, 1000, 'Full day access', 2, true),
  (gen_random_uuid(), '1 Week Package', 'Weekly', 1, 168, 5000, 'One week of unlimited access', 3, true),
  (gen_random_uuid(), '1 Month Package', 'Monthly', 1, 720, 15000, 'One month of unlimited access', 4, true);
```

## Frontend Integration Guide

### 1. **Package Selection Screen**
- Display all active packages
- Show package type, hours, price
- Highlight popular packages
- Add "Purchase" button

### 2. **My Subscriptions Screen**
- List active subscriptions
- Show progress bar for hours used
- Display remaining hours prominently
- Show purchase and activation dates

### 3. **Start Session Flow**
- Check if user has active subscriptions
- Show two options:
  - "Pay Per Use" (regular flow)
  - "Use Subscription" (new flow)
- For subscription: select which subscription to use
- Admin assigns table
- Start session

### 4. **Active Session Display**
- Show "Subscription Session" badge
- Display remaining hours in package
- Show current session duration
- No cost display (it's prepaid)

### 5. **Admin Dashboard**
- Manage packages (create, edit, activate/deactivate)
- View all user subscriptions
- Filter by status
- Quick purchase for walk-in customers

## Testing Checklist

- [ ] Create subscription packages
- [ ] User purchases subscription
- [ ] Admin purchases subscription for user
- [ ] Start subscription session
- [ ] End subscription session and verify hours deducted
- [ ] Start multiple sessions from same subscription
- [ ] Verify time tracking is accurate
- [ ] Test subscription expiry when hours reach 0
- [ ] Test table assignment and release
- [ ] Test change table during subscription session
- [ ] Verify receipts show subscription info
- [ ] Test cancellation
- [ ] Test admin viewing all subscriptions

## Notes

- Subscription time is tracked in hours for flexibility
- Weekly package = 7 days × 24 hours = 168 hours
- Monthly package = 30 days × 24 hours = 720 hours
- Users can have multiple active subscriptions
- Each session independently tracks time consumed
- Perfect for regular customers who want better rates

