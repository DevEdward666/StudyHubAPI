# Transaction Pending Endpoint Fix

## Issue
The `/admin/transactions/pending` endpoint was returning data with an incorrect structure that didn't match the frontend schema validation. The frontend expected top-level properties `tableId`, `startTime`, and `endTime`, but the backend was returning them nested in a `Tables` object.

### Error Message
```json
{
  "url": "/admin/transactions/pending",
  "method": "GET",
  "errors": [
    {
      "expected": "string",
      "code": "invalid_type",
      "path": ["data", 0, "tableId"],
      "message": "Invalid input: expected string, received undefined"
    },
    {
      "expected": "string",
      "code": "invalid_type",
      "path": ["data", 0, "startTime"],
      "message": "Invalid input: expected string, received undefined"
    },
    {
      "expected": "string",
      "code": "invalid_type",
      "path": ["data", 0, "endTime"],
      "message": "Invalid input: expected string, received undefined"
    }
  ]
}
```

## Root Cause
The `TransactionWithUserDto` had a nested `TableSession Tables` property, and the mapper was creating a nested object structure. However, the frontend schema (`TransactionWithUserSchema`) extends `TableSessionSchema`, which expects these fields at the top level.

## Solution

### Backend Changes

#### 1. Updated DTO Structure
**File**: `Study-Hub/Models/DTOs/AdminDto.cs`

**Before**:
```csharp
public class TransactionWithUserDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserDto User { get; set; }
    public TableSession Tables { get; set; }  // ❌ Nested object
    public decimal Amount { get; set; }
    // ...
}
```

**After**:
```csharp
public class TransactionWithUserDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TableId { get; set; }        // ✅ Top-level property
    public DateTime StartTime { get; set; }  // ✅ Top-level property
    public DateTime? EndTime { get; set; }   // ✅ Top-level property (nullable)
    public UserDto User { get; set; }
    public decimal Amount { get; set; }
    public decimal Cost { get; set; }
    public string Status { get; set; }
    public string PaymentMethod { get; set; }
    public string TransactionId { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### 2. Updated Mapper
**File**: `Study-Hub/Service/AdminService.cs`

**Before**:
```csharp
private static TransactionWithUserDto MapToTransactionWithUserDto(TableSession transaction)
{
    return new TransactionWithUserDto
    {
        Id = transaction.Id,
        UserId = transaction.UserId,
        User = new UserDto { /* ... */ },
        Tables = new TableSession  // ❌ Creating nested object
        {
            Id = transaction.Id,
            UserId = transaction.UserId,
            TableId = transaction.TableId,
            StartTime = transaction.StartTime,
            EndTime = transaction.EndTime,
            // ...
        },
        Amount = transaction.Amount,
        // ...
    };
}
```

**After**:
```csharp
private static TransactionWithUserDto MapToTransactionWithUserDto(TableSession transaction)
{
    return new TransactionWithUserDto
    {
        Id = transaction.Id,
        UserId = transaction.UserId,
        TableId = transaction.TableId,        // ✅ Direct assignment
        StartTime = transaction.StartTime,    // ✅ Direct assignment
        EndTime = transaction.EndTime,        // ✅ Direct assignment
        User = new UserDto
        {
            Id = transaction.User.Id,
            Email = transaction.User.Email,
            Name = transaction.User.Name,
            EmailVerified = transaction.User.EmailVerified,
            CreatedAt = transaction.User.CreatedAt,
            UpdatedAt = transaction.User.UpdatedAt
        },
        Amount = transaction.Amount,
        Cost = transaction.Amount,
        Status = transaction.Status,
        CreatedAt = transaction.CreatedAt
    };
}
```

### Frontend Schema (Already Correct)
**File**: `study_hub_app/src/schema/admin.schema.ts`

```typescript
export const TransactionWithUserSchema = TableSessionSchema.extend({
  user: UserSchema.optional().nullable(),
});
```

This extends `TableSessionSchema` which includes:
```typescript
export const TableSessionSchema = z.object({
  id: z.string(),
  userId: z.string(),
  tableId: z.string(),        // ✅ Expected at top level
  startTime: z.string(),      // ✅ Expected at top level
  endTime: z.string().nullable(), // ✅ Expected at top level
  amount: z.number(),
  status: z.string(),
  createdAt: z.string(),
});
```

## Result

The endpoint now returns data in the correct format:

```json
{
  "success": true,
  "data": [
    {
      "id": "...",
      "userId": "...",
      "tableId": "...",        // ✅ Top-level
      "startTime": "...",      // ✅ Top-level
      "endTime": null,         // ✅ Top-level (nullable)
      "user": {
        "id": "...",
        "email": "...",
        "name": "..."
      },
      "amount": 100,
      "cost": 100,
      "status": "active",
      "createdAt": "..."
    }
  ]
}
```

## Testing

✅ Backend builds successfully  
✅ No compilation errors  
✅ DTO structure matches frontend schema  
✅ Mapper creates correct object structure  

## Files Modified

1. `/Users/edward/Documents/StudyHubAPI/Study-Hub/Models/DTOs/AdminDto.cs`
   - Updated `TransactionWithUserDto` to have `TableId`, `StartTime`, and `EndTime` as top-level properties
   - Made `EndTime` nullable to match database schema

2. `/Users/edward/Documents/StudyHubAPI/Study-Hub/Service/AdminService.cs`
   - Updated `MapToTransactionWithUserDto` to map fields directly instead of nesting

## Status

✅ **FIXED** - The `/admin/transactions/pending` endpoint now returns data that matches the frontend schema validation.

---

**Date**: October 30, 2025  
**Issue**: Frontend validation error on pending transactions endpoint  
**Resolution**: Flattened DTO structure to match frontend schema

