# ✅ RateId Foreign Key Added to table_sessions

## Summary

Successfully added a foreign key relationship from `table_sessions` to the `rates` table. Each session now references a specific rate instead of storing a duplicate rate value.

---

## Changes Made

### 1. Database Model Updated ✅

**File:** `Study-Hub/Models/Entities/TableSession.cs`

**Changed from:**
```csharp
[Column("rate", TypeName = "decimal(10,2)")]
public decimal? Rate { get; set; }
```

**Changed to:**
```csharp
[Column("rate_id")]
public Guid? RateId { get; set; }

// Navigation property
[ForeignKey("RateId")]
public virtual Rate? Rate { get; set; }
```

---

### 2. DTOs Updated ✅

**File:** `Study-Hub/Models/DTOs/AdminDto.cs`

Updated `TransactionWithUserDto`:
```csharp
public Guid? RateId { get; set; }  // Instead of decimal? Rate
```

**File:** `Study-Hub/Models/DTOs/TableDto.cs`

Updated `StartSessionRequestDto`:
```csharp
public Guid? RateId { get; set; }  // Instead of decimal? Rate
```

---

### 3. Service Layer Updated ✅

**File:** `Study-Hub/Service/AdminService.cs`

Updated `MapToTransactionWithUserDto`:
```csharp
RateId = transaction.RateId,  // Map the foreign key
```

**File:** `Study-Hub/Service/TableService.cs`

Updated in two locations:

1. **StartTableSessionAsync** - Creating new session:
```csharp
var session = new TableSession
{
    // ...existing properties...
    RateId = request.RateId,  // Store rate reference
    // ...
};
```

2. **ChangeTableAsync** - Transferring session:
```csharp
var newSession = new TableSession
{
    // ...existing properties...
    RateId = currentSession.RateId,  // Transfer rate reference
    // ...
};
```

---

### 4. Migration Updated ✅

**File:** `Study-Hub/Migrations/20251103000000_AddRateToTableSession.cs`

Complete migration with foreign key constraint:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Add rate_id column
    migrationBuilder.AddColumn<Guid>(
        name: "rate_id",
        table: "table_sessions",
        type: "uuid",
        nullable: true);

    // Create index for performance
    migrationBuilder.CreateIndex(
        name: "IX_table_sessions_rate_id",
        table: "table_sessions",
        column: "rate_id");

    // Add foreign key constraint
    migrationBuilder.AddForeignKey(
        name: "FK_table_sessions_rates_rate_id",
        table: "table_sessions",
        column: "rate_id",
        principalTable: "rates",
        principalColumn: "id",
        onDelete: ReferentialAction.SetNull);
}
```

---

## Database Schema

### Table: table_sessions

**New Column:**
```sql
ALTER TABLE table_sessions 
ADD COLUMN rate_id UUID NULL;

CREATE INDEX IX_table_sessions_rate_id ON table_sessions(rate_id);

ALTER TABLE table_sessions
ADD CONSTRAINT FK_table_sessions_rates_rate_id
FOREIGN KEY (rate_id) 
REFERENCES rates(id)
ON DELETE SET NULL;
```

**Column Details:**
- **Name:** `rate_id`
- **Type:** `UUID`
- **Nullable:** Yes
- **Foreign Key:** References `rates(id)`
- **On Delete:** SET NULL (preserves session if rate is deleted)
- **Index:** Created for performance

---

## Benefits

### ✅ Data Integrity
- Enforced referential integrity
- Can't reference non-existent rates
- Database validates rate existence

### ✅ Normalization
- No duplicate rate data (hours, price)
- Single source of truth in `rates` table
- Easier to update rate definitions

### ✅ Historical Tracking
- Each session references exact rate used
- Rate details preserved in `rates` table
- Can track rate changes over time

### ✅ Reporting & Analytics
- Join to `rates` table for full rate details
- Query sessions by rate package
- Calculate revenue by rate type

### ✅ Rate Management
- Update rate descriptions without affecting sessions
- Deactivate rates without losing session history
- View which sessions used each rate

---

## Usage Examples

### Creating a Session

**Frontend Request:**
```json
POST /api/tables/sessions/start
{
  "tableId": "abc-123",
  "hours": 2,
  "amount": 100,
  "rateId": "rate-uuid-here"  ← References rates table
}
```

**Backend Processing:**
```csharp
var session = new TableSession
{
    UserId = userId,
    TableId = tableId,
    RateId = request.RateId,  // Foreign key to rates
    Amount = 100,
    // ...
};
```

---

### Retrieving Session with Rate Details

**Query with Navigation Property:**
```csharp
var session = await _context.TableSessions
    .Include(s => s.Rate)  // Load related rate
    .Include(s => s.Table)
    .Include(s => s.User)
    .FirstOrDefaultAsync(s => s.Id == sessionId);

