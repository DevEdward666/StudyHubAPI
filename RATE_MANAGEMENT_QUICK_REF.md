# Rate Management Quick Reference

## Create Different Rate Types

### 1 Hour Rate
```bash
curl -X POST http://localhost:5212/api/admin/rates \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "hours": 1,
    "durationType": "Hourly",
    "durationValue": 1,
    "price": 50.00,
    "description": "1 Hour Rate",
    "isActive": true,
    "displayOrder": 1
  }'
```

### 1 Day Rate (24 hours)
```bash
curl -X POST http://localhost:5212/api/admin/rates \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "hours": 24,
    "durationType": "Daily",
    "durationValue": 1,
    "price": 1000.00,
    "description": "1 Day Pass",
    "isActive": true,
    "displayOrder": 2
  }'
```

### 1 Week Rate (168 hours)
```bash
curl -X POST http://localhost:5212/api/admin/rates \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "hours": 168,
    "durationType": "Weekly",
    "durationValue": 1,
    "price": 5000.00,
    "description": "1 Week Pass",
    "isActive": true,
    "displayOrder": 3
  }'
```

### 1 Month Rate (720 hours = 30 days)
```bash
curl -X POST http://localhost:5212/api/admin/rates \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "hours": 720,
    "durationType": "Monthly",
    "durationValue": 1,
    "price": 15000.00,
    "description": "1 Month Premium",
    "isActive": true,
    "displayOrder": 4
  }'
```

## Hours Calculation Reference

| Duration | Formula | Example |
|----------|---------|---------|
| Hourly | value × 1 | 3 hours = 3 |
| Daily | value × 24 | 2 days = 48 |
| Weekly | value × 168 | 1 week = 168 |
| Monthly | value × 720 | 1 month = 720 |

## Common Packages

- **1 Hour** = 1 hour (₱50)
- **3 Hours** = 3 hours (₱120)
- **1 Day** = 24 hours (₱1,000)
- **3 Days** = 72 hours (₱2,500)
- **1 Week** = 168 hours (₱5,000)
- **2 Weeks** = 336 hours (₱9,000)
- **1 Month** = 720 hours (₱15,000)
- **3 Months** = 2,160 hours (₱40,000)

---

**Quick Start:** See `RATE_DURATION_TYPES_IMPLEMENTATION.md` for full documentation

