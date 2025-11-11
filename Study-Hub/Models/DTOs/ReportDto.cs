using Study_Hub.Models.Entities;
using System.ComponentModel.DataAnnotations;

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
        public List<TopUserDto> TopUsers { get; set; }
    }

    public class TransactionSummaryDto
    {
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
    }

    public class TopUserDto
    {
        public Guid UserId { get; set; }
        public string UserEmail { get; set; }
        public string? UserName { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class GetReportRequestDto
    {
        [Required]
        public ReportPeriod Period { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class TransactionReportResponseDto
    {
        [Required]
        public TransactionReportDto Report { get; set; }
        [Required]
        public DateTime GeneratedAt { get; set; }
    }

    public class ExportReportRequestDto
    {
        [Required]
        public ReportPeriod Period { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Required]
        [RegularExpression("^(json|csv)$", ErrorMessage = "Format must be 'json' or 'csv'")]
        public string Format { get; set; } = "json"; // json, csv
    }
}
