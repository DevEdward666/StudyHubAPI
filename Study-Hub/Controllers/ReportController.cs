using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Study_Hub.Models.DTOs;
using Study_Hub.Services.Interfaces;
using System.Security.Claims;

namespace Study_Hub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IAdminService _adminService;

        public ReportController(IReportService reportService, IAdminService adminService)
        {
            _reportService = reportService;
            _adminService = adminService;
        }

        /// <summary>
        /// Get transaction report by period
        /// </summary>
        [HttpPost("transactions")]
        public async Task<ActionResult<ApiResponse<TransactionReportResponseDto>>> GetTransactionReport([FromBody] GetReportRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<TransactionReportResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var report = await _reportService.GetTransactionReportAsync(
                    request.Period,
                    request.StartDate,
                    request.EndDate
                );

                var response = new TransactionReportResponseDto
                {
                    Report = report,
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(ApiResponse<TransactionReportResponseDto>.SuccessResponse(response, "Report generated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TransactionReportResponseDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get daily transaction report
        /// </summary>
        [HttpGet("transactions/daily")]
        public async Task<ActionResult<ApiResponse<TransactionReportResponseDto>>> GetDailyReport([FromQuery] DateTime? date = null)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var report = await _reportService.GetDailyReportAsync(date);

                var response = new TransactionReportResponseDto
                {
                    Report = report,
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(ApiResponse<TransactionReportResponseDto>.SuccessResponse(response, "Daily report generated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TransactionReportResponseDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get weekly transaction report
        /// </summary>
        [HttpGet("transactions/weekly")]
        public async Task<ActionResult<ApiResponse<TransactionReportResponseDto>>> GetWeeklyReport([FromQuery] DateTime? weekStartDate = null)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var report = await _reportService.GetWeeklyReportAsync(weekStartDate);

                var response = new TransactionReportResponseDto
                {
                    Report = report,
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(ApiResponse<TransactionReportResponseDto>.SuccessResponse(response, "Weekly report generated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TransactionReportResponseDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get monthly transaction report
        /// </summary>
        [HttpGet("transactions/monthly")]
        public async Task<ActionResult<ApiResponse<TransactionReportResponseDto>>> GetMonthlyReport([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var report = await _reportService.GetMonthlyReportAsync(year, month);

                var response = new TransactionReportResponseDto
                {
                    Report = report,
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(ApiResponse<TransactionReportResponseDto>.SuccessResponse(response, "Monthly report generated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TransactionReportResponseDto>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Export transaction report to CSV
        /// </summary>
        [HttpPost("transactions/export")]
        public async Task<IActionResult> ExportReport([FromBody] ExportReportRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                if (request.Format.ToLower() == "csv")
                {
                    var csv = await _reportService.ExportReportToCsvAsync(
                        request.Period,
                        request.StartDate,
                        request.EndDate
                    );

                    var fileName = $"transaction_report_{request.Period}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
                    var bytes = System.Text.Encoding.UTF8.GetBytes(csv);

                    return File(bytes, "text/csv", fileName);
                }
                else if (request.Format.ToLower() == "json")
                {
                    var report = await _reportService.GetTransactionReportAsync(
                        request.Period,
                        request.StartDate,
                        request.EndDate
                    );

                    return new JsonResult(report)
                    {
                        ContentType = "application/json",
                        StatusCode = 200
                    };
                }
                else
                {
                    return BadRequest(ApiResponse<string>.ErrorResponse("Invalid export format. Use 'csv' or 'json'."));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get quick stats for dashboard
        /// </summary>
        [HttpGet("transactions/quick-stats")]
        public async Task<ActionResult<ApiResponse<object>>> GetQuickStats()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var today = await _reportService.GetDailyReportAsync();
                var thisWeek = await _reportService.GetWeeklyReportAsync();
                var thisMonth = await _reportService.GetMonthlyReportAsync();

                var stats = new
                {
                    Today = new
                    {
                        Transactions = today.Summary.TotalTransactions,
                        Amount = today.Summary.TotalAmount
                    },
                    ThisWeek = new
                    {
                        Transactions = thisWeek.Summary.TotalTransactions,
                        Amount = thisWeek.Summary.TotalAmount
                    },
                    ThisMonth = new
                    {
                        Transactions = thisMonth.Summary.TotalTransactions,
                        Amount = thisMonth.Summary.TotalAmount
                    }
                };

                return Ok(ApiResponse<object>.SuccessResponse(stats, "Quick stats retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get formal daily sales report
        /// </summary>
        [HttpGet("sales/daily")]
        public async Task<ActionResult<ApiResponse<object>>> GetDailySalesReport([FromQuery] DateTime? date = null)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var targetDate = date ?? DateTime.UtcNow.Date;
                var salesReport = await _reportService.GetDailySalesReportAsync(targetDate);

                return Ok(ApiResponse<object>.SuccessResponse(salesReport, "Daily sales report generated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Export daily sales report to CSV
        /// </summary>
        [HttpPost("sales/export")]
        public async Task<IActionResult> ExportSalesReport([FromBody] ExportSalesReportRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var csv = await _reportService.ExportDailySalesReportToCsvAsync(request.Date);
                var fileName = $"daily_sales_report_{request.Date:yyyyMMdd}.csv";
                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExportSalesReport: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return BadRequest(ApiResponse<string>.ErrorResponse($"Export failed: {ex.Message}"));
            }
        }

        /// <summary>
        /// Export daily sales report to PDF (HTML for print)
        /// </summary>
        [HttpPost("sales/export-pdf")]
        public async Task<IActionResult> ExportSalesReportPdf([FromBody] ExportSalesReportRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var htmlBytes = await _reportService.ExportDailySalesReportToPdfAsync(request.Date);
                var fileName = $"daily_sales_report_{request.Date:yyyyMMdd}.html";

                return File(htmlBytes, "text/html", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExportSalesReportPdf: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return BadRequest(ApiResponse<string>.ErrorResponse($"Export failed: {ex.Message}"));
            }
        }

        /// <summary>
        /// Export sales report to CSV for any period
        /// </summary>
        [HttpPost("sales/export-period")]
        public async Task<IActionResult> ExportSalesReportPeriod([FromBody] ExportReportRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var csv = await _reportService.ExportSalesReportToCsvAsync(request.Period, request.StartDate.Value, request.EndDate.Value);
                var fileName = $"sales_report_{request.Period}_{request.StartDate:yyyyMMdd}_to_{request.EndDate:yyyyMMdd}.csv";
                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExportSalesReportPeriod: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return BadRequest(ApiResponse<string>.ErrorResponse($"Export failed: {ex.Message}"));
            }
        }

        /// <summary>
        /// Export sales report to PDF (HTML) for any period
        /// </summary>
        [HttpPost("sales/export-period-pdf")]
        public async Task<IActionResult> ExportSalesReportPeriodPdf([FromBody] ExportReportRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var htmlBytes = await _reportService.ExportSalesReportToPdfAsync(request.Period, request.StartDate.Value, request.EndDate.Value);
                var fileName = $"sales_report_{request.Period}_{request.StartDate:yyyyMMdd}_to_{request.EndDate:yyyyMMdd}.html";

                return File(htmlBytes, "text/html", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExportSalesReportPeriodPdf: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return BadRequest(ApiResponse<string>.ErrorResponse($"Export failed: {ex.Message}"));
            }
        }
    }
}
