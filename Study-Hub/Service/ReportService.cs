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
            // Get all table sessions in the period
            var sessions = await _context.TableSessions
                .Include(t => t.User)
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .ToListAsync();

            // Calculate summary
            var summary = CalculateSummaryFromSessions(sessions);

            // Group by status
            var byStatus = GroupSessionsByStatus(sessions);

            // Group by payment method
            var byPaymentMethod = GroupSessionsByPaymentMethod(sessions);

            // Daily breakdown
            var dailyBreakdown = CalculateSessionsDailyBreakdown(sessions, startDate, endDate);

            // Top users
            var topUsers = CalculateTopUsersFromSessions(sessions);

            return new TransactionReportDto
            {
                Period = period,
                StartDate = startDate,
                EndDate = endDate,
                Summary = summary,
                ByStatus = byStatus,
                ByPaymentMethod = byPaymentMethod,
                DailyBreakdown = dailyBreakdown,
                TopUsers = topUsers
            };
        }

        private TransactionSummaryDto CalculateSummaryFromSessions(List<TableSession> sessions)
        {
            var totalTransactions = sessions.Count;
            var totalAmount = sessions.Sum(t => t.Amount);

            return new TransactionSummaryDto
            {
                TotalTransactions = totalTransactions,
                TotalAmount = totalAmount,
                TotalCost = totalAmount, // For sessions, cost equals amount
                AverageTransactionAmount = totalTransactions > 0 ? totalAmount / totalTransactions : 0,
                ApprovedCount = 0,
                PendingCount = 0,
                RejectedCount = 0,
                ApprovedAmount = 0,
                PendingAmount = 0,
                RejectedAmount = 0
            };
        }

        private List<TransactionByStatusDto> GroupSessionsByStatus(List<TableSession> sessions)
        {
            var totalSessions = sessions.Count;

            return sessions
                .GroupBy(t => t.Status)
                .Select(g => new TransactionByStatusDto
                {
                    Status = g.Key == "active" ? TransactionStatus.Pending : TransactionStatus.Approved,
                    Count = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount),
                    TotalCost = g.Sum(t => t.Amount),
                    Percentage = totalSessions > 0 ? (decimal)g.Count() / totalSessions * 100 : 0
                })
                .OrderByDescending(s => s.Count)
                .ToList();
        }

        private List<TransactionByPaymentMethodDto> GroupSessionsByPaymentMethod(List<TableSession> sessions)
        {
            return sessions
                .Where(t => !string.IsNullOrEmpty(t.PaymentMethod))
                .GroupBy(t => t.PaymentMethod!)
                .Select(g => new TransactionByPaymentMethodDto
                {
                    PaymentMethod = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount),
                    AverageAmount = g.Average(t => t.Amount)
                })
                .OrderByDescending(pm => pm.TotalAmount)
                .ToList();
        }

        private List<DailyTransactionDto> CalculateSessionsDailyBreakdown(List<TableSession> sessions, DateTime startDate, DateTime endDate)
        {
            return sessions
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new DailyTransactionDto
                {
                    Date = DateTime.SpecifyKind(g.Key, DateTimeKind.Utc),
                    Count = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount),
                    TotalCost = g.Sum(t => t.Amount),
                    ApprovedCount = 0,
                    PendingCount = 0,
                    RejectedCount = 0
                })
                .OrderBy(d => d.Date)
                .ToList();
        }

        private List<TopUserDto> CalculateTopUsersFromSessions(List<TableSession> sessions)
        {
            return sessions
                .GroupBy(t => new { t.UserId, t.User.Name, t.User.Email })
                .Select(g => new TopUserDto
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.Name,
                    UserEmail = g.Key.Email,
                    TransactionCount = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount),
                    TotalCost = g.Sum(t => t.Amount)
                })
                .OrderByDescending(u => u.TotalAmount)
                .Take(10)
                .ToList();
        
        }

        private List<DailyTransactionDto> CalculateDailyBreakdown(List<CreditTransaction> transactions, DateTime startDate, DateTime endDate)
        {
            var dailyData = new List<DailyTransactionDto>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var dayTransactions = transactions
                    .Where(t => t.CreatedAt.Date == currentDate)
                    .ToList();

                dailyData.Add(new DailyTransactionDto
                {
                    Date = currentDate,
                    Count = dayTransactions.Count,
                    TotalAmount = dayTransactions.Sum(t => t.Amount),
                    TotalCost = dayTransactions.Sum(t => t.Cost),
                    ApprovedCount = dayTransactions.Count(t => t.Status == TransactionStatus.Approved),
                    PendingCount = dayTransactions.Count(t => t.Status == TransactionStatus.Pending),
                    RejectedCount = dayTransactions.Count(t => t.Status == TransactionStatus.Rejected)
                });

                currentDate = currentDate.AddDays(1);
            }

            return dailyData;
        }

        private List<TopUserDto> CalculateTopUsers(List<CreditTransaction> transactions)
        {
            return transactions
                .GroupBy(t => new { t.UserId, t.User.Email, t.User.Name })
                .Select(g => new TopUserDto
                {
                    UserId = g.Key.UserId,
                    UserEmail = g.Key.Email,
                    UserName = g.Key.Name,
                    TransactionCount = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount),
                    TotalCost = g.Sum(t => t.Cost)
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
            csv.AppendLine($"Total Cost,{report.Summary.TotalCost:F2}");
            csv.AppendLine($"Average Transaction,{report.Summary.AverageTransactionAmount:F2}");
            csv.AppendLine($"Approved Count,{report.Summary.ApprovedCount}");
            csv.AppendLine($"Pending Count,{report.Summary.PendingCount}");
            csv.AppendLine($"Rejected Count,{report.Summary.RejectedCount}");
            csv.AppendLine($"Approved Amount,{report.Summary.ApprovedAmount:F2}");
            csv.AppendLine($"Pending Amount,{report.Summary.PendingAmount:F2}");
            csv.AppendLine($"Rejected Amount,{report.Summary.RejectedAmount:F2}");
            csv.AppendLine();

            // By Status
            csv.AppendLine("TRANSACTIONS BY STATUS");
            csv.AppendLine("Status,Count,Total Amount,Total Cost,Percentage");
            foreach (var status in report.ByStatus)
            {
                csv.AppendLine($"{status.Status},{status.Count},{status.TotalAmount:F2},{status.TotalCost:F2},{status.Percentage:F2}%");
            }
            csv.AppendLine();

            // By Payment Method
            csv.AppendLine("TRANSACTIONS BY PAYMENT METHOD");
            csv.AppendLine("Payment Method,Count,Total Amount,Average Amount");
            foreach (var method in report.ByPaymentMethod)
            {
                csv.AppendLine($"{method.PaymentMethod},{method.Count},{method.TotalAmount:F2},{method.AverageAmount:F2}");
            }
            csv.AppendLine();

            // Daily Breakdown
            csv.AppendLine("DAILY BREAKDOWN");
            csv.AppendLine("Date,Count,Total Amount,Total Cost,Approved,Pending,Rejected");
            foreach (var day in report.DailyBreakdown)
            {
                csv.AppendLine($"{day.Date:yyyy-MM-dd},{day.Count},{day.TotalAmount:F2},{day.TotalCost:F2},{day.ApprovedCount},{day.PendingCount},{day.RejectedCount}");
            }
            csv.AppendLine();

            // Top Users
            csv.AppendLine("TOP USERS");
            csv.AppendLine("Email,Name,Transaction Count,Total Amount,Total Cost");
            foreach (var user in report.TopUsers)
            {
                csv.AppendLine($"{user.UserEmail},{user.UserName ?? "N/A"},{user.TransactionCount},{user.TotalAmount:F2},{user.TotalCost:F2}");
            }

            return csv.ToString();
        }
    }
}

