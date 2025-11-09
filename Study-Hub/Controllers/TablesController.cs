using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Study_Hub.Models.DTOs;
using StudyHubApi.Services.Interfaces;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Study_Hub.Data;
using Study_Hub.Service.Interface;

namespace StudyHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly ITableService _tableService;
        private readonly IThermalPrinterService _printerService;
        private readonly ApplicationDbContext _context;

        public TablesController(ITableService tableService, IThermalPrinterService printerService, ApplicationDbContext context)
        {
            _tableService = tableService;
            _printerService = printerService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<StudyTableDto>>>> GetAllTables()
        {
            try
            {
                var tables = await _tableService.GetAllTablesAsync();
                return Ok(ApiResponse<List<StudyTableDto>>.SuccessResponse(tables));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<StudyTableDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("by-qr/{qrCode}")]
        public async Task<ActionResult<ApiResponse<StudyTableDto?>>> GetTableByQR(string qrCode)
        {
            try
            {
                var table = await _tableService.GetTableByQRAsync(Uri.UnescapeDataString(qrCode));
                return Ok(ApiResponse<StudyTableDto?>.SuccessResponse(table));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<StudyTableDto?>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("sessions/start")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<Guid>>> StartTableSession([FromBody] StartSessionRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var sessionId = await _tableService.StartTableSessionAsync(userId, request);
                
                // Get session details for receipt
                var session = await _context.TableSessions
                    .Include(s => s.User)
                    .Include(s => s.Table)
                    .FirstOrDefaultAsync(s => s.Id == sessionId);
                
                if (session != null)
                {
                    // Prepare receipt data
                    var receipt = new ReceiptDto
                    {
                        TransactionId = session.Id.ToString(),
                        TransactionDate = session.CreatedAt,
                        CustomerName = session.User?.Name ?? "Guest",
                        TableNumber = session.Table?.TableNumber ?? "Unknown",
                        StartTime = session.StartTime,
                        EndTime = session.EndTime ?? session.StartTime.AddHours(request.hours),
                        HourlyRate = session.Table?.HourlyRate ?? 0,
                        Hours = request.hours,
                        TotalAmount = session.Amount,
                        PaymentMethod = session.PaymentMethod ?? "Cash",
                        Cash = session.Cash,
                        Change = session.Change,
                        WifiPassword = "password1234",
                        BusinessName = "Sunny Side Up Work + Study",
                        BusinessAddress = "Your Business Address",
                        BusinessContact = "Contact: 09XX-XXX-XXXX"
                    };
                    
                    // Print receipt (async - don't wait for it to complete)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _printerService.PrintReceiptAsync(receipt);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to print receipt: {ex.Message}");
                        }
                    });
                }
                
                return Ok(ApiResponse<Guid>.SuccessResponse(sessionId, "Session started successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Guid>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("sessions/end")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<EndSessionResponseDto>>> EndTableSession([FromBody] Guid sessionId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _tableService.EndTableSessionAsync(userId, sessionId);
                return Ok(ApiResponse<EndSessionResponseDto>.SuccessResponse(result, "Session ended successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<EndSessionResponseDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("sessions/active")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<SessionWithTableDto?>>> GetUserActiveSession()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var session = await _tableService.GetUserActiveSessionAsync(userId);
                return Ok(ApiResponse<SessionWithTableDto?>.SuccessResponse(session));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SessionWithTableDto?>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("sessions/change-table")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ChangeTableResponseDto>>> ChangeTable([FromBody] ChangeTableRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _tableService.ChangeTableAsync(userId, request);
                return Ok(ApiResponse<ChangeTableResponseDto>.SuccessResponse(result, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ChangeTableResponseDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("sessions/start-subscription")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<Guid>>> StartSubscriptionSession([FromBody] StartSubscriptionSessionDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var sessionId = await _tableService.StartSubscriptionSessionAsync(userId, request);
                
                // Get session details for receipt
                var session = await _context.TableSessions
                    .Include(s => s.User)
                    .Include(s => s.Table)
                    .Include(s => s.Subscription)
                        .ThenInclude(sub => sub.Package)
                    .FirstOrDefaultAsync(s => s.Id == sessionId);
                
                if (session != null && session.Subscription != null)
                {
                    // Prepare receipt data for subscription session
                    var receipt = new ReceiptDto
                    {
                        TransactionId = session.Id.ToString(),
                        TransactionDate = session.CreatedAt,
                        CustomerName = session.User?.Name ?? "Guest",
                        TableNumber = session.Table?.TableNumber ?? "Unknown",
                        StartTime = session.StartTime,
                        EndTime = null, // Open-ended for subscription
                        HourlyRate = 0,
                        Hours = 0,
                        TotalAmount = 0,
                        PaymentMethod = $"Subscription: {session.Subscription.Package?.Name}",
                        Cash = null,
                        Change = null,
                        WifiPassword = "password1234",
                        BusinessName = "Sunny Side Up Work + Study",
                        BusinessAddress = "Your Business Address",
                        BusinessContact = "Contact: 09XX-XXX-XXXX"
                    };
                    
                    // Print receipt (async - don't wait for it to complete)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _printerService.PrintReceiptAsync(receipt);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to print receipt: {ex.Message}");
                        }
                    });
                }
                
                return Ok(ApiResponse<Guid>.SuccessResponse(sessionId, "Subscription session started successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Guid>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("sessions/end-subscription")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<EndSessionResponseDto>>> EndSubscriptionSession([FromBody] Guid sessionId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _tableService.EndSubscriptionSessionAsync(userId, sessionId);
                return Ok(ApiResponse<EndSessionResponseDto>.SuccessResponse(result, "Subscription session ended successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<EndSessionResponseDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("sessions/{sessionId}/print-receipt")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> PrintReceipt(Guid sessionId, [FromBody] PrintReceiptRequest? request = null)
        {
            try
            {
                var session = await _context.TableSessions
                    .Include(s => s.User)
                    .Include(s => s.Table)
                    .FirstOrDefaultAsync(s => s.Id == sessionId);
                
                if (session == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("Session not found"));
                }
                
                // Calculate hours
                var endTime = session.EndTime ?? DateTime.UtcNow;
                var duration = endTime - session.StartTime;
                var hours = Math.Ceiling(duration.TotalHours);
                
                // Use custom password if provided, otherwise use default
                var wifiPassword = request?.WifiPassword ?? "password1234";

                var receipt = new ReceiptDto
                {
                    TransactionId = session.Id.ToString(),
                    TransactionDate = session.CreatedAt,
                    CustomerName = session.User?.Name ?? "Guest",
                    TableNumber = session.Table?.TableNumber ?? "Unknown",
                    StartTime = session.StartTime,
                    EndTime = endTime,
                    HourlyRate = session.Table?.HourlyRate ?? 0,
                    Hours = hours,
                    TotalAmount = session.Amount,
                    PaymentMethod = session.PaymentMethod ?? "Cash",
                    Cash = session.Cash,
                    Change = session.Change,
                    WifiPassword = wifiPassword,
                    BusinessName = "Sunny Side Up Work + Study",
                    BusinessAddress = "Your Business Address",
                    BusinessContact = "Contact: 09XX-XXX-XXXX"
                };
                
                var success = await _printerService.PrintReceiptAsync(receipt);
                
                if (success)
                {
                    return Ok(ApiResponse<bool>.SuccessResponse(true, "Receipt printed successfully"));
                }
                else
                {
                    return Ok(ApiResponse<bool>.ErrorResponse("Failed to print receipt"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("sessions/{sessionId}/receipt-preview")]
        [Authorize]
        public async Task<ActionResult<byte[]>> PreviewReceipt(Guid sessionId)
        {
            try
            {
                var session = await _context.TableSessions
                    .Include(s => s.User)
                    .Include(s => s.Table)
                    .FirstOrDefaultAsync(s => s.Id == sessionId);
                
                if (session == null)
                {
                    return NotFound();
                }
                
                // Calculate hours
                var endTime = session.EndTime ?? DateTime.UtcNow;
                var duration = endTime - session.StartTime;
                var hours = Math.Ceiling(duration.TotalHours);
                
                var receipt = new ReceiptDto
                {
                    TransactionId = session.Id.ToString(),
                    TransactionDate = session.CreatedAt,
                    CustomerName = session.User?.Name ?? "Guest",
                    TableNumber = session.Table?.TableNumber ?? "Unknown",
                    StartTime = session.StartTime,
                    EndTime = endTime,
                    HourlyRate = session.Table?.HourlyRate ?? 0,
                    Hours = hours,
                    TotalAmount = session.Amount,
                    PaymentMethod = session.PaymentMethod ?? "Cash",
                    Cash = session.Cash,
                    Change = session.Change,
                    WifiPassword = "password1234",
                    BusinessName = "Sunny Side Up Work + Study",
                    BusinessAddress = "Your Business Address",
                    BusinessContact = "Contact: 09XX-XXX-XXXX"
                };
                
                var receiptData = await _printerService.GenerateReceiptAsync(receipt);
                
                return File(receiptData, "application/octet-stream", $"receipt_{sessionId}.bin");
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}