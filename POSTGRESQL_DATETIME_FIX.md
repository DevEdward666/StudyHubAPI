# PostgreSQL DateTime UTC Fix

## Problem
```
Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone', 
only UTC is supported. Note that it's not possible to mix DateTimes with different Kinds 
in an array, range, or multirange. (Parameter 'value')
```

## Root Cause
PostgreSQL's `timestamp with time zone` column type requires DateTime values with `DateTimeKind.Utc`. When creating DateTime objects in C# without specifying the kind, they default to `DateTimeKind.Unspecified`, which causes this error.

### Common Problematic Patterns:
```csharp
// ❌ BAD - Creates DateTime with Unspecified kind
var date = new DateTime(2024, 10, 25);
var targetDate = someDate?.Date;

// ✅ GOOD - Explicitly specify UTC kind
var date = DateTime.SpecifyKind(new DateTime(2024, 10, 25), DateTimeKind.Utc);
var targetDate = someDate.HasValue 
    ? DateTime.SpecifyKind(someDate.Value.Date, DateTimeKind.Utc) 
    : DateTime.UtcNow.Date;
```

## Solution Applied

### File: `Study-Hub/Service/ReportService.cs`

#### 1. Fixed GetTransactionReportAsync
```csharp
public async Task<TransactionReportDto> GetTransactionReportAsync(
    ReportPeriod period, DateTime? startDate = null, DateTime? endDate = null)
{
    // Ensure dates are UTC if provided
    var utcStartDate = startDate.HasValue 
        ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc) 
        : (DateTime?)null;
    
    return period switch
    {
        ReportPeriod.Daily => await GetDailyReportAsync(utcStartDate),
        ReportPeriod.Weekly => await GetWeeklyReportAsync(utcStartDate),
        ReportPeriod.Monthly => await GetMonthlyReportAsync(utcStartDate?.Year, utcStartDate?.Month),
        _ => throw new ArgumentException("Invalid report period")
    };
}
```

#### 2. Fixed GetDailyReportAsync
```csharp
public async Task<TransactionReportDto> GetDailyReportAsync(DateTime? date = null)
{
    var targetDate = date.HasValue 
        ? DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc) 
        : DateTime.UtcNow.Date;
    var startDate = targetDate;
    var endDate = targetDate.AddDays(1).AddTicks(-1);

    return await GenerateReportAsync(ReportPeriod.Daily, startDate, endDate);
}
```

#### 3. Fixed GetWeeklyReportAsync
```csharp
public async Task<TransactionReportDto> GetWeeklyReportAsync(DateTime? weekStartDate = null)
{
    var targetDate = weekStartDate.HasValue 
        ? DateTime.SpecifyKind(weekStartDate.Value.Date, DateTimeKind.Utc) 
        : DateTime.UtcNow.Date;
    
    // Get the start of the week (Monday)
    var dayOfWeek = (int)targetDate.DayOfWeek;
    var startDate = targetDate.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1));
    var endDate = startDate.AddDays(7).AddTicks(-1);

    return await GenerateReportAsync(ReportPeriod.Weekly, startDate, endDate);
}
```

#### 4. Fixed GetMonthlyReportAsync
```csharp
public async Task<TransactionReportDto> GetMonthlyReportAsync(int? year = null, int? month = null)
{
    var targetYear = year ?? DateTime.UtcNow.Year;
    var targetMonth = month ?? DateTime.UtcNow.Month;
    
    var startDate = DateTime.SpecifyKind(
        new DateTime(targetYear, targetMonth, 1), 
        DateTimeKind.Utc
    );
    var endDate = startDate.AddMonths(1).AddTicks(-1);

    return await GenerateReportAsync(ReportPeriod.Monthly, startDate, endDate);
}
```

## Key Changes Summary

✅ All nullable DateTime parameters are now converted to UTC using `DateTime.SpecifyKind()`  
✅ DateTime constructors explicitly specify `DateTimeKind.Utc`  
✅ Changed namespace from `Study_Hub.Services` to `Study_Hub.Service` to match file location  
✅ Updated DI registration in `Program.cs`  

## Testing

After this fix, all report endpoints should work correctly:

### Daily Report
```bash
curl -X GET "http://localhost:5000/api/report/transactions/daily" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Weekly Report
```bash
curl -X GET "http://localhost:5000/api/report/transactions/weekly" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Monthly Report
```bash
curl -X GET "http://localhost:5000/api/report/transactions/monthly?year=2024&month=10" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Custom Report with Date Range
```bash
curl -X POST "http://localhost:5000/api/report/transactions" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "period": "Daily",
    "startDate": "2024-10-01T00:00:00Z",
    "endDate": "2024-10-25T23:59:59Z"
  }'
```

**Important**: When sending dates from the client, always use ISO 8601 format with 'Z' suffix to indicate UTC:
- ✅ `"2024-10-25T00:00:00Z"` (UTC)
- ✅ `"2024-10-25T10:30:00+00:00"` (explicit UTC offset)
- ❌ `"2024-10-25T10:30:00"` (no timezone, will be treated as unspecified)

## Best Practices for PostgreSQL + EF Core + DateTime

1. **Always use UTC times** when working with PostgreSQL `timestamp with time zone`
2. **Store times as UTC** in the database
3. **Convert to local time only in the presentation layer** (frontend)
4. **Use `DateTime.UtcNow`** instead of `DateTime.Now`
5. **Explicitly specify DateTimeKind** when constructing DateTime from components
6. **Use ISO 8601 format with timezone** in API requests/responses

## Verification

Run the project and test the endpoints. The error should no longer appear:

```bash
cd Study-Hub
dotnet build
dotnet run
```

All compile errors have been resolved. The system now correctly handles DateTime values for PostgreSQL.

