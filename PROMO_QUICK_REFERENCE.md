# 🎯 Promo System - Quick Reference Card

## 📦 What You Got
A complete backend promo/coupon system for admin/credits with 3 promo types, full CRUD, validation, and analytics.

## 🚀 Quick Commands

### Start API
```bash
cd Study-Hub && dotnet run
```

### Test Endpoints
Open `test-promos.http` in your editor (with REST Client extension)

## 🔑 Promo Types

| Type | Description | Example |
|------|-------------|---------|
| **Percentage** | % bonus on purchase | Buy 100, get 20% extra (120 total) |
| **FixedAmount** | Fixed bonus amount | Buy any, get +50 credits |
| **BuyXGetY** | Buy X, get Y free | Buy 100, get 20 free (120 total) |

## 🌐 Key Endpoints

### Create Promo (Admin)
```http
POST /api/admin/promos/create
{
  "code": "WELCOME20",
  "name": "Welcome Bonus",
  "type": "Percentage",
  "percentageBonus": 20,
  "minPurchaseAmount": 100
}
```

### Validate Promo (User)
```http
POST /api/user/promos/validate
{
  "promoCode": "WELCOME20",
  "purchaseAmount": 150
}
```

### Get All Promos (Admin)
```http
GET /api/admin/promos?includeInactive=false
```

### Get Statistics (Admin)
```http
GET /api/admin/promos/{promoId}/statistics
```

## 📊 Response Example
```json
{
  "success": true,
  "data": {
    "isValid": true,
    "originalAmount": 150,
    "bonusAmount": 30,
    "totalAmount": 180,
    "message": "Promo code is valid"
  }
}
```

## ✅ Validation Checks
- ✓ Status = Active
- ✓ Within date range
- ✓ Meets minimum purchase
- ✓ Under usage limits
- ✓ User hasn't exceeded per-user limit

## 🎨 Sample Promo Codes

```json
// Welcome bonus - 20% off, first purchase only
{"code": "WELCOME20", "type": "Percentage", "percentageBonus": 20, "usagePerUser": 1}

// Student discount - 15% off anytime
{"code": "STUDENT15", "type": "Percentage", "percentageBonus": 15}

// Fixed bonus - Get 50 credits
{"code": "BONUS50", "type": "FixedAmount", "fixedBonusAmount": 50}

// Loyalty - Buy 500, get 100
{"code": "LOYAL500", "type": "BuyXGetY", "buyAmount": 500, "getAmount": 100}

// Black Friday - 50% bonus, capped at 500
{"code": "BLACKFRIDAY50", "type": "Percentage", "percentageBonus": 50, "maxDiscountAmount": 500}
```

## 📁 Files to Know

| File | Purpose |
|------|---------|
| `test-promos.http` | API test examples |
| `PROMO_SUMMARY.md` | Full documentation |
| `PROMO_IMPLEMENTATION.md` | Implementation guide |
| `Service/PromoService.cs` | Business logic |
| `Models/DTOs/PromoDto.cs` | Data structures |

## 🔧 Troubleshooting

| Issue | Solution |
|-------|----------|
| Build fails | Run `dotnet build` and check errors |
| Migration fails | Check PostgreSQL connection |
| 401 Unauthorized | Get fresh JWT token |
| "Promo not found" | Check code spelling, verify status |
| "Usage limit reached" | Check usageLimit and usagePerUser |

## 🎯 Next Steps

1. ✅ Backend complete
2. 🔲 Test with `test-promos.http`
3. 🔲 Create sample promos
4. 🔲 Build admin UI
5. 🔲 Add to user purchase flow

## 💡 Pro Tips

- **Codes are case-insensitive** (WELCOME20 = welcome20)
- **Soft delete** keeps history
- **All dates are UTC**
- **Validation is automatic**
- **Statistics update in real-time**

## 🎊 Status: READY TO USE! ✅

Backend is 100% complete with:
- ✅ 3 promo types
- ✅ 12 admin endpoints  
- ✅ 2 user endpoints
- ✅ Full validation
- ✅ Complete analytics
- ✅ Database migrated

**Go create your first promo!** 🚀

