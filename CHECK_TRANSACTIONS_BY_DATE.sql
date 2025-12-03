# Quick Check: What Dates Have Transactions?

Run this query in DBBeaver to see which dates have transactions:

```sql
-- Show all transactions grouped by Philippine date
SELECT 
    DATE(purchase_date AT TIME ZONE 'UTC' AT TIME ZONE 'Asia/Manila') as philippine_date,
    COUNT(*) as transaction_count,
    SUM(purchase_amount) as total_amount,
    MIN(purchase_date) as first_transaction_utc,
    MAX(purchase_date) as last_transaction_utc
FROM user_subscriptions
WHERE purchase_date >= '2025-11-01'::timestamptz
GROUP BY DATE(purchase_date AT TIME ZONE 'UTC' AT TIME ZONE 'Asia/Manila')
ORDER BY philippine_date DESC;
```

This will show you:
- Which Philippine dates have transactions
- How many transactions per date
- Total amount per date
- First and last transaction times in UTC

## Quick Answer

If the result shows:
```
philippine_date | transaction_count | total_amount
2025-12-03      | 5                 | 750.00
2025-12-02      | 3                 | 450.00
2025-12-01      | 0                 | 0
```

Then:
- ✅ Daily report for Dec 1: 0 transactions (correct!)
- ✅ Quick-stats "today" (Dec 3): 5 transactions (correct!)

**Conclusion**: Quick-stats is working correctly - it's showing data for December 3 (today), not December 1.

---

## If There Are NO Transactions for Dec 3

If the query shows 0 transactions for December 3, but quick-stats shows data, then run:

```sql
-- Check the EXACT date range quick-stats is querying
-- Philippine Dec 3 = UTC Dec 2 16:00 to UTC Dec 3 16:00
SELECT 
    COUNT(*) as count,
    SUM(purchase_amount) as total,
    MIN(purchase_date) as earliest,
    MAX(purchase_date) as latest
FROM user_subscriptions
WHERE purchase_date >= '2025-12-02 16:00:00'::timestamptz
  AND purchase_date < '2025-12-03 16:00:00'::timestamptz;
```

This should match quick-stats "today" value.