// Access rate details:
var hours = session.Rate?.Hours;
var price = session.Rate?.Price;
var description = session.Rate?.Description;
```

**API Response:**
```json
{
  "id": "session-123",
  "rateId": "rate-456",
  "amount": 100,
  "rate": {
    "id": "rate-456",
    "hours": 2,
    "price": 50.00,
    "description": "2-Hour Study Package"
  }
}
```

---

### Reporting Queries

**Sessions by Rate:**
```sql
SELECT 
    r.hours,
    r.price,
    r.description,
    COUNT(ts.id) as session_count,
    SUM(ts.amount) as total_revenue
FROM table_sessions ts
JOIN rates r ON ts.rate_id = r.id
WHERE ts.created_at >= '2025-01-01'
GROUP BY r.id, r.hours, r.price, r.description
ORDER BY total_revenue DESC;
```

**Find all sessions using a specific rate:**
```sql
SELECT * FROM table_sessions 
WHERE rate_id = 'specific-rate-uuid';
```

---

## Data Migration Strategy

### For Existing Data

If you have existing `table_sessions` without `rate_id`:

```sql
-- Option 1: Set to NULL (if rate unknown)
-- Already handled by nullable column

-- Option 2: Match by hours and create rate reference
UPDATE table_sessions ts
SET rate_id = (
    SELECT id FROM rates r
    WHERE r.hours = EXTRACT(HOUR FROM (ts.end_time - ts.start_time))
    AND r.is_active = true
    LIMIT 1
)
WHERE ts.rate_id IS NULL
AND ts.end_time IS NOT NULL;

-- Option 3: Create default rate for existing sessions
INSERT INTO rates (id, hours, price, description, is_active)
VALUES (uuid_generate_v4(), 1, 50.00, 'Legacy Rate', true)
RETURNING id;

UPDATE table_sessions 
SET rate_id = 'returned-uuid-from-above'
WHERE rate_id IS NULL;
```

---

## Frontend Integration

### Frontend Schema Update

Update the frontend schema to use `rateId`:

```typescript
// study_hub_app/src/schema/table.schema.ts
export const TableSessionSchema = z.object({
  id: z.string(),
  userId: z.string(),
  tableId: z.string().optional().nullable(),
  startTime: z.string().optional().nullable(),
  endTime: z.string().optional().nullable(),
  amount: z.number(),
  status: z.string(),
  rateId: z.string().optional().nullable(),  // Add this
  // ...other fields
});
```

### Fetching Rate Details

When displaying session details, fetch the rate information:

```typescript
const session = await tableService.getActiveSession();

// If rate details needed:
if (session.rateId) {
  const rate = await rateService.getRate(session.rateId);
  // Use rate.hours, rate.price, rate.description
}
```

---

## API Changes

### Affected Endpoints

**POST /api/tables/sessions/start**
- Now accepts `rateId` instead of `rate` value
- Frontend must send selected rate's UUID

**GET /api/admin/transactions/***
- Returns `rateId` in response
- Can optionally include rate details via expand/include

### Example API Updates

**Request:**
```json
{
  "tableId": "...",
  "hours": 2,
  "amount": 100,
  "rateId": "selected-rate-uuid"  ← Changed from rate: 50.00
}
```

**Response:**
```json
{
  "id": "session-123",
  "rateId": "rate-456",  ← Returns UUID instead of value
  "amount": 100,
  "startTime": "...",
  "endTime": "..."
}
```

---

## Advantages Over Previous Approach

### Before (Storing Rate Value):
```
table_sessions
├── rate: 50.00  ← Duplicate data
└── amount: 100.00
```

**Problems:**
- ❌ Duplicate rate data in every session
- ❌ No link to rate definition
- ❌ Can't update rate descriptions
- ❌ Hard to query by rate package
- ❌ No referential integrity

### After (Foreign Key):
```
table_sessions           rates
├── rate_id ──────────→ ├── id
└── amount: 100.00       ├── hours: 2
                         ├── price: 50.00
                         └── description: "..."
