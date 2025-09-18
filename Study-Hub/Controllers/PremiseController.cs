using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Study_Hub.Models.DTOs;
using Study_Hub.Services.Interfaces;
using StudyHubApi.Services.Interfaces;
using System.Security.Claims;

namespace StudyHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PremiseController : ControllerBase
    {
        private readonly IPremiseService _premiseService;
        private readonly IAdminService _adminService;

        public PremiseController(IPremiseService premiseService, IAdminService adminService)
        {
            _premiseService = premiseService;
            _adminService = adminService;
        }

        [HttpGet("qr-codes")]
        public async Task<ActionResult<ApiResponse<List<PremiseQrCodeDto>>>> GetPremiseQRCodes()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var codes = await _premiseService.GetPremiseQRCodesAsync();
                return Ok(ApiResponse<List<PremiseQrCodeDto>>.SuccessResponse(codes));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<PremiseQrCodeDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("create-qr")]
        public async Task<ActionResult<ApiResponse<CreatePremiseResponseDto>>> CreatePremiseQRCode([FromBody] CreatePremiseQRRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _premiseService.CreatePremiseQRCodeAsync(request);
                return Ok(ApiResponse<CreatePremiseResponseDto>.SuccessResponse(result, "Premise QR code created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CreatePremiseResponseDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("activate")]
        public async Task<ActionResult<ApiResponse<PremiseAccessDto>>> ActivatePremiseAccess([FromBody] ActivatePremiseRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _premiseService.ActivatePremiseAccessAsync(userId, request);
                return Ok(ApiResponse<PremiseAccessDto>.SuccessResponse(result, "Premise access activated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PremiseAccessDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("access")]
        public async Task<ActionResult<ApiResponse<ActivePremiseAccessDto?>>> CheckPremiseAccess()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var access = await _premiseService.CheckPremiseAccessAsync(userId);
                return Ok(ApiResponse<ActivePremiseAccessDto?>.SuccessResponse(access));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ActivePremiseAccessDto?>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("cleanup")]
        public async Task<ActionResult<ApiResponse>> CleanupExpiredAccess()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _premiseService.CleanupExpiredAccessAsync(userId);
                return Ok(ApiResponse.SuccessResponse("Expired access cleaned up"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
        }
    }
}