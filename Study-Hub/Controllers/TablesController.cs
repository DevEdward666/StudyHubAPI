using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Study_Hub.Models.DTOs;
using StudyHubApi.Services.Interfaces;
using System.Security.Claims;

namespace StudyHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly ITableService _tableService;

        public TablesController(ITableService tableService)
        {
            _tableService = tableService;
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
    }
}