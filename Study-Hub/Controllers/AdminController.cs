﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Study_Hub.Models.DTOs;
using Study_Hub.Services.Interfaces;
using StudyHubApi.Services.Interfaces;
using System.Security.Claims;

namespace StudyHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("is-admin")]
        public async Task<ActionResult<ApiResponse<bool>>> IsAdmin()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var isAdmin = await _adminService.IsAdminAsync(userId);
                return Ok(ApiResponse<bool>.SuccessResponse(isAdmin));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<List<UserWithInfoDto>>>> GetAllUsers()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var users = await _adminService.GetAllUsersAsync();
                return Ok(ApiResponse<List<UserWithInfoDto>>.SuccessResponse(users));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<UserWithInfoDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("transactions/pending")]
        public async Task<ActionResult<ApiResponse<List<TransactionWithUserDto>>>> GetPendingTransactions()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var transactions = await _adminService.GetPendingTransactionsAsync();
                return Ok(ApiResponse<List<TransactionWithUserDto>>.SuccessResponse(transactions));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TransactionWithUserDto>>.ErrorResponse(ex.Message));
            }
        }
        [HttpGet("transactions/all")]
        public async Task<ActionResult<ApiResponse<List<TransactionWithUserDto>>>> GetAllTransactions()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var transactions = await _adminService.GetPendingTransactionsAsync();
                return Ok(ApiResponse<List<TransactionWithUserDto>>.SuccessResponse(transactions));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TransactionWithUserDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("transactions/approve")]
        public async Task<ActionResult<ApiResponse<bool>>> ApproveTransaction([FromBody] ApproveTransactionRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _adminService.ApproveTransactionAsync(request.TransactionId, userId);
                if (!result)
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to approve transaction"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Transaction approved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("transactions/reject")]
        public async Task<ActionResult<ApiResponse<bool>>> RejectTransaction([FromBody] ApproveTransactionRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _adminService.RejectTransactionAsync(request.TransactionId, userId);
                if (!result)
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to reject transaction"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Transaction rejected successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("tables/create")]
        public async Task<ActionResult<ApiResponse<CreateTableResponseDto>>> CreateTable([FromBody] CreateTableRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _adminService.CreateStudyTableAsync(request);
                return Ok(ApiResponse<CreateTableResponseDto>.SuccessResponse(result, "Table created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CreateTableResponseDto>.ErrorResponse(ex.Message));
            }
        }
        [HttpPut("tables/update")]
        public async Task<ActionResult<ApiResponse<UpdateTableResponseDto>>> UpdateTable([FromBody] UpdateTableRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _adminService.UpdateStudyTableAsync(request);
                return Ok(ApiResponse<UpdateTableResponseDto>.SuccessResponse(result, "Table updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UpdateTableResponseDto>.ErrorResponse(ex.Message));
            }
        }
        [HttpPost("tables/selected")]
        public async Task<ActionResult<ApiResponse<SelectedTableResponseDto>>> SelectedTable([FromBody] SelectedTableRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _adminService.SelectedStudyTableAsync(request);
                return Ok(ApiResponse<SelectedTableResponseDto>.SuccessResponse(result, "Selected Table"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SelectedTableResponseDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("credits/add-approved")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<AdminAddCreditsResponseDto>>> AddApprovedCredits([FromBody] AdminAddCreditsRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _adminService.AddApprovedCreditsAsync(userId, request);
                return Ok(ApiResponse<AdminAddCreditsResponseDto>.SuccessResponse(result, "Credits added successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<AdminAddCreditsResponseDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("users/make-admin")]
        public async Task<ActionResult<ApiResponse<bool>>> MakeUserAdmin([FromBody] MakeUserAdminRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _adminService.MakeUserAdminAsync(request.UserEmail);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "User promoted to admin successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("users/toggle-admin")]
        public async Task<ActionResult<ApiResponse<ToggleUserAdminResponseDto>>> ToggleUserAdmin([FromBody] ToggleUserAdminRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _adminService.ToggleUserAdminAsync(request.UserId);
                return Ok(ApiResponse<ToggleUserAdminResponseDto>.SuccessResponse(result, "Admin status toggled successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ToggleUserAdminResponseDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("tables/generate-qr")]
        public async Task<ActionResult<ApiResponse<QRGenerationResponseDto>>> GenerateTableQR([FromBody] Guid tableId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _adminService.GenerateTableQRAsync(tableId);
                return Ok(ApiResponse<QRGenerationResponseDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<QRGenerationResponseDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("setup-data")]
        public async Task<ActionResult<ApiResponse<string>>> SetupData()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _adminService.SetupDataAsync();
                return Ok(ApiResponse<string>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }
}