# Promo System Implementation Guide

## Overview
The Promo System allows administrators to create and manage promotional codes that give users bonus credits on purchases. The system supports three types of promos:

1. **Percentage** - Give a percentage bonus (e.g., 20% extra credits)
2. **FixedAmount** - Give a fixed bonus amount (e.g., +50 credits)
3. **BuyXGetY** - Buy X amount, get Y amount free (e.g., Buy 100, Get 20)

## Database Schema

### Promos Table
- **id** (UUID) - Primary key
- **code** (string, unique) - Promo code (e.g., "WELCOME20")
- **name** (string) - Display name
- **description** (string) - Description
- **type** (enum) - Percentage | FixedAmount | BuyXGetY
- **status** (enum) - Active | Inactive | Expired | Scheduled
- **percentage_bonus** (decimal) - For Percentage type
- **fixed_bonus_amount** (decimal) - For FixedAmount type
- **buy_amount** (decimal) - For BuyXGetY type
- **get_amount** (decimal) - For BuyXGetY type
- **min_purchase_amount** (decimal) - Minimum purchase required
- **max_discount_amount** (decimal) - Maximum bonus cap
- **usage_limit** (int) - Total usage limit
- **usage_per_user** (int) - Per-user usage limit
- **current_usage_count** (int) - Current usage count
- **start_date** (datetime) - When promo becomes active
- **end_date** (datetime) - When promo expires
- **created_by** (UUID) - Admin who created it
- **is_deleted** (bool) - Soft delete flag

### PromoUsages Table
- **id** (UUID) - Primary key
- **promo_id** (UUID) - Foreign key to Promos
- **user_id** (UUID) - Foreign key to Users
- **transaction_id** (UUID) - Foreign key to CreditTransactions
- **purchase_amount** (decimal) - Original purchase amount
- **bonus_amount** (decimal) - Bonus credits given
- **used_at** (datetime) - When promo was used

## API Endpoints

### Admin Endpoints (Require Admin Role)

#### Create Promo
```http
POST /api/admin/promos/create
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "code": "WELCOME20",
  "name": "Welcome Bonus",
  "description": "Get 20% bonus credits!",
  "type": "Percentage",
  "percentageBonus": 20,
  "minPurchaseAmount": 100,
  "maxDiscountAmount": 50,
  "usagePerUser": 1,
  "startDate": "2025-01-01T00:00:00Z",
  "endDate": "2025-12-31T23:59:59Z"
}
```

#### Update Promo
```http
PUT /api/admin/promos/update
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "promoId": "guid-here",
  "name": "Updated Name",
  "status": "Active",
  "usageLimit": 200
}
```

#### Get All Promos
```http
GET /api/admin/promos?includeInactive=true
Authorization: Bearer {admin_token}
```

#### Get Promo by ID
```http
GET /api/admin/promos/{promoId}
Authorization: Bearer {admin_token}
```

#### Get Promo by Code
```http
GET /api/admin/promos/code/{code}
Authorization: Bearer {admin_token}
```

#### Toggle Promo Status
```http
PATCH /api/admin/promos/toggle-status
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "promoId": "guid-here",
  "status": "Inactive"
}
```

#### Delete Promo (Soft Delete)
```http
DELETE /api/admin/promos/delete/{promoId}
Authorization: Bearer {admin_token}
```

#### Get Promo Usage History
```http
GET /api/admin/promos/{promoId}/usage-history
Authorization: Bearer {admin_token}
```

#### Get Promo Statistics
```http
GET /api/admin/promos/{promoId}/statistics
Authorization: Bearer {admin_token}
```

#### Get All Promo Statistics
```http
GET /api/admin/promos/statistics/all
Authorization: Bearer {admin_token}
```

### User Endpoints

#### Get Available Promos
```http
GET /api/user/promos/available
Authorization: Bearer {user_token}
```

#### Validate Promo Code
```http
POST /api/user/promos/validate
Authorization: Bearer {user_token}
Content-Type: application/json

{
  "promoCode": "WELCOME20",
  "purchaseAmount": 150
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "isValid": true,
    "message": "Promo code is valid",
    "originalAmount": 150,
    "bonusAmount": 30,
    "totalAmount": 180,
    "promoDetails": { ... }
  }
}
```

## Setup Instructions

### 1. Create Database Migration

Run the migration script:
```bash
chmod +x create-promo-migration.sh
./create-promo-migration.sh
```

Or manually:
```bash
cd Study-Hub
dotnet ef migrations add AddPromoSystem
dotnet ef database update
```

### 2. Test the Endpoints

