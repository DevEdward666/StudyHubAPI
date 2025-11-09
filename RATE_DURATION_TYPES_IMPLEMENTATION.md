# ✅ Rate Management - Days, Weeks, and Months Support

## Overview
The rate management system has been updated to support multiple duration types: **Hourly**, **Daily**, **Weekly**, and **Monthly** rates.

## What Changed

### 1. Database Schema Updates

#### New Columns Added to `rates` Table:
- **`duration_type`** (varchar 50) - Type of duration: "Hourly", "Daily", "Weekly", "Monthly"
- **`duration_value`** (integer) - Value for the duration: 1, 2, 3, etc.

**Example:**
- 1 Day = duration_type: "Daily", duration_value: 1, hours: 24
- 2 Weeks = duration_type: "Weekly", duration_value: 2, hours: 336
- 1 Month = duration_type: "Monthly", duration_value: 1, hours: 720

### 2. Updated Models

#### Rate Entity (`Rate.cs`)
```csharp
public class Rate
{
    public Guid Id { get; set; }
    public int Hours { get; set; }
    public string DurationType { get; set; } = "Hourly"; // NEW
    public int DurationValue { get; set; } = 1; // NEW
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    // ... other fields
}
```

#### Rate DTOs
- **`RateDto`** - Now includes `DurationType` and `DurationValue`
- **`CreateRateRequestDto`** - Required fields for creating rates
- **`UpdateRateRequestDto`** - Required fields for updating rates

### 3. Migration Applied
- **Migration:** `AddDurationTypeToRates`
- **Status:** ✅ Applied to database

## API Usage Examples

### Create Hourly Rate
```http
POST /api/admin/rates
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "hours": 1,
  "durationType": "Hourly",
  "durationValue": 1,
  "price": 50.00,
  "description": "1 Hour Rate",
  "isActive": true,
  "displayOrder": 1
}
```

### Create Daily Rate
```http
POST /api/admin/rates
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "hours": 24,
  "durationType": "Daily",
  "durationValue": 1,
  "price": 1000.00,
  "description": "1 Day Pass - Full day access",
  "isActive": true,
  "displayOrder": 2
}
```

### Create Weekly Rate
```http
POST /api/admin/rates
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "hours": 168,
  "durationType": "Weekly",
  "durationValue": 1,
  "price": 5000.00,
  "description": "1 Week Pass - Perfect for exam week",
  "isActive": true,
  "displayOrder": 3
}
```

### Create Multi-Week Rate
```http
POST /api/admin/rates
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "hours": 336,
  "durationType": "Weekly",
  "durationValue": 2,
  "price": 9000.00,
  "description": "2 Weeks Pass - Best value for extended study",
  "isActive": true,
  "displayOrder": 4
}
```

### Create Monthly Rate
```http
POST /api/admin/rates
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "hours": 720,
  "durationType": "Monthly",
  "durationValue": 1,
  "price": 15000.00,
  "description": "1 Month Premium - Unlimited access for serious students",
  "isActive": true,
  "displayOrder": 5
}
```

### Update Rate
```http
PUT /api/admin/rates
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "id": "rate-guid-here",
  "hours": 720,
  "durationType": "Monthly",
  "durationValue": 1,
  "price": 14500.00,
  "description": "1 Month Premium - NOW WITH DISCOUNT!",
  "isActive": true,
  "displayOrder": 5
}
```

## Duration Type Reference

| Duration Type | Duration Value | Hours Calculation | Example |
|---------------|----------------|-------------------|---------|
| **Hourly** | 1-24 | 1 × value | 1 Hour = 1 hour |
| **Hourly** | 1-24 | N × value | 3 Hours = 3 hours |
| **Daily** | 1-365 | 24 × value | 1 Day = 24 hours |
| **Daily** | 1-365 | 24 × value | 3 Days = 72 hours |
| **Weekly** | 1-52 | 168 × value | 1 Week = 168 hours |
| **Weekly** | 1-52 | 168 × value | 2 Weeks = 336 hours |
| **Monthly** | 1-12 | 720 × value | 1 Month = 720 hours (30 days) |
| **Monthly** | 1-12 | 720 × value | 3 Months = 2160 hours |

## Common Rate Packages

### Budget Options
```json
{
  "hours": 1,
  "durationType": "Hourly",
  "durationValue": 1,
  "price": 50.00,
  "description": "1 Hour - Drop-in rate"
}
```

```json
{
  "hours": 3,
  "durationType": "Hourly",
  "durationValue": 3,
  "price": 120.00,
  "description": "3 Hours - Short study session"
}
```

### Popular Options
```json
{
  "hours": 24,
  "durationType": "Daily",
  "durationValue": 1,
  "price": 1000.00,
  "description": "1 Day Pass - Perfect for cramming"
}
```

```json
{
  "hours": 168,
  "durationType": "Weekly",
  "durationValue": 1,
  "price": 5000.00,
  "description": "1 Week Pass - Exam preparation special"
}
```

