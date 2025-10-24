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
    }
}