Use the `test-promos.http` file to test all endpoints. Replace the tokens with actual JWT tokens from your authentication system.

## Usage Examples

### Example 1: Welcome Bonus (20% extra credits)
```json
{
  "code": "WELCOME20",
  "name": "Welcome Bonus",
  "type": "Percentage",
  "percentageBonus": 20,
  "minPurchaseAmount": 100,
  "usagePerUser": 1
}
```
- User buys 150 credits → Gets 30 bonus (20% of 150) → Total: 180 credits

### Example 2: Fixed Bonus (+50 credits)
```json
{
  "code": "BONUS50",
  "name": "50 Credits Bonus",
  "type": "FixedAmount",
  "fixedBonusAmount": 50,
  "minPurchaseAmount": 200
}
```
- User buys 200 credits → Gets 50 bonus → Total: 250 credits

### Example 3: Buy 100 Get 20
```json
{
  "code": "BUY100GET20",
  "name": "Buy 100, Get 20",
  "type": "BuyXGetY",
  "buyAmount": 100,
  "getAmount": 20
}
```
- User buys 100 credits → Gets 20 bonus → Total: 120 credits
- User buys 250 credits → Gets 40 bonus (2x multiplier) → Total: 290 credits

## Promo Validation Rules

The system automatically validates:

1. **Status** - Must be Active
2. **Date Range** - Current date must be between startDate and endDate
3. **Minimum Purchase** - Purchase amount must meet minPurchaseAmount
4. **Usage Limits** - Total usage and per-user usage limits
5. **Max Discount** - Bonus amount capped at maxDiscountAmount

## Integration with Purchase Workflow

### Option 1: Manual Application (Recommended)
Admin manually applies promo when approving transaction:
1. User submits purchase request
2. Admin sees transaction in pending list
3. Admin validates promo code
4. Admin approves transaction with promo applied
5. System adds purchase amount + bonus to user credits

### Option 2: Automatic Application
Modify the purchase flow to accept promo code:
1. User enters promo code during purchase
2. System validates promo
3. System stores promo code with transaction
4. When admin approves, bonus is automatically applied

## Promo Statistics

The system tracks:
- **Total Usage Count** - How many times promo was used
- **Unique Users** - Number of different users who used it
- **Total Bonus Given** - Sum of all bonus credits given
- **Total Purchase Amount** - Sum of all purchases with this promo
- **Last Used At** - When it was last used

## Best Practices

### Creating Promos
1. **Use clear, memorable codes** (e.g., WELCOME20, BLACKFRIDAY50)
2. **Set reasonable limits** (usageLimit, usagePerUser)
3. **Set expiration dates** for time-limited offers
4. **Cap bonuses** with maxDiscountAmount to prevent abuse
5. **Test promos** before going live

### Managing Promos
1. **Monitor statistics** regularly
2. **Deactivate expired promos** manually if needed
3. **Use descriptive names** for easy identification
4. **Track performance** to see which promos work best

### Security
1. Promo codes are case-insensitive
2. Soft delete preserves history
3. All operations require admin authentication
4. Usage is tracked per user and overall

## Files Created

1. **Models/Entities/Promo.cs** - Database entities
2. **Models/DTOs/PromoDto.cs** - Data transfer objects
3. **Service/Interface/IPromoService.cs** - Service interface
4. **Service/PromoService.cs** - Service implementation
5. **Controllers/AdminController.cs** - Admin endpoints (updated)
6. **Controllers/UserController.cs** - User endpoints (updated)
7. **Data/ApplicationDBContext.cs** - Database context (updated)
8. **Program.cs** - DI registration (updated)
9. **test-promos.http** - API tests
10. **create-promo-migration.sh** - Migration script
11. **PROMO_IMPLEMENTATION.md** - This guide

## Troubleshooting

### Promo not applying
- Check promo status is Active
- Verify dates are valid
- Check usage limits
- Ensure minimum purchase is met

### Database errors
- Run migration: `dotnet ef database update`
- Check connection string
- Verify PostgreSQL is running

### Authorization errors
- Verify JWT token is valid
- Check user has admin role
- Ensure [Authorize] attribute is present

## Next Steps

1. ✅ Create database migration
2. ✅ Test admin endpoints
3. ✅ Test user endpoints
4. 🔲 Integrate with purchase workflow
5. 🔲 Add promo UI to admin panel
6. 🔲 Add promo validation to user purchase page
7. 🔲 Set up automated promo notifications

## Support

For issues or questions, check:
- Error logs in console
- Database schema
- API response messages
- JWT token validity