```

**Benefits:**
- ✅ No duplicate data
- ✅ Links to rate definition
- ✅ Can update rate metadata
- ✅ Easy to query by rate
- ✅ Database enforced integrity

---

## Testing

### 1. Create Session with Rate

```bash
POST /api/tables/sessions/start
{
  "tableId": "...",
  "userId": "...",
  "rateId": "rate-uuid-from-rates-table",
  "hours": 2,
  "amount": 100
}
```

**Verify:**
- Session created with `rate_id` populated
- Foreign key constraint works

### 2. Query Session

```sql
SELECT ts.*, r.hours, r.price, r.description
FROM table_sessions ts
LEFT JOIN rates r ON ts.rate_id = r.id
WHERE ts.id = 'session-uuid';
```

**Verify:**
- Rate details joined correctly
- NULL rate_id handled gracefully

### 3. Delete Rate (Soft)

```sql
-- Should fail if ON DELETE RESTRICT
-- Should set NULL if ON DELETE SET NULL
DELETE FROM rates WHERE id = 'rate-uuid';
```

**Verify:**
- Sessions preserved
- rate_id set to NULL (or delete prevented)

---

## Migration Steps

### Apply Migration

**Option 1: Using EF CLI**
```bash
cd Study-Hub
dotnet ef database update
```

**Option 2: Manual SQL**
```sql
-- Add column
ALTER TABLE table_sessions 
ADD COLUMN rate_id UUID NULL;

-- Create index
CREATE INDEX IX_table_sessions_rate_id 
ON table_sessions(rate_id);

-- Add foreign key
ALTER TABLE table_sessions
ADD CONSTRAINT FK_table_sessions_rates_rate_id
FOREIGN KEY (rate_id) 
REFERENCES rates(id)
ON DELETE SET NULL;
```

### Verify Migration

```sql
-- Check column exists
\d table_sessions

-- Check foreign key
SELECT conname, conrelid::regclass, confrelid::regclass
FROM pg_constraint
WHERE conname = 'FK_table_sessions_rates_rate_id';

-- Check index
\di IX_table_sessions_rate_id
```

---

## Files Modified

1. ✅ `Models/Entities/TableSession.cs` - Changed to RateId + navigation property
2. ✅ `Models/DTOs/AdminDto.cs` - Changed to Guid? RateId
3. ✅ `Models/DTOs/TableDto.cs` - Changed to Guid? RateId  
4. ✅ `Service/AdminService.cs` - Updated mapping to RateId
5. ✅ `Service/TableService.cs` - Updated to use RateId (2 places)
6. ✅ `Migrations/20251103000000_AddRateToTableSession.cs` - Complete FK migration

---

## Status

✅ **Code Complete** - All files updated with foreign key  
✅ **Migration Ready** - Includes FK constraint and index  
✅ **No Errors** - Compiles successfully (only warnings)  
✅ **Backward Compatible** - Nullable column  
✅ **Database Integrity** - Foreign key enforced  

---

## Next Steps

1. **Apply Migration** - Run `dotnet ef database update`
2. **Update Frontend** - Change to send `rateId` instead of `rate`
3. **Test** - Create sessions with rate references
4. **Verify** - Check foreign key constraints work
5. **Monitor** - Ensure queries perform well with new joins

---

## Documentation Reference

- **Rates Table:** See `Rate.cs` model
- **Migration Guide:** See Entity Framework documentation
- **Frontend Schema:** Update TypeScript types accordingly

---

**Date:** November 3, 2025  
**Change:** Added `rate_id` foreign key to `table_sessions` table  
**Type:** Database schema enhancement  
**Status:** ✅ Ready for deployment

