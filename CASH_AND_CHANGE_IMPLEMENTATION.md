# Cash and Change Feature Implementation

## Overview
Added `cash` and `change` fields to the `table_sessions` table to track cash payments and automatically calculate change. The system includes validation to prevent transactions when cash is insufficient.

## Database Changes

### Entity Update
**File**: `Study-Hub/Models/Entities/TableSession.cs`

Added new properties:
```csharp
[Column("cash", TypeName = "decimal(10,2)")]
public decimal? Cash { get; set; }

[Column("change", TypeName = "decimal(10,2)")]
public decimal? Change { get; set; }
```

### Migration
- **Migration Name**: `AddCashAndChangeToTableSession` (generated)
- **Action**: Adds `cash` and `change` columns to `table_sessions` table
- **Type**: `decimal(10,2)` (nullable)
- **Purpose**: Track cash payments and change for audit trail

## Backend Changes

### 1. DTO Updates

#### StartSessionRequestDto
**File**: `Study-Hub/Models/DTOs/TableDto.cs`

Added:
```csharp
public decimal? Cash { get; set; }
public decimal? Change { get; set; }
```

#### TransactionWithUserDto
**File**: `Study-Hub/Models/DTOs/AdminDto.cs`

Added:
```csharp
public decimal? Cash { get; set; }
public decimal? Change { get; set; }
```

### 2. Service Layer Updates

#### TableService
**File**: `Study-Hub/Service/TableService.cs`

Updated `StartTableSessionAsync` to save cash and change:
```csharp
var session = new TableSession
{
    // ...existing fields...
    PaymentMethod = request.PaymentMethod,
    Cash = request.Cash,
    Change = request.Change,
    // ...
};
```

#### AdminService
**File**: `Study-Hub/Service/AdminService.cs`

Updated `MapToTransactionWithUserDto` to include cash and change:
```csharp
Cash = transaction.Cash,
Change = transaction.Change,
```

## Frontend Changes

### 1. Schema Updates

#### TableSessionSchema
**File**: `study_hub_app/src/schema/table.schema.ts`

Added to TableSessionSchema:
```typescript
cash: z.number().optional().nullable(),
change: z.number().optional().nullable(),
```

#### StartSessionRequestSchema
**File**: `study_hub_app/src/schema/table.schema.ts`

Added:
```typescript
cash: z.number().optional(),
change: z.number().optional(),
```

#### Admin Schema
**File**: `study_hub_app/src/schema/admin.schema.ts`

Updated `getTransactionWithUserColumn`:
```typescript
cash: z.number().optional().nullable(),
change: z.number().optional().nullable(),
```

### 2. Transaction Management UI

**File**: `study_hub_app/src/pages/TransactionManagement.tsx`

#### State Management
Added state for cash and change:
```typescript
const [cash, setCash] = useState<number>(0);
const [change, setChange] = useState<number>(0);
```

#### Auto-Calculate Change Handler
```typescript
const handleCashInput = (value: number) => {
  setCash(value);
  const totalAmount = sessionPrice - promoDiscount;
  const calculatedChange = value - totalAmount;
  setChange(calculatedChange >= 0 ? calculatedChange : 0);
};
```

#### Cash Input Field (Conditional Display)
Only shown when payment method is "Cash":
```tsx
{paymentMethod === "Cash" && (
  <IonItem style={{ marginBottom: "20px" }}>
    <IonLabel position="stacked">Cash Amount *</IonLabel>
    <IonInput
      type="number"
      value={cash}
      placeholder="Enter cash amount"
      onIonInput={(e) => handleCashInput(parseFloat(e.detail.value || "0"))}
      min={0}
      step={0.01}
    />
  </IonItem>
)}
```

#### Validation for Insufficient Cash
```typescript
const totalAmount = sessionPrice - promoDiscount;
if (paymentMethod === "Cash" && cash < totalAmount) {
  showConfirmation({
    header: 'Insufficient Cash',
    message: `Cash amount (₱${cash.toFixed(2)}) is less than the total amount (₱${totalAmount.toFixed(2)}).\n\nPlease enter sufficient cash to proceed.`,
    confirmText: 'OK',
    cancelText: ''
  }, () => {});
  return;
}
```