### Premium Options
```json
{
  "hours": 720,
  "durationType": "Monthly",
  "durationValue": 1,
  "price": 15000.00,
  "description": "1 Month Premium - Best value for regulars"
}
```

```json
{
  "hours": 2160,
  "durationType": "Monthly",
  "durationValue": 3,
  "price": 40000.00,
  "description": "3 Months Elite - Long-term commitment discount"
}
```

## Validation Rules

### Duration Type
- Must be one of: "Hourly", "Daily", "Weekly", "Monthly"
- Case-sensitive

### Duration Value
- **Minimum:** 1
- **Maximum:** 365
- Integer only

### Hours
- **Minimum:** 1
- **Maximum:** 8760 (1 year)
- Must match the calculated hours for the duration type

### Price
- **Minimum:** 0.01
- **Maximum:** 100,000.00
- Decimal with 2 decimal places

## Business Rules

### 1. No Duplicate Rates
- Cannot create two rates with the same hours, duration type, and duration value
- System will return error: "A rate for X {DurationType} (Y hours) already exists"

### 2. Hours Calculation
The hours field should match the duration:
- **Hourly:** hours = duration_value
- **Daily:** hours = duration_value × 24
- **Weekly:** hours = duration_value × 168
- **Monthly:** hours = duration_value × 720 (assuming 30 days)

### 3. Display Order
- Lower numbers appear first
- Recommended order:
  1. Hourly rates (1, 2, 3...)
  2. Daily rates (10, 11, 12...)
  3. Weekly rates (20, 21, 22...)
  4. Monthly rates (30, 31, 32...)

## Response Format

### Get All Rates
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "hours": 1,
      "durationType": "Hourly",
      "durationValue": 1,
      "price": 50.00,
      "description": "1 Hour Rate",
      "isActive": true,
      "displayOrder": 1,
      "createdAt": "2025-11-08T10:00:00Z",
      "updatedAt": "2025-11-08T10:00:00Z"
    },
    {
      "id": "guid",
      "hours": 168,
      "durationType": "Weekly",
      "durationValue": 1,
      "price": 5000.00,
      "description": "1 Week Pass",
      "isActive": true,
      "displayOrder": 3,
      "createdAt": "2025-11-08T10:00:00Z",
      "updatedAt": "2025-11-08T10:00:00Z"
    }
  ],
  "message": "Success",
  "errors": null
}
```

## Frontend Integration Tips

### Display Rate Name
```typescript
function formatRateName(rate: RateDto): string {
  if (rate.durationType === "Hourly") {
    return `${rate.durationValue} Hour${rate.durationValue > 1 ? 's' : ''}`;
  }
  if (rate.durationType === "Daily") {
    return `${rate.durationValue} Day${rate.durationValue > 1 ? 's' : ''}`;
  }
  if (rate.durationType === "Weekly") {
    return `${rate.durationValue} Week${rate.durationValue > 1 ? 's' : ''}`;
  }
  if (rate.durationType === "Monthly") {
    return `${rate.durationValue} Month${rate.durationValue > 1 ? 's' : ''}`;
  }
  return `${rate.hours} Hours`;
}
```

### Calculate Price Per Hour
```typescript
function calculatePricePerHour(rate: RateDto): number {
  return rate.price / rate.hours;
}
```

### Display Savings
```typescript
function calculateSavings(rate: RateDto, hourlyRate: number): string {
  const pricePerHour = rate.price / rate.hours;
  const savings = ((hourlyRate - pricePerHour) / hourlyRate) * 100;
  return savings > 0 ? `Save ${savings.toFixed(0)}%` : '';
}
```

## Migration Details

### Migration Name
`AddDurationTypeToRates`

### SQL Changes
```sql
-- Add new columns
ALTER TABLE rates 
ADD COLUMN duration_type VARCHAR(50) DEFAULT 'Hourly',
ADD COLUMN duration_value INTEGER DEFAULT 1;

-- Update existing records (if any)
UPDATE rates 
SET duration_type = 'Hourly',
    duration_value = hours
WHERE duration_type IS NULL;
```

## Testing Checklist

- [ ] Create hourly rate (1 hour)
- [ ] Create multi-hour rate (3 hours)
- [ ] Create daily rate (1 day = 24 hours)
- [ ] Create weekly rate (1 week = 168 hours)
- [ ] Create monthly rate (1 month = 720 hours)
- [ ] Create multi-week rate (2 weeks = 336 hours)
- [ ] Update existing rate
- [ ] Try to create duplicate rate (should fail)
- [ ] Get all rates (should include duration type)
- [ ] Get active rates only
- [ ] Delete a rate

## Backward Compatibility

### Existing Rates
- All existing rates will have `durationType` = "Hourly"
- All existing rates will have `durationValue` = value of hours field
- No data loss

### Legacy Support
The system maintains backward compatibility:
- Old API calls still work
- Hours field is still required
- Existing rates continue to function

---

**Status:** ✅ IMPLEMENTED & READY  
**Migration Applied:** ✅ Yes  
**Build Status:** ✅ Success  
**Date:** November 8, 2025

