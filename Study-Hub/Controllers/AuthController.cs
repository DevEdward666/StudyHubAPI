﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Study_Hub.Models.DTOs;
using Study_Hub.Services.Interfaces;
using System.Security.Claims;

namespace StudyHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signin")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> SignIn([FromBody] LoginRequestDto request)
        {
            try
            {
                var result = await _authService.SignInAsync(request);
                return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Signed in successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<AuthResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("signup")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> SignUp([FromBody] RegisterRequestDto request)
        {
            try
            {
                var result = await _authService.SignUpAsync(request);
                return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Account created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var user = await _authService.GetCurrentUserAsync(userId);

                if (user == null)
                    return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));

                return Ok(ApiResponse<UserDto>.SuccessResponse(user));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("signout")]
        [Authorize]
        public new ActionResult<ApiResponse> SignOut()
        {
            // In a stateless JWT system, signout is handled client-side
            return Ok(ApiResponse.SuccessResponse("Signed out successfully"));
        }
    }
}