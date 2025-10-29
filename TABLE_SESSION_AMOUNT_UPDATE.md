# Table Session Amount Update - Implementation Complete

## Summary

Successfully updated the table session system to change `credit_used` to `amount`, with the amount now being passed from the frontend instead of calculated in the backend.

## Changes Made

### Backend Changes

#### 1. Entity Model
**File**: `Study-Hub/Models/Entities/TableSession.cs`
- ✅ Changed `CreditsUsed` property to `Amount`
- ✅ Updated column attribute from `credits_used` to `amount`

#### 2. DTOs
**File**: `Study-Hub/Models/DTOs/TableDto.cs`
- ✅ Added `amount` field to `StartSessionRequestDto` (required, decimal)
- ✅ Changed `CreditsUsed` to `Amount` in `SessionWithTableDto`
- ✅ Changed `CreditsUsed` to `Amount` in `EndSessionResponseDto`

#### 3. Service Layer
**File**: `Study-Hub/Service/TableService.cs`
- ✅ Updated `StartTableSessionAsync` to use `request.amount` instead of calculating from hourly rate
- ✅ Updated `EndTableSessionAsync` to use `Amount` instead of `CreditsUsed`
- ✅ Updated `MapToSessionWithTableDto` to map `Amount` property

**File**: `Study-Hub/Service/UserService.cs`
- ✅ Updated `MapToSessionWithTableDto` to map `Amount` property

#### 4. Database Migration
- ✅ Created migration: `RenameCreditsUsedToAmountInTableSession`
- ✅ Applied migration successfully
- ✅ Column renamed from `credits_used` to `amount` in `table_sessions` table

### Frontend Changes

#### 1. Schema Updates
**File**: `study_hub_app/src/schema/table.schema.ts`
- ✅ Changed `creditsUsed` to `amount` in `TableSessionSchema`
- ✅ Added required `amount` field to `StartSessionRequestSchema`
- ✅ Changed `creditsUsed` to `amount` in `EndSessionResponseSchema`

#### 2. Service Updates
No changes needed - the service uses the schemas which were updated.

#### 3. Component Updates

**File**: `study_hub_app/src/pages/dashboard/TableScanner.tsx`
- ✅ Calculate amount: `scannedTable.hourlyRate * selectedHours`
- ✅ Pass `amount` when starting session

**File**: `study_hub_app/src/pages/tables/TableDetails.tsx`
- ✅ Calculate amount with promo discount: `finalCredits`
- ✅ Pass `amount`, `hours`, and `endTime` when starting session
- ✅ Changed `activeSession.creditsUsed` to `activeSession.amount` in display

**File**: `study_hub_app/src/pages/TransactionManagement.tsx`
- ✅ Calculate amount: `(hourlyRate * hours) - promoDiscount`
- ✅ Pass `amount` when starting admin session
- ✅ Updated mutation type to include `amount` field

**File**: `study_hub_app/src/pages/dashboard/Dashboard.tsx`
- ✅ Changed all `activeSession.creditsUsed` to `activeSession.amount` (3 occurrences)

**File**: `study_hub_app/src/pages/history/History.tsx`
- ✅ Changed `session.creditsUsed` to `session.amount` in display

**File**: `study_hub_app/src/pages/profile/Profile.tsx`
- ✅ Changed `s.creditsUsed` to `s.amount` in stats calculation

## Flow Changes

### Before (Backend Calculated)
```
Frontend → Send: { tableId, hours }
Backend → Calculate: amount = hourlyRate * hours
Backend → Store: credits_used = calculated amount
```

### After (Frontend Passes Amount)
```
Frontend → Calculate: amount = (hourlyRate * hours) - promoDiscount
Frontend → Send: { tableId, hours, amount }
Backend → Use: amount from request
Backend → Store: amount = request.amount
```

## Benefits

1. **Flexibility**: Frontend can apply promo codes and discounts before sending
2. **Transparency**: Admin and users see exact amount before confirming
3. **Consistency**: Amount calculated once in one place
4. **Promo Support**: Easy to apply promotional discounts
5. **Admin Control**: Admin can set custom amounts when creating sessions

## Testing Checklist

### Backend
- [x] Build successful
- [x] Migration applied
- [x] No compilation errors
- [ ] Test startSession API with amount parameter
- [ ] Test endSession API returns amount
- [ ] Verify database stores amount correctly

### Frontend
- [x] Build successful
- [x] No compilation errors
- [ ] Test TableScanner session start with calculated amount
- [ ] Test TableDetails session start with promo discount
- [ ] Test TransactionManagement admin session start
- [ ] Verify Dashboard displays amount correctly
- [ ] Verify History shows amount correctly
- [ ] Verify Profile stats calculate with amount

## API Changes

### Start Session Endpoint
**Before**:
```json
POST /tables/sessions/start
{
  "tableId": "guid",
  "qrCode": "string",
  "hours": 2
}
```

**After**:
```json
POST /tables/sessions/start
{
  "tableId": "guid",
  "qrCode": "string",
  "hours": 2,
  "amount": 50.00
}
```

### Session Response
**Before**:
```json
{
  "id": "guid",
  "creditsUsed": 50.00,
  ...
}
```

**After**:
```json
{
  "id": "guid",
  "amount": 50.00,
  ...
}
```

## Database Schema

### table_sessions Table
**Column Change**:
- `credits_used` → `amount` (decimal(10,2))

All data preserved during migration via column rename.

## Code Examples

### Frontend - Starting a Session
```typescript
// Calculate amount with promo discount
const amount = (table.hourlyRate * hours) - promoDiscount;

// Start session with calculated amount
await startSession.mutateAsync({
  tableId: table.id,
  qrCode: table.qrCode,
  hours: hours,
  endTime: endTime.toISOString(),
  amount: amount,
  promoId: selectedPromoId
});
```

### Backend - Using Amount
```csharp
// Use amount from request instead of calculating
userCredits.Balance -= request.amount;
userCredits.TotalSpent += request.amount;

var session = new TableSession
{
    UserId = targetUserId,
    TableId = request.TableId,
    StartTime = DateTime.UtcNow,
    EndTime = request.endTime,
    Amount = request.amount, // Use passed amount
    Status = "active"
};
```

## Migration Details

**Migration Name**: `20251029130501_RenameCreditsUsedToAmountInTableSession`

**SQL Generated**:
```sql
ALTER TABLE table_sessions RENAME COLUMN credits_used TO amount;
```

**Rollback** (if needed):
```bash
cd Study-Hub
dotnet ef database update <PreviousMigrationName>
dotnet ef migrations remove
```

## Breaking Changes

⚠️ **Frontend Required**: The `amount` field is now required when starting a session. All frontend clients must be updated to pass the amount.

⚠️ **API Version**: Consider this a minor version bump if you have external API consumers.

## Deployment Steps

1. **Backend**:
   ```bash
   cd Study-Hub
   dotnet ef database update
   dotnet publish -c Release
   # Deploy to server
   ```

2. **Frontend**:
   ```bash
   cd study_hub_app
   npm run build
   # Deploy dist folder
   ```

3. **Verify**:
   - Test session creation
   - Check database for amount column
   - Verify existing sessions still work
   - Test with promo codes

## Status

- **Implementation**: ✅ Complete
- **Backend Build**: ✅ Passing
- **Frontend Build**: ✅ Passing
- **Database Migration**: ✅ Applied
- **Ready for Testing**: ✅ Yes
- **Ready for Production**: ⏳ After testing

---

**Date**: October 29, 2025  
**Changes**: Backend + Frontend + Database  
**Impact**: All session-related functionality  
**Breaking**: Yes - requires updated frontend