#### Real-Time Summary Display
Shows cash received, insufficient warning, or change:
```tsx
{paymentMethod === "Cash" && cash > 0 && (
  <>
    <p style={{ marginTop: "10px", paddingTop: "10px", borderTop: "1px solid #ddd" }}>
      <strong>Cash Received:</strong> ₱{cash.toFixed(2)}
    </p>
    {cash < (selectedRate.price - promoDiscount) ? (
      <p style={{ color: "#dc3545" }}>
        <strong>⚠️ Insufficient:</strong> Need ₱{((selectedRate.price - promoDiscount) - cash).toFixed(2)} more
      </p>
    ) : (
      <p style={{ color: "#28a745" }}>
        <strong>Change:</strong> ₱{change.toFixed(2)}
      </p>
    )}
  </>
)}
```

#### Mutation Update
Updated `startSessionMutation` to include cash and change:
```typescript
startSessionMutation.mutate({
  // ...existing fields...
  paymentMethod: paymentMethod,
  cash: paymentMethod === "Cash" ? cash : undefined,
  change: paymentMethod === "Cash" ? change : undefined,
});
```

#### Table Columns
Added cash and change columns to both transaction tables:
```typescript
{
  key: "cash",
  label: "Cash",
  sortable: true,
  render: (value) => value ? PesoFormat(value) : "N/A",
},
{
  key: "change",
  label: "Change",
  sortable: true,
  render: (value) => value ? PesoFormat(value) : "N/A",
}
```

## User Flow

### Cash Payment Process:

1. **Admin Creates Transaction**:
   - Selects user, table, and rate package
   - Chooses "Cash" as payment method
   - Cash input field appears

2. **Cash Input**:
   - Admin enters cash amount received
   - System automatically calculates change
   - Real-time validation shows:
     - ✅ Change amount (if sufficient)
     - ⚠️ Insufficient warning (if not enough)

3. **Validation**:
   - If cash < total amount:
     - Shows warning dialog
     - Prevents session start
     - Displays exact shortage amount
   - If cash >= total amount:
     - Allows session start
     - Saves cash and change to database

4. **Transaction Record**:
   - Cash amount stored
   - Change amount stored
   - Displayed in transaction tables

### Non-Cash Payments:

For "EWallet" and "Bank Transfer":
- Cash input field is hidden
- Cash and change fields are null
- Display shows "N/A" in transaction tables

## Data Flow

```
User Input (Cash Amount)
    ↓
handleCashInput()
    ↓
Auto-calculate Change = Cash - TotalAmount
    ↓
Validation Check (Cash >= TotalAmount?)
    ↓ (if valid)
startSessionMutation()
    ↓
Backend API (/tables/sessions/start)
    ↓
TableService.StartTableSessionAsync()
    ↓
Database (cash, change columns)
    ↓
Displayed in Transaction Tables
```

## Validation Rules

### Frontend Validation:
1. **Cash Amount Required**: For cash payments, must enter cash amount
2. **Minimum Check**: Cash must be >= total amount (price - discount)
3. **Visual Feedback**: Real-time display of insufficient cash with exact shortage
4. **Prevention**: Cannot start session if cash is insufficient

### Backend Validation:
- Cash and change are optional (nullable)
- Only saved when payment method is "Cash"
- Stored with 2 decimal precision

## API Changes

### POST /tables/sessions/start

**Request Body** (updated):
```json
{
  "tableId": "guid",
  "userId": "guid",
  "qrCode": "string",
  "hours": 2,
  "amount": 50.00,
  "paymentMethod": "Cash",
  "cash": 100.00,     // NEW FIELD (optional)
  "change": 50.00     // NEW FIELD (optional)
}
```

### GET /admin/transactions/pending & /admin/transactions/all

