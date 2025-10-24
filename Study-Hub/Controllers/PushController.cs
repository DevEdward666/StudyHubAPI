using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Study_Hub.Models.DTOs;
using Study_Hub.Service.Interface;
using System.Security.Claims;

namespace Study_Hub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PushController : ControllerBase
    {
        private readonly IPushNotificationService _pushService;
        private readonly ILogger<PushController> _logger;

        public PushController(
            IPushNotificationService pushService,
            ILogger<PushController> logger)
        {
            _pushService = pushService;
            _logger = logger;
        }

        /// <summary>
        /// Get VAPID public key for push subscription
        /// </summary>
        [HttpGet("vapid-public-key")]
        [AllowAnonymous]
        public ActionResult<VapidPublicKeyDto> GetVapidPublicKey()
        {
            try
            {
                var publicKey = _pushService.GetVapidPublicKey();
                return Ok(new VapidPublicKeyDto { PublicKey = publicKey });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting VAPID public key");
                return StatusCode(500, new { message = "Error getting VAPID public key" });
            }
        }

        /// <summary>
        /// Subscribe to push notifications
        /// </summary>
        [HttpPost("subscribe")]
        public async Task<ActionResult<ApiResponse<PushSubscriptionResponseDto>>> Subscribe(
            [FromBody] PushSubscriptionDto subscription)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? throw new UnauthorizedAccessException("User ID not found"));

                var result = await _pushService.SubscribeAsync(userId, subscription);

                return Ok(ApiResponse<PushSubscriptionResponseDto>.SuccessResponse(result, "Successfully subscribed to push notifications"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to push notifications");
                return StatusCode(500, ApiResponse<PushSubscriptionResponseDto>.ErrorResponse("Error subscribing to push notifications"));
            }
        }

        /// <summary>
        /// Unsubscribe from push notifications
        /// </summary>
        [HttpPost("unsubscribe")]
        public async Task<ActionResult<ApiResponse<bool>>> Unsubscribe(
            [FromBody] string endpoint)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? throw new UnauthorizedAccessException("User ID not found"));

                var result = await _pushService.UnsubscribeAsync(userId, endpoint);

                return Ok(ApiResponse<bool>.SuccessResponse(result, result ? "Successfully unsubscribed" : "Subscription not found"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing from push notifications");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error unsubscribing from push notifications"));
            }
        }

        /// <summary>
        /// Get user's push subscriptions
        /// </summary>
        [HttpGet("subscriptions")]
        public async Task<ActionResult<ApiResponse<List<PushSubscriptionResponseDto>>>> GetSubscriptions()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? throw new UnauthorizedAccessException("User ID not found"));

                var subscriptions = await _pushService.GetUserSubscriptionsAsync(userId);

                return Ok(ApiResponse<List<PushSubscriptionResponseDto>>.SuccessResponse(subscriptions, "Subscriptions retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriptions");
                return StatusCode(500, ApiResponse<List<PushSubscriptionResponseDto>>.ErrorResponse("Error getting subscriptions"));
            }
        }

        /// <summary>
        /// Send test push notification
        /// </summary>
        [HttpPost("test")]
        public async Task<ActionResult<ApiResponse<bool>>> SendTestPush(
            [FromBody] TestPushDto testPush)
        {
            try
            {
                var userId = testPush.UserId;
                
                // Allow admin to test for any user, or user to test for themselves
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
                
                if (!isAdmin && currentUserId != userId.ToString())
                {
                    return Forbid();
                }

                var result = await _pushService.SendTestPushAsync(
                    userId, 
                    testPush.Title, 
                    testPush.Body);

                return Ok(ApiResponse<bool>.SuccessResponse(result, result ? "Test notification sent successfully" : "Failed to send test notification"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test push notification");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error sending test push notification"));
            }
        }

        /// <summary>
        /// Send push notification to specific users (Admin only)
        /// </summary>
        [HttpPost("send")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<int>>> SendPushToUsers(
            [FromBody] SendPushToUsersDto dto)
        {
            try
            {
                var count = await _pushService.SendPushNotificationToUsersAsync(
                    dto.UserIds, 
                    dto.Notification);

                return Ok(ApiResponse<int>.SuccessResponse(count, $"Push notification sent to {count} subscription(s)"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to users");
                return StatusCode(500, ApiResponse<int>.ErrorResponse("Error sending push notification"));
            }
        }

        /// <summary>
        /// Send push notification to all users (Admin only)
        /// </summary>
        [HttpPost("broadcast")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<int>>> BroadcastPush(
            [FromBody] PushNotificationDto notification)
        {
            try
            {
                var count = await _pushService.SendPushNotificationToAllUsersAsync(notification);

                return Ok(ApiResponse<int>.SuccessResponse(count, $"Push notification broadcast to {count} subscription(s)"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting push notification");
                return StatusCode(500, ApiResponse<int>.ErrorResponse("Error broadcasting push notification"));
            }
        }
    }

    // Additional DTO for sending to multiple users
    public class SendPushToUsersDto
    {
        public List<Guid> UserIds { get; set; }
        public PushNotificationDto Notification { get; set; }
    }
}

