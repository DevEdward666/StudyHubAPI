using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Study_Hub.Models.DTOs;
using StudyHubApi.Services.Interfaces;
using System.Security.Claims;

namespace StudyHubApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        #region Package Endpoints

        [HttpGet("packages")]
        public async Task<ActionResult<ApiResponse<List<SubscriptionPackageDto>>>> GetPackages([FromQuery] bool activeOnly = false)
        {
            try
            {
                var packages = activeOnly
                    ? await _subscriptionService.GetActivePackagesAsync()
                    : await _subscriptionService.GetAllPackagesAsync();

                return Ok(ApiResponse<List<SubscriptionPackageDto>>.SuccessResponse(packages));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<SubscriptionPackageDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("packages/{packageId}")]
        public async Task<ActionResult<ApiResponse<SubscriptionPackageDto>>> GetPackage(Guid packageId)
        {
            try
            {
                var package = await _subscriptionService.GetPackageByIdAsync(packageId);
                if (package == null)
                    return NotFound(ApiResponse<SubscriptionPackageDto>.ErrorResponse("Package not found"));

                return Ok(ApiResponse<SubscriptionPackageDto>.SuccessResponse(package));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SubscriptionPackageDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("packages")]
        // //[Authorize(Roles = "admin,superadmin")]
        public async Task<ActionResult<ApiResponse<SubscriptionPackageDto>>> CreatePackage([FromBody] CreateSubscriptionPackageDto dto)
        {
            try
            {
                var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var package = await _subscriptionService.CreatePackageAsync(dto, adminId);
                return Ok(ApiResponse<SubscriptionPackageDto>.SuccessResponse(package, "Package created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SubscriptionPackageDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("packages/{packageId}")]
        //[Authorize(Roles = "admin,superadmin")]
        public async Task<ActionResult<ApiResponse<SubscriptionPackageDto>>> UpdatePackage(Guid packageId, [FromBody] UpdateSubscriptionPackageDto dto)
        {
            try
            {
                var package = await _subscriptionService.UpdatePackageAsync(packageId, dto);
                return Ok(ApiResponse<SubscriptionPackageDto>.SuccessResponse(package, "Package updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SubscriptionPackageDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("packages/{packageId}")]
        //[Authorize(Roles = "admin,superadmin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePackage(Guid packageId)
        {
            try
            {
                var result = await _subscriptionService.DeletePackageAsync(packageId);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Package deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        #endregion

        #region User Subscription Endpoints

        [HttpGet("my-subscriptions")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<UserSubscriptionDto>>>> GetMySubscriptions([FromQuery] bool activeOnly = false)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var subscriptions = activeOnly
                    ? await _subscriptionService.GetActiveUserSubscriptionsAsync(userId)
                    : await _subscriptionService.GetUserSubscriptionsAsync(userId);

                return Ok(ApiResponse<List<UserSubscriptionDto>>.SuccessResponse(subscriptions));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<UserSubscriptionDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("{subscriptionId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserSubscriptionDto>>> GetSubscription(Guid subscriptionId)
        {
            try
            {
                var subscription = await _subscriptionService.GetSubscriptionByIdAsync(subscriptionId);
                if (subscription == null)
                    return NotFound(ApiResponse<UserSubscriptionDto>.ErrorResponse("Subscription not found"));

                return Ok(ApiResponse<UserSubscriptionDto>.SuccessResponse(subscription));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserSubscriptionDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("purchase")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserSubscriptionDto>>> PurchaseSubscription([FromBody] PurchaseSubscriptionDto dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var subscription = await _subscriptionService.PurchaseSubscriptionAsync(userId, dto);
                return Ok(ApiResponse<UserSubscriptionDto>.SuccessResponse(subscription, "Subscription purchased successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserSubscriptionDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("{subscriptionId}/cancel")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> CancelSubscription(Guid subscriptionId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _subscriptionService.CancelSubscriptionAsync(subscriptionId, userId);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Subscription cancelled successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("{subscriptionId}/usage")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<SubscriptionUsageDto>>> GetSubscriptionUsage(Guid subscriptionId)
        {
            try
            {
                var usage = await _subscriptionService.GetSubscriptionUsageAsync(subscriptionId);
                return Ok(ApiResponse<SubscriptionUsageDto>.SuccessResponse(usage));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SubscriptionUsageDto>.ErrorResponse(ex.Message));
            }
        }

        #endregion

        #region Admin Endpoints

        [HttpGet("admin/all")]
        //[Authorize(Roles = "admin,superadmin")]
        public async Task<ActionResult<ApiResponse<List<UserSubscriptionWithUserDto>>>> GetAllUserSubscriptions()
        {
            try
            {
                var subscriptions = await _subscriptionService.GetAllUserSubscriptionsAsync();
                return Ok(ApiResponse<List<UserSubscriptionWithUserDto>>.SuccessResponse(subscriptions));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<UserSubscriptionWithUserDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("admin/status/{status}")]
        //[Authorize(Roles = "admin,superadmin")]
        public async Task<ActionResult<ApiResponse<List<UserSubscriptionWithUserDto>>>> GetSubscriptionsByStatus(string status)
        {
            try
            {
                var subscriptions = await _subscriptionService.GetSubscriptionsByStatusAsync(status);
                return Ok(ApiResponse<List<UserSubscriptionWithUserDto>>.SuccessResponse(subscriptions));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<UserSubscriptionWithUserDto>>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("admin/purchase")]
        //[Authorize(Roles = "admin,superadmin")]
        public async Task<ActionResult<ApiResponse<UserSubscriptionDto>>> AdminPurchaseSubscription([FromBody] AdminPurchaseSubscriptionDto dto)
        {
            try
            {
                var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var subscription = await _subscriptionService.AdminPurchaseSubscriptionAsync(dto, adminId);
                return Ok(ApiResponse<UserSubscriptionDto>.SuccessResponse(subscription, "Subscription purchased successfully for user"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserSubscriptionDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("admin/user/{userId}")]
        //[Authorize(Roles = "admin,superadmin")]
        public async Task<ActionResult<ApiResponse<List<UserSubscriptionDto>>>> GetUserSubscriptionsAdmin(Guid userId)
        {
            try
            {
                var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(userId);
                return Ok(ApiResponse<List<UserSubscriptionDto>>.SuccessResponse(subscriptions));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<UserSubscriptionDto>>.ErrorResponse(ex.Message));
            }
        }

        #endregion
    }
}