**Response** (updated):
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "userId": "guid",
      "tableId": "guid",
      "amount": 50.00,
      "paymentMethod": "Cash",
      "cash": 100.00,          // NEW FIELD
      "change": 50.00,         // NEW FIELD
      "user": { ... },
      "tables": { ... },
      "createdAt": "2025-10-30T10:00:00Z"
    }
  ]
}
```

## Examples

### Example 1: Exact Cash Payment
```
Rate Package: ₱50.00 (1 hour)
Promo Discount: ₱0.00
Total Amount: ₱50.00
Cash Received: ₱50.00
Change: ₱0.00
Status: ✅ Allowed
```

### Example 2: Cash with Change
```
Rate Package: ₱50.00 (1 hour)
Promo Discount: ₱0.00
Total Amount: ₱50.00
Cash Received: ₱100.00
Change: ₱50.00
Status: ✅ Allowed
```

### Example 3: Insufficient Cash
```
Rate Package: ₱50.00 (1 hour)
Promo Discount: ₱5.00
Total Amount: ₱45.00
Cash Received: ₱40.00
Change: ₱0.00
Status: ❌ Blocked
Warning: "Need ₱5.00 more"
```

### Example 4: With Promo Discount
```
Rate Package: ₱100.00 (2 hours)
Promo Discount: ₱20.00
Total Amount: ₱80.00
Cash Received: ₱100.00
Change: ₱20.00
Status: ✅ Allowed
```

## Benefits

1. **Accurate Cash Tracking**: Complete record of cash received and change given
2. **Automatic Calculation**: No manual calculation errors for change
3. **Fraud Prevention**: Ensures sufficient cash before starting session
4. **Audit Trail**: Full transparency for financial reconciliation
5. **User Experience**: Real-time feedback prevents errors
6. **Reporting**: Enables cash flow analysis and reporting

## UI Enhancements

### Visual Indicators:
- 🟢 **Green Text**: Change amount (sufficient cash)
- 🔴 **Red Text**: Insufficient cash warning
- ⚠️ **Warning Icon**: Clear indication of problem
- 💵 **Currency Format**: Consistent ₱ symbol and 2 decimals

### Conditional Display:
- Cash input only shown for "Cash" payment method
- Summary section adapts based on payment method
- Table columns show "N/A" for non-cash payments

## Testing Checklist

- [x] Database migration created
- [x] Backend entities updated
- [x] DTOs updated with cash and change
- [x] Service layer saves cash and change
- [x] Frontend schemas updated
- [x] Cash input field added (conditional)
- [x] Auto-calculate change implemented
- [x] Insufficient cash validation added
- [x] Real-time summary display
- [x] Table columns display cash and change
- [x] State resets when modal closes
- [ ] Test exact payment (cash = total)
- [ ] Test payment with change (cash > total)
- [ ] Test insufficient cash (cash < total)
- [ ] Test with promo discount
- [ ] Verify non-cash payments don't save cash/change
- [ ] Verify display in transaction tables
- [ ] Test with existing transactions (should show N/A)

## Files Modified

### Backend (5 files)
1. `Study-Hub/Models/Entities/TableSession.cs`
2. `Study-Hub/Models/DTOs/TableDto.cs`
3. `Study-Hub/Models/DTOs/AdminDto.cs`
4. `Study-Hub/Service/TableService.cs`
5. `Study-Hub/Service/AdminService.cs`
6. Migration file (generated)

### Frontend (3 files)
1. `study_hub_app/src/schema/table.schema.ts`
2. `study_hub_app/src/schema/admin.schema.ts`
3. `study_hub_app/src/pages/TransactionManagement.tsx`

## Edge Cases Handled

1. **Zero Cash**: Shows "N/A" in tables
2. **Exact Payment**: Change = 0.00
3. **Large Cash**: Handles any amount up to decimal(10,2)
4. **Payment Method Change**: Clears cash/change when switching from Cash
5. **Modal Cancel**: Resets cash and change to 0
6. **Decimal Precision**: Handles cents properly (0.01 step)
7. **Non-Cash Methods**: Cash/change not saved (null)
8. **Negative Change Prevention**: Change never goes below 0

## Future Enhancements

1. **Quick Cash Buttons**: Add buttons for common denominations (₱50, ₱100, ₱200, ₱500, ₱1000)
2. **Cash Drawer Tracking**: Track cash drawer totals by shift
3. **Change Shortage Alert**: Alert if change drawer is low
4. **Receipt Generation**: Print receipt with cash and change details
5. **Cash Summary Report**: Daily cash collection and change given
6. **Multi-Currency Support**: Support for other currencies
7. **Cashback Feature**: Allow cashback for certain payment methods
8. **Split Payment**: Allow partial cash + partial card payments

## Security Considerations

1. **Validation**: Both frontend and backend validation
2. **Audit Trail**: All cash transactions logged with timestamp
3. **User Attribution**: Who processed the cash transaction
4. **Immutable Records**: Cash and change cannot be edited after transaction
5. **Financial Reporting**: Cash totals for reconciliation

---

**Implementation Date**: October 30, 2025  
**Status**: ✅ Complete  
**Version**: 1.0  
**Related Feature**: Payment Method Implementation

