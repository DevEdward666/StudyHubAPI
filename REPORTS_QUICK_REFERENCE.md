# Transaction Reports - Quick Reference Guide

## 🚀 Quick Start

### Get Today's Report
```bash
GET /api/report/transactions/daily
```

### Get This Week's Report
```bash
GET /api/report/transactions/weekly
```

### Get This Month's Report
```bash
GET /api/report/transactions/monthly
```

### Get Dashboard Quick Stats
```bash
GET /api/report/transactions/quick-stats
```

---

## 📊 Report Types

| Type | Endpoint | Parameters | Description |
|------|----------|------------|-------------|
| Daily | `GET /api/report/transactions/daily` | `?date=YYYY-MM-DD` | Single day report |
| Weekly | `GET /api/report/transactions/weekly` | `?weekStartDate=YYYY-MM-DD` | Monday-Sunday report |
| Monthly | `GET /api/report/transactions/monthly` | `?year=YYYY&month=MM` | Full month report |
| Custom | `POST /api/report/transactions` | JSON body | Flexible period |
| Export | `POST /api/report/transactions/export` | JSON body | Download CSV/JSON |
| Quick Stats | `GET /api/report/transactions/quick-stats` | None | Dashboard overview |

---

## 📈 What's Included in Each Report

### Summary Section
- Total transactions count
- Total amount (credits)
- Total cost (money)
- Average transaction amount
- Count by status (Approved/Pending/Rejected)
- Amount by status

### Analysis Sections
- **By Status**: Distribution across Approved/Pending/Rejected
- **By Payment Method**: GCash, PayPal, Admin Credit, etc.
- **Daily Breakdown**: Day-by-day trends
- **Top Users**: Top 10 users by transaction volume

---

## 💾 Export Options

### CSV Export
```bash
POST /api/report/transactions/export
{
  "period": "Monthly",
  "startDate": "2025-10-01T00:00:00Z",
  "endDate": "2025-10-31T23:59:59Z",
  "format": "csv"
}
```
Downloads: `transaction_report_Monthly_20251025103000.csv`

### JSON Export
```bash
POST /api/report/transactions/export
{
  "period": "Monthly",
  "startDate": "2025-10-01T00:00:00Z",
  "endDate": "2025-10-31T23:59:59Z",
  "format": "json"
}
```
Returns: Complete JSON report structure

---

## 🔐 Authentication

All endpoints require:
```
Authorization: Bearer {admin-jwt-token}
```

Only users with admin role can access reports.

---

## 📅 Date Handling

### Daily Reports
- Default: Today (UTC)
- Custom: `?date=2025-10-25`

### Weekly Reports
- Default: Current week (Monday as start)
- Custom: `?weekStartDate=2025-10-21`
- Always spans Monday-Sunday

### Monthly Reports
- Default: Current month
- Custom: `?year=2025&month=10`
- Includes full calendar month

---

## 🎯 Use Cases

### Dashboard Widget
```javascript
// Get quick overview for dashboard
GET /api/report/transactions/quick-stats

// Returns today, this week, and this month stats
```

### Daily Operations Review
```javascript
// Check today's transactions
GET /api/report/transactions/daily

// Review status breakdown, top users, payment methods
```

### Weekly Business Review
```javascript
// Monday morning review
GET /api/report/transactions/weekly

// See week trends, identify patterns
```

### Monthly Financial Report
```javascript
// End of month analysis
GET /api/report/transactions/monthly?year=2025&month=10

// Export for accounting
POST /api/report/transactions/export
{
  "period": "Monthly",
  "startDate": "2025-10-01T00:00:00Z",
  "endDate": "2025-10-31T23:59:59Z",
  "format": "csv"
}
```

### Custom Period Analysis
```javascript
// Analyze specific date range
POST /api/report/transactions
{
  "period": "Weekly",
  "startDate": "2025-10-01T00:00:00Z",
  "endDate": "2025-10-07T23:59:59Z"
}
```

---

## 📊 Sample Report Structure

```json
{
  "period": "Daily",
  "startDate": "2025-10-25T00:00:00Z",
  "endDate": "2025-10-25T23:59:59.999Z",
  "summary": {
    "totalTransactions": 45,
    "totalAmount": 4500.00,
    "totalCost": 4050.00,
    "averageTransactionAmount": 100.00,
    "approvedCount": 40,
    "pendingCount": 3,
    "rejectedCount": 2
  },
  "byStatus": [...],
  "byPaymentMethod": [...],
  "dailyBreakdown": [...],
  "topUsers": [...]
}
```

---

## 🎨 Integration Examples

### React Dashboard Component
```javascript
async function fetchQuickStats() {
  const response = await fetch('/api/report/transactions/quick-stats', {
    headers: {
      'Authorization': `Bearer ${adminToken}`
    }
  });
  const data = await response.json();
  return data.data;
}
```

### Download Monthly CSV
```javascript
async function downloadMonthlyReport(year, month) {
  const response = await fetch('/api/report/transactions/export', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${adminToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      period: 'Monthly',
      startDate: `${year}-${month}-01T00:00:00Z`,
      endDate: new Date(year, month, 0).toISOString(),
      format: 'csv'
    })
  });
  
  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `report_${year}_${month}.csv`;
  a.click();
}
```

### Python Analysis Script
```python
import requests
import pandas as pd
from io import StringIO

# Get CSV report
response = requests.post(
    'http://localhost:5000/api/report/transactions/export',
    headers={'Authorization': f'Bearer {admin_token}'},
    json={
        'period': 'Monthly',
        'startDate': '2025-10-01T00:00:00Z',
        'endDate': '2025-10-31T23:59:59Z',
        'format': 'csv'
    }
)

# Parse CSV with pandas
df = pd.read_csv(StringIO(response.text), skiprows=5)
print(df.describe())
```

---

## ⚡ Performance Tips

1. **Caching**: Results are computed on-demand; consider caching for frequently accessed reports
2. **Date Ranges**: Larger date ranges take longer to process
3. **Peak Hours**: Schedule large report exports during off-peak hours
4. **Pagination**: Top users limited to 10; daily breakdown includes all days in period

---

## 🐛 Troubleshooting

### 401 Unauthorized
- Check if token is valid
- Ensure token is included in Authorization header

### 403 Forbidden
- Verify user has admin role
- Check admin status with `/api/admin/is-admin`

### Empty Reports
- Verify transactions exist in date range
- Check database for data availability
- Confirm timezone (all dates in UTC)

### CSV Not Downloading
- Check format parameter is "csv"
- Verify Content-Type in response
- Use proper download handling in frontend

---

## 📞 Support

For issues or questions:
1. Check the complete documentation: `TRANSACTION_REPORTS_IMPLEMENTATION.md`
2. Review test examples: `test-reports.http`
3. Verify build status: `dotnet build`

---

## ✅ Checklist for First Use

- [ ] Ensure admin user exists
- [ ] Get admin JWT token
- [ ] Test `/api/report/transactions/quick-stats` first
- [ ] Verify response structure
- [ ] Try daily report for today
- [ ] Test CSV export with small date range
- [ ] Integrate into dashboard/frontend
- [ ] Set up scheduled report generation (optional)

---

**Report System Ready!** 🎉

Start with `/api/report/transactions/quick-stats` to get an overview, then explore specific reports as needed.

