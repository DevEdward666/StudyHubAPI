using Study_Hub.Models.Entities;

namespace Study_Hub.Models.DTOs
{
    public enum ReportPeriod
    {
        Daily,
        Weekly,
        Monthly
    }

    public class TransactionReportDto
    {
        public ReportPeriod Period { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TransactionSummaryDto Summary { get; set; }
        public List<TransactionByStatusDto> ByStatus { get; set; }
        public List<TransactionByPaymentMethodDto> ByPaymentMethod { get; set; }
        public List<DailyTransactionDto> DailyBreakdown { get; set; }
        public List<TopUserDto> TopUsers { get; set; }
    }

    public class TransactionSummaryDto
    {
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AverageTransactionAmount { get; set; }
        public int ApprovedCount { get; set; }
        public int PendingCount { get; set; }
        public int RejectedCount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal RejectedAmount { get; set; }
    }

    public class TransactionByStatusDto
    {
        public TransactionStatus Status { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Percentage { get; set; }
    }

    public class TransactionByPaymentMethodDto
    {
        public string PaymentMethod { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
    }

    public class DailyTransactionDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalCost { get; set; }
        public int ApprovedCount { get; set; }
        public int PendingCount { get; set; }
        public int RejectedCount { get; set; }
    }

    public class TopUserDto
    {
        public Guid UserId { get; set; }
        public string UserEmail { get; set; }
        public string? UserName { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class GetReportRequestDto
    {
        public ReportPeriod Period { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class TransactionReportResponseDto
    {
        public TransactionReportDto Report { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class ExportReportRequestDto
    {
        public ReportPeriod Period { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Format { get; set; } = "json"; // json, csv
    }
}

