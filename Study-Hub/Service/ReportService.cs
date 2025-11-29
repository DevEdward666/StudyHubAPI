using Microsoft.EntityFrameworkCore;
using Study_Hub.Data;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using Study_Hub.Services.Interfaces;
using System.Text;

namespace Study_Hub.Service
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TransactionReportDto> GetTransactionReportAsync(ReportPeriod period, DateTime? startDate = null, DateTime? endDate = null)
        {
            // Ensure dates are UTC if provided
            var utcStartDate = startDate.HasValue ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc) : (DateTime?)null;
            
            return period switch
            {
                ReportPeriod.Daily => await GetDailyReportAsync(utcStartDate),
                ReportPeriod.Weekly => await GetWeeklyReportAsync(utcStartDate),
                ReportPeriod.Monthly => await GetMonthlyReportAsync(utcStartDate?.Year, utcStartDate?.Month),
                _ => throw new ArgumentException("Invalid report period")
            };
        }

        public async Task<TransactionReportDto> GetDailyReportAsync(DateTime? date = null)
        {
            var targetDate = date.HasValue 
                ? DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc) 
                : DateTime.UtcNow.Date;
            var startDate = targetDate;
            var endDate = targetDate.AddDays(1).AddTicks(-1);

            return await GenerateReportAsync(ReportPeriod.Daily, startDate, endDate);
        }

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

        public async Task<TransactionReportDto> GetMonthlyReportAsync(int? year = null, int? month = null)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;
            var targetMonth = month ?? DateTime.UtcNow.Month;
            
            var startDate = DateTime.SpecifyKind(new DateTime(targetYear, targetMonth, 1), DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1).AddTicks(-1);

            return await GenerateReportAsync(ReportPeriod.Monthly, startDate, endDate);
        }

        private async Task<TransactionReportDto> GenerateReportAsync(ReportPeriod period, DateTime startDate, DateTime endDate)
        {
            // Get all user subscriptions (purchases) in the period
            var subscriptions = await _context.UserSubscriptions
                .Include(s => s.User)
                .Include(s => s.Package)
                .Where(s => s.PurchaseDate >= startDate && s.PurchaseDate <= endDate)
                .ToListAsync();

            // Calculate summary
            var summary = CalculateSummaryFromSubscriptions(subscriptions);

            // Top users
            var topUsers = CalculateTopUsersFromSubscriptions(subscriptions);

            return new TransactionReportDto
            {
                Period = period,
                StartDate = startDate,
                EndDate = endDate,
                Summary = summary,
                TopUsers = topUsers
            };
        }

        private TransactionSummaryDto CalculateSummaryFromSubscriptions(List<UserSubscription> subscriptions)
        {
            var totalTransactions = subscriptions.Count;
            var totalAmount = subscriptions.Sum(s => s.PurchaseAmount);

            return new TransactionSummaryDto
            {
                TotalTransactions = totalTransactions,
                TotalAmount = totalAmount,
                AverageAmount = totalTransactions > 0 ? totalAmount / totalTransactions : 0
            };
        }

        private List<TopUserDto> CalculateTopUsersFromSubscriptions(List<UserSubscription> subscriptions)
        {
            return subscriptions
                .GroupBy(s => new { s.UserId, s.User.Name, s.User.Email })
                .Select(g => new TopUserDto
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.Name,
                    UserEmail = g.Key.Email,
                    TransactionCount = g.Count(),
                    TotalAmount = g.Sum(s => s.PurchaseAmount)
                })
                .OrderByDescending(u => u.TotalAmount)
                .Take(10)
                .ToList();
        }

        public async Task<string> ExportReportToCsvAsync(ReportPeriod period, DateTime? startDate = null, DateTime? endDate = null)
        {
            var report = await GetTransactionReportAsync(period, startDate, endDate);
            var csv = new StringBuilder();

            // Header
            csv.AppendLine($"Transaction Report - {period}");
            csv.AppendLine($"Period: {report.StartDate:yyyy-MM-dd} to {report.EndDate:yyyy-MM-dd}");
            csv.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            csv.AppendLine();

            // Summary
            csv.AppendLine("SUMMARY");
            csv.AppendLine("Metric,Value");
            csv.AppendLine($"Total Transactions,{report.Summary.TotalTransactions}");
            csv.AppendLine($"Total Amount,{report.Summary.TotalAmount:F2}");
            csv.AppendLine($"Average Amount,{report.Summary.AverageAmount:F2}");
            csv.AppendLine();

            // Top Users
            csv.AppendLine("TOP USERS");
            csv.AppendLine("Email,Name,Transaction Count,Total Amount");
            foreach (var user in report.TopUsers)
            {
                csv.AppendLine($"{user.UserEmail},{user.UserName ?? "N/A"},{user.TransactionCount},{user.TotalAmount:F2}");
            }

            return csv.ToString();
        }

        public async Task<object> GetDailySalesReportAsync(DateTime date)
        {
            var targetDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var startDate = targetDate;
            var endDate = targetDate.AddDays(1).AddTicks(-1);

            // Get all subscriptions for the day
            var subscriptions = await _context.UserSubscriptions
                .Include(s => s.User)
                .Include(s => s.Package)
                .Where(s => s.PurchaseDate >= startDate && s.PurchaseDate <= endDate)
                .OrderBy(s => s.PurchaseDate)
                .ToListAsync();

            // Calculate totals by package type
            var packageSummary = subscriptions
                .GroupBy(s => s.Package.PackageType)
                .Select(g => new
                {
                    PackageType = g.Key,
                    TotalSales = g.Count(),
                    TotalRevenue = g.Sum(s => s.PurchaseAmount),
                    AveragePrice = g.Average(s => s.PurchaseAmount)
                })
                .OrderByDescending(p => p.TotalRevenue)
                .ToList();

            // Detailed transactions
            var detailedTransactions = subscriptions.Select(s => new
            {
                TransactionId = s.Id.ToString(),
                Time = s.PurchaseDate.ToString("yyyy-MM-dd HH:mm:ss"),
                CustomerName = s.User.Name ?? "N/A",
                CustomerEmail = s.User.Email,
                PackageName = s.Package.Name,
                PackageType = s.Package.PackageType,
                Duration = $"{s.Package.DurationValue} {s.Package.PackageType}",
                TotalHours = s.Package.TotalHours,
                Price = s.PurchaseAmount,
                PaymentMethod = s.PaymentMethod ?? "N/A",
                Status = "Completed"
            }).ToList();

            // Calculate summary
            var totalSales = subscriptions.Count;
            var totalRevenue = subscriptions.Sum(s => s.PurchaseAmount);
            var averageTransactionValue = totalSales > 0 ? totalRevenue / totalSales : 0;

            return new
            {
                ReportDate = targetDate,
                GeneratedAt = DateTime.UtcNow,
                CompanyName = "Sunny Side Up Work + Study",
                ReportTitle = "Daily Sales Report",
                Summary = new
                {
                    TotalSales = totalSales,
                    TotalRevenue = totalRevenue,
                    AverageTransactionValue = averageTransactionValue,
                    UniqueCustomers = subscriptions.Select(s => s.UserId).Distinct().Count()
                },
                PackageBreakdown = packageSummary,
                DetailedTransactions = detailedTransactions,
                Period = new
                {
                    Start = startDate,
                    End = endDate
                }
            };
        }

        public async Task<string> ExportDailySalesReportToCsvAsync(DateTime date)
        {
            var salesReport = await GetDailySalesReportAsync(date);
            dynamic report = salesReport;
            
            var csv = new StringBuilder();

            // Header
            csv.AppendLine("================================================================================");
            csv.AppendLine($"{report.CompanyName}");
            csv.AppendLine($"Daily Transaction Report");
            csv.AppendLine($"Report Date: {report.ReportDate}");
            csv.AppendLine($"Generated: {report.GeneratedAt}");
            csv.AppendLine("================================================================================");
            csv.AppendLine();

            // Detailed Transactions
            csv.AppendLine("Transaction ID,Date,Time,Customer Name,Customer Email,Package Name,Package Type,Duration,Total Hours,Price,Payment Method,Status");
            
            foreach (var transaction in report.DetailedTransactions)
            {
                csv.AppendLine($"{transaction.TransactionId},{report.ReportDate:yyyy-MM-dd},{transaction.Time},{transaction.CustomerName},{transaction.CustomerEmail},{transaction.PackageName},{transaction.PackageType},{transaction.Duration},{transaction.TotalHours},₱{transaction.Price},{transaction.PaymentMethod},{transaction.Status}");
            }
            
            csv.AppendLine();
            csv.AppendLine($"Total Transactions: {report.Summary.TotalSales}");
            csv.AppendLine($"Total Revenue: ₱{report.Summary.TotalRevenue}");
            csv.AppendLine();
            csv.AppendLine("================================================================================");
            csv.AppendLine("End of Report");
            csv.AppendLine("================================================================================");

            return csv.ToString();
        }

        public async Task<byte[]> ExportDailySalesReportToPdfAsync(DateTime date)
        {
            var salesReport = await GetDailySalesReportAsync(date);
            dynamic report = salesReport;
            
            var html = new StringBuilder();
            
            // HTML structure for PDF generation
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<title>Daily Sales Report</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; }");
            html.AppendLine("h1 { color: #333; text-align: center; margin-bottom: 5px; }");
            html.AppendLine("h2 { color: #666; text-align: center; margin-top: 0; font-size: 18px; }");
            html.AppendLine(".header { text-align: center; margin-bottom: 30px; border-bottom: 2px solid #333; padding-bottom: 10px; }");
            html.AppendLine(".meta { text-align: center; color: #666; font-size: 12px; margin-bottom: 20px; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            html.AppendLine("th { background-color: #333; color: white; padding: 10px; text-align: left; font-size: 11px; }");
            html.AppendLine("td { border: 1px solid #ddd; padding: 8px; font-size: 10px; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
            html.AppendLine(".footer { margin-top: 30px; padding-top: 20px; border-top: 2px solid #333; }");
            html.AppendLine(".total { font-size: 14px; font-weight: bold; margin: 5px 0; }");
            html.AppendLine("@media print { body { margin: 20px; } }");
            html.AppendLine("</style></head><body>");
            
            // Header
            html.AppendLine($"<div class='header'>");
            html.AppendLine($"<h1>{report.CompanyName}</h1>");
            html.AppendLine($"<h2>Daily Transaction Report</h2>");
            html.AppendLine($"</div>");
            
            html.AppendLine($"<div class='meta'>");
            html.AppendLine($"<p>Report Date: {report.ReportDate:MMMM dd, yyyy}</p>");
            html.AppendLine($"<p>Generated: {report.GeneratedAt:MMMM dd, yyyy HH:mm:ss} UTC</p>");
            html.AppendLine($"</div>");
            
            // Transactions table
            html.AppendLine("<table>");
            html.AppendLine("<thead><tr>");
            html.AppendLine("<th>Transaction ID</th><th>Time</th><th>Customer</th><th>Email</th>");
            html.AppendLine("<th>Package</th><th>Type</th><th>Duration</th><th>Hours</th><th>Price</th><th>Payment</th>");
            html.AppendLine("</tr></thead><tbody>");
            
            foreach (var transaction in report.DetailedTransactions)
            {
                string transId = transaction.TransactionId.ToString();
                string shortId = transId.Length > 13 ? transId.Substring(0, 13) + "..." : transId;
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{shortId}</td>");
                html.AppendLine($"<td>{transaction.Time.ToString().Split(' ')[1]}</td>");
                html.AppendLine($"<td>{transaction.CustomerName}</td>");
                html.AppendLine($"<td>{transaction.CustomerEmail}</td>");
                html.AppendLine($"<td>{transaction.PackageName}</td>");
                html.AppendLine($"<td>{transaction.PackageType}</td>");
                html.AppendLine($"<td>{transaction.Duration}</td>");
                html.AppendLine($"<td>{transaction.TotalHours}</td>");
                html.AppendLine($"<td>₱{transaction.Price:N2}</td>");
                html.AppendLine($"<td>{transaction.PaymentMethod}</td>");
                html.AppendLine("</tr>");
            }
            
            html.AppendLine("</tbody></table>");
            
            // Footer with totals
            html.AppendLine("<div class='footer'>");
            html.AppendLine($"<p class='total'>Total Transactions: {report.Summary.TotalSales}</p>");
            html.AppendLine($"<p class='total'>Total Revenue: ₱{report.Summary.TotalRevenue:N2}</p>");
            html.AppendLine("</div>");
            
            html.AppendLine("</body></html>");
            
            return Encoding.UTF8.GetBytes(html.ToString());
        }

        public async Task<object> GetSalesReportAsync(ReportPeriod period, DateTime startDate, DateTime endDate)
        {
            var targetStartDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var targetEndDate = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            // Get all subscriptions for the period
            var subscriptions = await _context.UserSubscriptions
                .Include(s => s.User)
                .Include(s => s.Package)
                .Where(s => s.PurchaseDate >= targetStartDate && s.PurchaseDate <= targetEndDate)
                .OrderBy(s => s.PurchaseDate)
                .ToListAsync();

            // Calculate totals by package type
            var packageSummary = subscriptions
                .GroupBy(s => s.Package.PackageType)
                .Select(g => new
                {
                    PackageType = g.Key,
                    TotalSales = g.Count(),
                    TotalRevenue = g.Sum(s => s.PurchaseAmount),
                    AveragePrice = g.Average(s => s.PurchaseAmount)
                })
                .OrderByDescending(p => p.TotalRevenue)
                .ToList();

            // Detailed transactions
            var detailedTransactions = subscriptions.Select(s => new
            {
                TransactionId = s.Id.ToString(),
                Time = s.PurchaseDate.ToString("yyyy-MM-dd HH:mm:ss"),
                CustomerName = s.User.Name ?? "N/A",
                CustomerEmail = s.User.Email,
                PackageName = s.Package.Name,
                PackageType = s.Package.PackageType,
                Duration = $"{s.Package.DurationValue} {s.Package.PackageType}",
                TotalHours = s.Package.TotalHours,
                Price = s.PurchaseAmount,
                PaymentMethod = s.PaymentMethod ?? "N/A",
                Status = "Completed"
            }).ToList();

            // Calculate summary
            var totalSales = subscriptions.Count;
            var totalRevenue = subscriptions.Sum(s => s.PurchaseAmount);
            var averageTransactionValue = totalSales > 0 ? totalRevenue / totalSales : 0;

            string periodName = period switch
            {
                ReportPeriod.Daily => "Daily",
                ReportPeriod.Weekly => "Weekly",
                ReportPeriod.Monthly => "Monthly",
                _ => "Custom Period"
            };

            return new
            {
                ReportDate = targetStartDate,
                StartDate = targetStartDate,
                EndDate = targetEndDate,
                GeneratedAt = DateTime.UtcNow,
                CompanyName = "Sunny Side Up Work + Study",
                ReportTitle = $"{periodName} Transaction Report",
                Period = periodName,
                Summary = new
                {
                    TotalSales = totalSales,
                    TotalRevenue = totalRevenue,
                    AverageTransactionValue = averageTransactionValue,
                    UniqueCustomers = subscriptions.Select(s => s.UserId).Distinct().Count()
                },
                PackageBreakdown = packageSummary,
                DetailedTransactions = detailedTransactions
            };
        }

        public async Task<string> ExportSalesReportToCsvAsync(ReportPeriod period, DateTime startDate, DateTime endDate)
        {
            var salesReport = await GetSalesReportAsync(period, startDate, endDate);
            dynamic report = salesReport;
            
            var csv = new StringBuilder();

            // Header
            csv.AppendLine("================================================================================");
            csv.AppendLine($"{report.CompanyName}");
            csv.AppendLine($"{report.ReportTitle}");
            csv.AppendLine($"Period: {report.StartDate:MMMM dd, yyyy} - {report.EndDate:MMMM dd, yyyy}");
            csv.AppendLine($"Generated: {report.GeneratedAt:MMMM dd, yyyy HH:mm:ss} UTC");
            csv.AppendLine("================================================================================");
            csv.AppendLine();

            // Detailed Transactions
            csv.AppendLine("Transaction ID,Date,Time,Customer Name,Customer Email,Package Name,Package Type,Duration,Total Hours,Price,Payment Method,Status");
            
            foreach (var transaction in report.DetailedTransactions)
            {
                string[] timeParts = transaction.Time.ToString().Split(' ');
                string date = timeParts.Length > 0 ? timeParts[0] : "";
                string time = timeParts.Length > 1 ? timeParts[1] : "";
                csv.AppendLine($"{transaction.TransactionId},{date},{time},{transaction.CustomerName},{transaction.CustomerEmail},{transaction.PackageName},{transaction.PackageType},{transaction.Duration},{transaction.TotalHours},₱{transaction.Price:N2},{transaction.PaymentMethod},{transaction.Status}");
            }
            
            csv.AppendLine();
            csv.AppendLine($"Total Transactions: {report.Summary.TotalSales}");
            csv.AppendLine($"Total Revenue: ₱{report.Summary.TotalRevenue:N2}");
            csv.AppendLine($"Average Transaction Value: ₱{report.Summary.AverageTransactionValue:N2}");
            csv.AppendLine($"Unique Customers: {report.Summary.UniqueCustomers}");
            csv.AppendLine();
            csv.AppendLine("================================================================================");
            csv.AppendLine("End of Report");
            csv.AppendLine("================================================================================");

            return csv.ToString();
        }

        public async Task<byte[]> ExportSalesReportToPdfAsync(ReportPeriod period, DateTime startDate, DateTime endDate)
        {
            var salesReport = await GetSalesReportAsync(period, startDate, endDate);
            dynamic report = salesReport;
            
            var html = new StringBuilder();
            
            // HTML structure for PDF generation
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine($"<title>{report.ReportTitle}</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; }");
            html.AppendLine("h1 { color: #333; text-align: center; margin-bottom: 5px; }");
            html.AppendLine("h2 { color: #666; text-align: center; margin-top: 0; font-size: 18px; }");
            html.AppendLine(".header { text-align: center; margin-bottom: 30px; border-bottom: 2px solid #333; padding-bottom: 10px; }");
            html.AppendLine(".meta { text-align: center; color: #666; font-size: 12px; margin-bottom: 20px; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            html.AppendLine("th { background-color: #333; color: white; padding: 10px; text-align: left; font-size: 11px; }");
            html.AppendLine("td { border: 1px solid #ddd; padding: 8px; font-size: 10px; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
            html.AppendLine(".footer { margin-top: 30px; padding-top: 20px; border-top: 2px solid #333; }");
            html.AppendLine(".total { font-size: 14px; font-weight: bold; margin: 5px 0; }");
            html.AppendLine("@media print { body { margin: 20px; } }");
            html.AppendLine("</style></head><body>");
            
            // Header
            html.AppendLine($"<div class='header'>");
            html.AppendLine($"<h1>{report.CompanyName}</h1>");
            html.AppendLine($"<h2>{report.ReportTitle}</h2>");
            html.AppendLine($"</div>");
            
            html.AppendLine($"<div class='meta'>");
            html.AppendLine($"<p>Period: {report.StartDate:MMMM dd, yyyy} - {report.EndDate:MMMM dd, yyyy}</p>");
            html.AppendLine($"<p>Generated: {report.GeneratedAt:MMMM dd, yyyy HH:mm:ss} UTC</p>");
            html.AppendLine($"</div>");
            
            // Transactions table
            html.AppendLine("<table>");
            html.AppendLine("<thead><tr>");
            html.AppendLine("<th>Transaction ID</th><th>Date</th><th>Time</th><th>Customer</th><th>Email</th>");
            html.AppendLine("<th>Package</th><th>Type</th><th>Duration</th><th>Hours</th><th>Price</th><th>Payment</th>");
            html.AppendLine("</tr></thead><tbody>");
            
            foreach (var transaction in report.DetailedTransactions)
            {
                string transId = transaction.TransactionId.ToString();
                string shortId = transId.Length > 13 ? transId.Substring(0, 13) + "..." : transId;
                string[] timeParts = transaction.Time.ToString().Split(' ');
                string date = timeParts.Length > 0 ? timeParts[0] : "";
                string time = timeParts.Length > 1 ? timeParts[1] : "";
                
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{shortId}</td>");
                html.AppendLine($"<td>{date}</td>");
                html.AppendLine($"<td>{time}</td>");
                html.AppendLine($"<td>{transaction.CustomerName}</td>");
                html.AppendLine($"<td>{transaction.CustomerEmail}</td>");
                html.AppendLine($"<td>{transaction.PackageName}</td>");
                html.AppendLine($"<td>{transaction.PackageType}</td>");
                html.AppendLine($"<td>{transaction.Duration}</td>");
                html.AppendLine($"<td>{transaction.TotalHours}</td>");
                html.AppendLine($"<td>₱{transaction.Price:N2}</td>");
                html.AppendLine($"<td>{transaction.PaymentMethod}</td>");
                html.AppendLine("</tr>");
            }
            
            html.AppendLine("</tbody></table>");
            
            // Footer with totals
            html.AppendLine("<div class='footer'>");
            html.AppendLine($"<p class='total'>Total Transactions: {report.Summary.TotalSales}</p>");
            html.AppendLine($"<p class='total'>Total Revenue: ₱{report.Summary.TotalRevenue:N2}</p>");
            html.AppendLine($"<p class='total'>Average Transaction Value: ₱{report.Summary.AverageTransactionValue:N2}</p>");
            html.AppendLine($"<p class='total'>Unique Customers: {report.Summary.UniqueCustomers}</p>");
            html.AppendLine("</div>");
            
            html.AppendLine("</body></html>");
            
            return Encoding.UTF8.GetBytes(html.ToString());
        }
    }
}
