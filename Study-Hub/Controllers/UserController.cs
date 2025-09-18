using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Study_Hub.Models.DTOs;
using StudyHubApi.Services.Interfaces;
using System.Security.Claims;

namespace StudyHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("credits")]
        public async Task<ActionResult<ApiResponse<UserCreditsDto?>>> GetUserCredits()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var credits = await _userService.GetUserCreditsAsync(userId);
                return Ok(ApiResponse<UserCreditsDto?>.SuccessResponse(credits));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserCreditsDto?>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("credits/initialize")]
        public async Task<ActionResult<ApiResponse<UserCreditsDto>>> InitializeUserCredits()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var credits = await _userService.InitializeUserCreditsAsync(userId);
                return Ok(ApiResponse<UserCreditsDto>.SuccessResponse(credits, "Credits initialized successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserCreditsDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("credits/purchase")]
        public async Task<ActionResult<ApiResponse<Guid>>> PurchaseCredits([FromBody] PurchaseCreditsRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var transactionId = await _userService.PurchaseCreditsAsync(userId, request);
                return Ok(ApiResponse<Guid>.SuccessResponse(transactionId, "Credit purchase request submitted"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Guid>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<ApiResponse<List<CreditTransactionDto>>>> GetUserTransactions()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var transactions = await _userService.GetUserTransactionsAsync(userId);
                return Ok(ApiResponse<List<CreditTransactionDto>>.SuccessResponse(transactions));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<CreditTransactionDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("sessions")]
        public async Task<ActionResult<ApiResponse<List<SessionWithTableDto>>>> GetUserSessions()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var sessions = await _userService.GetUserSessionsAsync(userId);
                return Ok(ApiResponse<List<SessionWithTableDto>>.SuccessResponse(sessions));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<SessionWithTableDto>>.ErrorResponse(ex.Message));
            }
        }
    }
}