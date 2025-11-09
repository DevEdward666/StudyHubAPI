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
                AverageAmount = totalTransactions > 0 ? totalAmount / totalTransactions : null
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
            if (report.Summary.AverageAmount.HasValue)
            {
                csv.AppendLine($"Average Amount,{report.Summary.AverageAmount.Value:F2}");
            }
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
    }
}

