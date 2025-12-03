using Study_Hub.Models.DTOs;

namespace Study_Hub.Services.Interfaces
{
    public interface IReportService
    {
        Task<TransactionReportDto> GetTransactionReportAsync(ReportPeriod period, DateTime? startDate = null, DateTime? endDate = null);
        Task<TransactionReportDto> GetDailyReportAsync(DateTime? date = null);
        Task<TransactionReportDto> GetWeeklyReportAsync(DateTime? weekStartDate = null);
        Task<TransactionReportDto> GetMonthlyReportAsync(int? year = null, int? month = null);
        Task<string> ExportReportToCsvAsync(ReportPeriod period, DateTime? startDate = null, DateTime? endDate = null);
        Task<object> GetQuickStatsAsync();
        Task<object> GetDailySalesReportAsync(DateTime date);
        Task<object> GetSalesReportAsync(ReportPeriod period, DateTime startDate, DateTime endDate);
        Task<string> ExportDailySalesReportToCsvAsync(DateTime date);
        Task<string> ExportSalesReportToCsvAsync(ReportPeriod period, DateTime startDate, DateTime endDate);
        Task<byte[]> ExportDailySalesReportToPdfAsync(DateTime date);
        Task<byte[]> ExportSalesReportToPdfAsync(ReportPeriod period, DateTime startDate, DateTime endDate);
    }
}

