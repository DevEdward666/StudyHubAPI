using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Study_Hub.Models.DTOs;
using Study_Hub.Services.Interfaces;
using System.Security.Claims;

namespace Study_Hub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        private readonly IRateService _rateService;
        private readonly IAdminService _adminService;

        public RatesController(IRateService rateService, IAdminService adminService)
        {
            _rateService = rateService;
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<RateDto>>>> GetAllRates()
        {
            try
            {
                var rates = await _rateService.GetAllRatesAsync();
                return Ok(ApiResponse<List<RateDto>>.SuccessResponse(rates));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<RateDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<List<RateDto>>>> GetActiveRates()
        {
            try
            {
                var rates = await _rateService.GetActiveRatesAsync();
                return Ok(ApiResponse<List<RateDto>>.SuccessResponse(rates));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<RateDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RateDto>>> GetRateById(Guid id)
        {
            try
            {
                var rate = await _rateService.GetRateByIdAsync(id);
                if (rate == null)
                    return NotFound(ApiResponse<RateDto>.ErrorResponse("Rate not found"));

                return Ok(ApiResponse<RateDto>.SuccessResponse(rate));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<RateDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("hours/{hours}")]
        public async Task<ActionResult<ApiResponse<RateDto>>> GetRateByHours(int hours)
        {
            try
            {
                var rate = await _rateService.GetRateByHoursAsync(hours);
                if (rate == null)
                    return NotFound(ApiResponse<RateDto>.ErrorResponse($"Rate for {hours} hours not found"));

                return Ok(ApiResponse<RateDto>.SuccessResponse(rate));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<RateDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RateDto>>> CreateRate([FromBody] CreateRateRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var rate = await _rateService.CreateRateAsync(userId, request);
                return Ok(ApiResponse<RateDto>.SuccessResponse(rate, "Rate created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<RateDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RateDto>>> UpdateRate([FromBody] UpdateRateRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var rate = await _rateService.UpdateRateAsync(userId, request);
                return Ok(ApiResponse<RateDto>.SuccessResponse(rate, "Rate updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<RateDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteRate(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _rateService.DeleteRateAsync(id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Rate not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Rate deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }
    }
}

