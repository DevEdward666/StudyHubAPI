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
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get paginated notifications for the current user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedNotificationsDto>>> GetNotifications([FromQuery] NotificationFilterDto filter)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _notificationService.GetUserNotificationsAsync(userId, filter);

                return Ok(ApiResponse<PaginatedNotificationsDto>.SuccessResponse(notifications, "Notifications retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedNotificationsDto>.ErrorResponse($"Error retrieving notifications: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a specific notification by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> GetNotification(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notification = await _notificationService.GetNotificationByIdAsync(id, userId);

                if (notification == null)
                {
                    return NotFound(ApiResponse<NotificationDto>.ErrorResponse("Notification not found"));
                }

                return Ok(ApiResponse<NotificationDto>.SuccessResponse(notification, "Notification retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<NotificationDto>.ErrorResponse($"Error retrieving notification: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get unread notifications for the current user
        /// </summary>
        [HttpGet("unread")]
        public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetUnreadNotifications()
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);

                return Ok(ApiResponse<List<NotificationDto>>.SuccessResponse(notifications, "Unread notifications retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<NotificationDto>>.ErrorResponse($"Error retrieving unread notifications: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get notification summary for the current user
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<NotificationSummaryDto>>> GetNotificationSummary()
        {
            try
            {
                var userId = GetCurrentUserId();
                var summary = await _notificationService.GetNotificationSummaryAsync(userId);

                return Ok(ApiResponse<NotificationSummaryDto>.SuccessResponse(summary, "Notification summary retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<NotificationSummaryDto>.ErrorResponse($"Error retrieving notification summary: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get unread count for the current user
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _notificationService.GetUnreadCountAsync(userId);

                return Ok(ApiResponse<int>.SuccessResponse(count, "Unread count retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.ErrorResponse($"Error retrieving unread count: {ex.Message}"));
            }
        }

        /// <summary>
        /// Mark specific notifications as read
        /// </summary>
        [HttpPut("mark-read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkNotificationsAsRead([FromBody] MarkNotificationReadDto markReadDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.MarkNotificationsAsReadAsync(userId, markReadDto);

                return Ok(ApiResponse<bool>.SuccessResponse(result, result ? "Notifications marked as read successfully" : "No notifications were updated"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Error marking notifications as read: {ex.Message}"));
            }
        }

        /// <summary>
        /// Mark a single notification as read
        /// </summary>
        [HttpPut("{id}/mark-read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkNotificationAsRead(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.MarkNotificationAsReadAsync(id, userId);

                if (!result)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("Notification not found or already read"));
                }

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Notification marked as read successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Error marking notification as read: {ex.Message}"));
            }
        }

        /// <summary>
        /// Mark all notifications as read for the current user
        /// </summary>
        [HttpPut("mark-all-read")]
        public async Task<ActionResult<ApiResponse<bool>>> MarkAllNotificationsAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.MarkAllNotificationsAsReadAsync(userId);

                return Ok(ApiResponse<bool>.SuccessResponse(result, result ? "All notifications marked as read successfully" : "No unread notifications found"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Error marking all notifications as read: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update a notification
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> UpdateNotification(Guid id, [FromBody] UpdateNotificationDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.UpdateNotificationAsync(id, updateDto, userId);

                if (result == null)
                {
                    return NotFound(ApiResponse<NotificationDto>.ErrorResponse("Notification not found"));
                }

                return Ok(ApiResponse<NotificationDto>.SuccessResponse(result, "Notification updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<NotificationDto>.ErrorResponse($"Error updating notification: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteNotification(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.DeleteNotificationAsync(id, userId);

                if (!result)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse("Notification not found"));
                }

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Notification deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Error deleting notification: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create a notification (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> CreateNotification([FromBody] CreateNotificationDto createDto)
        {
            try
            {
                var notification = await _notificationService.CreateNotificationAsync(createDto);

                return CreatedAtAction(
                    nameof(GetNotification), 
                    new { id = notification.Id }, 
                    ApiResponse<NotificationDto>.SuccessResponse(notification, "Notification created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<NotificationDto>.ErrorResponse($"Error creating notification: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create bulk notifications (Admin only)
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> CreateBulkNotifications([FromBody] BulkNotificationDto bulkDto)
        {
            try
            {
                var notifications = await _notificationService.CreateBulkNotificationsAsync(bulkDto);

                return Ok(ApiResponse<List<NotificationDto>>.SuccessResponse(notifications, $"{notifications.Count} notifications created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<NotificationDto>>.ErrorResponse($"Error creating bulk notifications: {ex.Message}"));
            }
        }

        /// <summary>
        /// Clean up expired notifications (Admin only)
        /// </summary>
        [HttpDelete("expired")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteExpiredNotifications()
        {
            try
            {
                var result = await _notificationService.DeleteExpiredNotificationsAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(result, result ? "Expired notifications deleted successfully" : "No expired notifications found"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Error deleting expired notifications: {ex.Message}"));
            }
        }

        /// <summary>
        /// Send welcome notification (Internal use)
        /// </summary>
        [HttpPost("welcome/{userId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponse<bool>>> SendWelcomeNotification(Guid userId, [FromBody] string userName)
        {
            try
            {
                await _notificationService.SendWelcomeNotificationAsync(userId, userName);

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Welcome notification sent successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Error sending welcome notification: {ex.Message}"));
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token");
            }
            return userId;
        }
    }
}