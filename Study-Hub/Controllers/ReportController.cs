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
                        Amount = today.Summary.TotalAmount,
                        Approved = today.Summary.ApprovedCount,
                        Pending = today.Summary.PendingCount
                    },
                    ThisWeek = new
                    {
                        Transactions = thisWeek.Summary.TotalTransactions,
                        Amount = thisWeek.Summary.TotalAmount,
                        Approved = thisWeek.Summary.ApprovedCount,
                        Pending = thisWeek.Summary.PendingCount
                    },
                    ThisMonth = new
                    {
                        Transactions = thisMonth.Summary.TotalTransactions,
                        Amount = thisMonth.Summary.TotalAmount,
                        Approved = thisMonth.Summary.ApprovedCount,
                        Pending = thisMonth.Summary.PendingCount
                    }
                };

                return Ok(ApiResponse<object>.SuccessResponse(stats, "Quick stats retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }
    }
}
