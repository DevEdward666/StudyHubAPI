# ✅ Rate Column Added to table_sessions

## Summary

Successfully added a `Rate` column to the `table_sessions` table to store the hourly rate used for each session.

---

## Changes Made

### 1. Database Model Updated ✅

**File:** `Study-Hub/Models/Entities/TableSession.cs`

Added property:
```csharp
[Column("rate", TypeName = "decimal(10,2)")]
public decimal? Rate { get; set; }
```

### 2. DTO Updated ✅

**File:** `Study-Hub/Models/DTOs/AdminDto.cs`

Added to `TransactionWithUserDto`:
```csharp
public decimal? Rate { get; set; }
```

**File:** `Study-Hub/Models/DTOs/TableDto.cs`

Added to `StartSessionRequestDto`:
```csharp
public decimal? Rate { get; set; }
```

### 3. Service Layer Updated ✅

**File:** `Study-Hub/Service/AdminService.cs`

Updated `MapToTransactionWithUserDto` method:
```csharp
Rate = transaction.Rate,
```

**File:** `Study-Hub/Service/TableService.cs`

Updated in two places:

1. **StartTableSessionAsync** - When creating new session:
```csharp
var session = new TableSession
{
    // ...existing properties...
    Rate = request.Rate,
    // ...
};
```

2. **ChangeTableAsync** - When transferring to new table:
```csharp
var newSession = new TableSession
{
    // ...existing properties...
    Rate = currentSession.Rate, // Transfer the rate
    // ...
};
```

### 4. Migration Created ✅

**File:** `Study-Hub/Migrations/20251103000000_AddRateToTableSession.cs`

Migration to add the `rate` column:
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<decimal>(
        name: "rate",
        table: "table_sessions",
        type: "numeric(10,2)",
        nullable: true);
}
```

---

## Database Schema

The `table_sessions` table now includes:

```sql
ALTER TABLE table_sessions 
ADD COLUMN rate NUMERIC(10,2) NULL;
```

**Column Details:**
- **Name:** `rate`
- **Type:** `decimal(10,2)` / `NUMERIC(10,2)`
- **Nullable:** Yes
- **Purpose:** Store the hourly rate used for this session

---

## Usage

### When Starting a Session

The rate is now stored with the session:

```csharp
var request = new StartSessionRequestDto
{
    TableId = tableId,
    hours = 2,
    amount = 100,
    Rate = 50.00m, // Store the rate used
    // ...other properties
};
```

### When Retrieving Transactions

The rate is included in the response:

```json
{
  "id": "...",
  "amount": 100,
  "rate": 50.00,
  "hours": 2,
  "startTime": "2025-01-03T10:00:00Z",
  "endTime": "2025-01-03T12:00:00Z"
}
```

### When Changing Tables

The rate is preserved when transferring to a new table:

```csharp
// Original session rate is transferred
newSession.Rate = currentSession.Rate;
```

---

## Benefits

✅ **Historical Data** - Preserves the rate used at the time of session  
✅ **Accurate Reporting** - Can calculate costs correctly even if rates change  
✅ **Audit Trail** - Track what rate was charged for each session  
✅ **Flexible Pricing** - Support different rates per session (promos, discounts)  

---

## Frontend Integration

The frontend already supports this through the browser printing implementation:

```tsx
await printDirect({
  storeName: 'STUDY HUB',
  sessionId: transaction.id,
  // ...
  rate: transaction.rate || transaction.tables?.hourlyRate || 0,
  totalAmount: transaction.cost || 0,
  // ...
});
```

---

## Migration Steps

To apply the migration to your database:

### Option 1: Using Entity Framework CLI (if available)

```bash
cd Study-Hub
dotnet ef database update
```

### Option 2: Manual SQL

Run this SQL on your PostgreSQL database:

```sql
ALTER TABLE table_sessions 
ADD COLUMN IF NOT EXISTS rate NUMERIC(10,2);
```

### Option 3: Through Application

The migration will be applied automatically when the application starts with `EnsureCreated()` or when you run migrations.

---

## Verification

After applying the migration, verify with:

```sql
-- Check column exists
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'table_sessions' 
  AND column_name = 'rate';

-- Should return:
-- column_name | data_type | is_nullable
-- rate        | numeric   | YES
```

---

## Backward Compatibility

✅ **Nullable Column** - Existing sessions without rate won't break  
✅ **Default Behavior** - Rate can be calculated from table's hourly rate if not provided  
✅ **Frontend Compatible** - Browser printing handles missing rate gracefully  

---

## Files Modified

1. ✅ `Models/Entities/TableSession.cs` - Added Rate property
2. ✅ `Models/DTOs/AdminDto.cs` - Added Rate to TransactionWithUserDto
3. ✅ `Models/DTOs/TableDto.cs` - Added Rate to StartSessionRequestDto
4. ✅ `Service/AdminService.cs` - Added Rate to mapping
5. ✅ `Service/TableService.cs` - Added Rate to session creation (2 places)
6. ✅ `Migrations/20251103000000_AddRateToTableSession.cs` - Created migration

---

## Testing

### 1. Start a New Session

```bash
POST /api/tables/sessions/start
{
  "tableId": "...",
  "hours": 2,
  "amount": 100,
  "rate": 50.00
}
```

Verify the rate is stored in the database.

### 2. Retrieve Transaction

```bash
GET /api/admin/transactions/all
```

Verify the rate appears in the response.

### 3. Print Receipt

Use the browser printing or backend printing to verify the rate shows correctly on receipts.

---

## Status

✅ **Complete** - All code changes made  
✅ **Migration Ready** - Migration file created  
✅ **No Errors** - All TypeScript/C# compilation successful  
✅ **Backward Compatible** - Won't break existing sessions  

---

## Next Steps

1. **Apply Migration** - Run `dotnet ef database update` or manual SQL
2. **Test** - Create a new session and verify rate is stored
3. **Deploy** - Push changes to production

---

**Date:** November 3, 2025  
**Change:** Added Rate column to table_sessions  
**Status:** ✅ Ready for deployment

