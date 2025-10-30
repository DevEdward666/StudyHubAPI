﻿﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
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
        private readonly IPromoService _promoService;
        private readonly IGlobalSettingsService _globalSettingsService;

        public AdminController(IAdminService adminService, IPromoService promoService,
            IGlobalSettingsService globalSettingsService)
        {
            _adminService = adminService;
            _promoService = promoService;
            _globalSettingsService = globalSettingsService;
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

                var transactions = await _adminService.GetAllTableTransactionsAsync();
                return Ok(ApiResponse<List<TransactionWithUserDto>>.SuccessResponse(transactions));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TransactionWithUserDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("transactions/approve")]
        public async Task<ActionResult<ApiResponse<bool>>> ApproveTransaction(
            [FromBody] ApproveTransactionRequestDto request)
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
        public async Task<ActionResult<ApiResponse<bool>>> RejectTransaction(
            [FromBody] ApproveTransactionRequestDto request)
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
        public async Task<ActionResult<ApiResponse<CreateTableResponseDto>>> CreateTable(
            [FromBody] CreateTableRequestDto request)
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
        public async Task<ActionResult<ApiResponse<UpdateTableResponseDto>>> UpdateTable(
            [FromBody] UpdateTableRequestDto request)
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
        public async Task<ActionResult<ApiResponse<SelectedTableResponseDto>>> SelectedTable(
            [FromBody] SelectedTableRequestDto request)
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
        public async Task<ActionResult<ApiResponse<AdminAddCreditsResponseDto>>> AddApprovedCredits(
            [FromBody] AdminAddCreditsRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _adminService.AddApprovedCreditsAsync(userId, request);
                return Ok(ApiResponse<AdminAddCreditsResponseDto>.SuccessResponse(result,
                    "Credits added successfully"));
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
        public async Task<ActionResult<ApiResponse<ToggleUserAdminResponseDto>>> ToggleUserAdmin(
            [FromBody] ToggleUserAdminRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _adminService.ToggleUserAdminAsync(request.UserId);
                return Ok(ApiResponse<ToggleUserAdminResponseDto>.SuccessResponse(result,
                    "Admin status toggled successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ToggleUserAdminResponseDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("users/create")]
        public async Task<ActionResult<ApiResponse<CreateUserResponseDto>>> CreateUser(
            [FromBody] CreateUserRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _adminService.CreateUserAsync(request);
                return Ok(ApiResponse<CreateUserResponseDto>.SuccessResponse(result,
                    "User created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CreateUserResponseDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("users/update")]
        public async Task<ActionResult<ApiResponse<UpdateUserResponseDto>>> UpdateUser(
            [FromBody] UpdateUserRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _adminService.UpdateUserAsync(request);
                return Ok(ApiResponse<UpdateUserResponseDto>.SuccessResponse(result,
                    "User updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UpdateUserResponseDto>.ErrorResponse(ex.Message));
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

        // PROMO MANAGEMENT ENDPOINTS

        [HttpPost("promos/create")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PromoDto>>> CreatePromo([FromBody] CreatePromoRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _promoService.CreatePromoAsync(userId, request);
                return Ok(ApiResponse<PromoDto>.SuccessResponse(result, "Promo created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PromoDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("promos/update")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PromoDto>>> UpdatePromo([FromBody] UpdatePromoRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _promoService.UpdatePromoAsync(userId, request);
                return Ok(ApiResponse<PromoDto>.SuccessResponse(result, "Promo updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PromoDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("promos/delete/{promoId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePromo(Guid promoId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _promoService.DeletePromoAsync(promoId);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Promo not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Promo deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpPatch("promos/toggle-status")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PromoDto>>> TogglePromoStatus(
            [FromBody] TogglePromoStatusRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _promoService.TogglePromoStatusAsync(request.PromoId, request.Status);
                return Ok(ApiResponse<PromoDto>.SuccessResponse(result, "Promo status updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PromoDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("promos")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<PromoDto>>>> GetAllPromos(
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _promoService.GetAllPromosAsync(includeInactive);
                return Ok(ApiResponse<List<PromoDto>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<PromoDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("promos/{promoId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PromoDto>>> GetPromoById(Guid promoId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _promoService.GetPromoByIdAsync(promoId);
                if (result == null)
                    return NotFound(ApiResponse<PromoDto>.ErrorResponse("Promo not found"));

                return Ok(ApiResponse<PromoDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PromoDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("promos/code/{code}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PromoDto>>> GetPromoByCode(string code)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _promoService.GetPromoByCodeAsync(code);
                if (result == null)
                    return NotFound(ApiResponse<PromoDto>.ErrorResponse("Promo not found"));

                return Ok(ApiResponse<PromoDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PromoDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("promos/{promoId}/usage-history")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<PromoUsageDto>>>> GetPromoUsageHistory(Guid promoId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _promoService.GetPromoUsageHistoryAsync(promoId);
                return Ok(ApiResponse<List<PromoUsageDto>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<PromoUsageDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("promos/{promoId}/statistics")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PromoStatisticsDto>>> GetPromoStatistics(Guid promoId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _promoService.GetPromoStatisticsAsync(promoId);
                return Ok(ApiResponse<PromoStatisticsDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PromoStatisticsDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("promos/statistics/all")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<PromoStatisticsDto>>>> GetAllPromoStatistics()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _promoService.GetAllPromoStatisticsAsync();
                return Ok(ApiResponse<List<PromoStatisticsDto>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<PromoStatisticsDto>>.ErrorResponse(ex.Message));
            }
        }

        // GLOBAL SETTINGS ENDPOINTS

        [HttpGet("settings")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<GlobalSettingDto>>>> GetAllSettings()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _globalSettingsService.GetAllSettingsAsync();
                return Ok(ApiResponse<List<GlobalSettingDto>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<GlobalSettingDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("settings/{settingId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<GlobalSettingDto>>> GetSettingById(Guid settingId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _globalSettingsService.GetSettingByIdAsync(settingId);
                if (result == null)
                    return NotFound(ApiResponse<GlobalSettingDto>.ErrorResponse("Setting not found"));

                return Ok(ApiResponse<GlobalSettingDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<GlobalSettingDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("settings/key/{key}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<GlobalSettingDto>>> GetSettingByKey(string key)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _globalSettingsService.GetSettingByKeyAsync(key);
                if (result == null)
                    return NotFound(ApiResponse<GlobalSettingDto>.ErrorResponse("Setting not found"));

                return Ok(ApiResponse<GlobalSettingDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<GlobalSettingDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("settings/category/{category}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<GlobalSettingDto>>>> GetSettingsByCategory(string category)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _globalSettingsService.GetSettingsByCategoryAsync(category);
                return Ok(ApiResponse<List<GlobalSettingDto>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<GlobalSettingDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("settings/create")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<GlobalSettingDto>>> CreateSetting(
            [FromBody] CreateGlobalSettingRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _globalSettingsService.CreateSettingAsync(userId, request);
                return Ok(ApiResponse<GlobalSettingDto>.SuccessResponse(result, "Setting created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<GlobalSettingDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("settings/update")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<GlobalSettingDto>>> UpdateSetting(
            [FromBody] UpdateGlobalSettingRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _globalSettingsService.UpdateSettingAsync(userId, request);
                return Ok(ApiResponse<GlobalSettingDto>.SuccessResponse(result, "Setting updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<GlobalSettingDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("settings/bulk-update")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<Dictionary<string, GlobalSettingDto>>>> BulkUpdateSettings(
            [FromBody] BulkUpdateSettingsRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _globalSettingsService.BulkUpdateSettingsAsync(userId, request);
                return Ok(ApiResponse<Dictionary<string, GlobalSettingDto>>.SuccessResponse(result,
                    "Settings updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, GlobalSettingDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("settings/delete/{settingId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteSetting(Guid settingId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _globalSettingsService.DeleteSettingAsync(settingId);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Setting not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Setting deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("settings/{settingId}/history")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<GlobalSettingHistoryDto>>>> GetSettingHistory(Guid settingId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _globalSettingsService.GetSettingHistoryAsync(settingId);
                return Ok(ApiResponse<List<GlobalSettingHistoryDto>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<GlobalSettingHistoryDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("settings/changes/recent")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<GlobalSettingHistoryDto>>>> GetRecentChanges(
            [FromQuery] int count = 50)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _globalSettingsService.GetRecentChangesAsync(count);
                return Ok(ApiResponse<List<GlobalSettingHistoryDto>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<GlobalSettingHistoryDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("settings/validate")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ValidateSettingResponseDto>>> ValidateSetting(
            [FromBody] ValidateSettingRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _globalSettingsService.ValidateSettingAsync(request);
                return Ok(ApiResponse<ValidateSettingResponseDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ValidateSettingResponseDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("settings/initialize-defaults")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> InitializeDefaultSettings(
            [FromBody] InitializeDefaultSettingsRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result =
                    await _globalSettingsService.InitializeDefaultSettingsAsync(userId, request.OverwriteExisting);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Default settings initialized successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("settings/export")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ExportSettingsResponseDto>>> ExportSettings(
            [FromQuery] string? category = null)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _globalSettingsService.ExportSettingsAsync(category);
                return Ok(ApiResponse<ExportSettingsResponseDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExportSettingsResponseDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("settings/import")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<Dictionary<string, GlobalSettingDto>>>> ImportSettings(
            [FromBody] ImportSettingsRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _globalSettingsService.ImportSettingsAsync(userId, request);
                return Ok(ApiResponse<Dictionary<string, GlobalSettingDto>>.SuccessResponse(result,
                    "Settings imported successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, GlobalSettingDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("settings/summary")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<SettingsSummaryDto>>> GetSettingsSummary()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (!await _adminService.IsAdminAsync(userId))
                    return Forbid();

                var result = await _globalSettingsService.GetSettingsSummaryAsync();
                return Ok(ApiResponse<SettingsSummaryDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SettingsSummaryDto>.ErrorResponse(ex.Message));
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

