using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.EntityFrameworkCore;
using Study_Hub.Data;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using Study_Hub.Service.Interface;
using System.Text.Json;

namespace Study_Hub.Service
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly PushServiceClient _pushClient;
        private readonly IConfiguration _configuration;
        private readonly string _vapidPublicKey;
        private readonly string _vapidPrivateKey;
        private readonly string _vapidSubject;

        public PushNotificationService(
            ApplicationDbContext context,
            PushServiceClient pushClient,
            IConfiguration configuration)
        {
            _context = context;
            _pushClient = pushClient;
            _configuration = configuration;

            _vapidPublicKey = _configuration["WebPush:VapidPublicKey"] 
                ?? throw new InvalidOperationException("VAPID public key not configured");
            _vapidPrivateKey = _configuration["WebPush:VapidPrivateKey"] 
                ?? throw new InvalidOperationException("VAPID private key not configured");
            _vapidSubject = _configuration["WebPush:VapidSubject"] 
                ?? "mailto:admin@studyhub.com";
        }

        #region Subscription Management

        public async Task<PushSubscriptionResponseDto> SubscribeAsync(Guid userId, PushSubscriptionDto subscription)
        {
            // Check if subscription already exists
            var existingSubscription = await _context.PushSubscriptions
                .FirstOrDefaultAsync(ps => ps.Endpoint == subscription.Endpoint);

            if (existingSubscription != null)
            {
                // Update existing subscription
                existingSubscription.UserId = userId;
                existingSubscription.P256dh = subscription.Keys?.P256dh;
                existingSubscription.Auth = subscription.Keys?.Auth;
                existingSubscription.IsActive = true;
                existingSubscription.UserAgent = subscription.UserAgent;
                existingSubscription.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return MapToResponseDto(existingSubscription);
            }

            // Create new subscription
            var newSubscription = new Models.Entities.PushSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Endpoint = subscription.Endpoint,
                P256dh = subscription.Keys?.P256dh,
                Auth = subscription.Keys?.Auth,
                UserAgent = subscription.UserAgent,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PushSubscriptions.Add(newSubscription);
            await _context.SaveChangesAsync();

            return MapToResponseDto(newSubscription);
        }

        public async Task<bool> UnsubscribeAsync(Guid userId, string endpoint)
        {
            var subscription = await _context.PushSubscriptions
                .FirstOrDefaultAsync(ps => ps.UserId == userId && ps.Endpoint == endpoint);

            if (subscription == null)
                return false;

            _context.PushSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PushSubscriptionResponseDto>> GetUserSubscriptionsAsync(Guid userId)
        {
            var subscriptions = await _context.PushSubscriptions
                .Where(ps => ps.UserId == userId && ps.IsActive)
                .ToListAsync();

            return subscriptions.Select(MapToResponseDto).ToList();
        }

        public async Task<bool> UpdateSubscriptionStatusAsync(Guid subscriptionId, bool isActive)
        {
            var subscription = await _context.PushSubscriptions
                .FindAsync(subscriptionId);

            if (subscription == null)
                return false;

            subscription.IsActive = isActive;
            subscription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Push Notifications

        public async Task<bool> SendPushNotificationAsync(Guid userId, PushNotificationDto notification)
        {
            var subscriptions = await _context.PushSubscriptions
                .Where(ps => ps.UserId == userId && ps.IsActive)
                .ToListAsync();

            if (!subscriptions.Any())
                return false;

            var pushMessage = CreatePushMessage(notification);
            var successCount = 0;

            foreach (var subscription in subscriptions)
            {
                try
                {
                    var pushSubscription = new Lib.Net.Http.WebPush.PushSubscription
                    {
                        Endpoint = subscription.Endpoint,
                        Keys = new Dictionary<string, string>
                        {
                            ["p256dh"] = subscription.P256dh ?? "",
                            ["auth"] = subscription.Auth ?? ""
                        }
                    };

                    await _pushClient.RequestPushMessageDeliveryAsync(
                        pushSubscription,
                        pushMessage,
                        new VapidAuthentication(_vapidPublicKey, _vapidPrivateKey)
                        {
                            Subject = _vapidSubject
                        });

                    subscription.LastUsedAt = DateTime.UtcNow;
                    successCount++;
                }
                catch (Exception ex)
                {
                    // Log the error (you can inject ILogger if needed)
                    Console.WriteLine($"Failed to send push notification: {ex.Message}");
                    
                    // If subscription is invalid (410 Gone), deactivate it
                    if (ex.Message.Contains("410"))
                    {
                        subscription.IsActive = false;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return successCount > 0;
        }

        public async Task<int> SendPushNotificationToAllUsersAsync(PushNotificationDto notification)
        {
            var userIds = await _context.PushSubscriptions
                .Where(ps => ps.IsActive)
                .Select(ps => ps.UserId)
                .Distinct()
                .ToListAsync();

            return await SendPushNotificationToUsersAsync(userIds, notification);
        }

        public async Task<int> SendPushNotificationToUsersAsync(List<Guid> userIds, PushNotificationDto notification)
        {
            var subscriptions = await _context.PushSubscriptions
                .Where(ps => userIds.Contains(ps.UserId) && ps.IsActive)
                .ToListAsync();

            if (!subscriptions.Any())
                return 0;

            var pushMessage = CreatePushMessage(notification);
            var successCount = 0;

            foreach (var subscription in subscriptions)
            {
                try
                {
                    var pushSubscription = new Lib.Net.Http.WebPush.PushSubscription
                    {
                        Endpoint = subscription.Endpoint,
                        Keys = new Dictionary<string, string>
                        {
                            ["p256dh"] = subscription.P256dh ?? "",
                            ["auth"] = subscription.Auth ?? ""
                        }
                    };

                    await _pushClient.RequestPushMessageDeliveryAsync(
                        pushSubscription,
                        pushMessage,
                        new VapidAuthentication(_vapidPublicKey, _vapidPrivateKey)
                        {
                            Subject = _vapidSubject
                        });

                    subscription.LastUsedAt = DateTime.UtcNow;
                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send push notification to user {subscription.UserId}: {ex.Message}");
                    
                    if (ex.Message.Contains("410"))
                    {
                        subscription.IsActive = false;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return successCount;
        }

        public async Task<bool> SendTestPushAsync(Guid userId, string title, string body)
        {
            var notification = new PushNotificationDto
            {
                Title = title,
                Body = body,
                Icon = "/icon.png",
                Badge = "/badge.png"
            };

            return await SendPushNotificationAsync(userId, notification);
        }

        #endregion

        #region VAPID Keys

        public string GetVapidPublicKey()
        {
            return _vapidPublicKey;
        }

        #endregion

        #region Cleanup

        public async Task<int> CleanupInactiveSubscriptionsAsync(int inactiveDays = 90)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-inactiveDays);
            
            var inactiveSubscriptions = await _context.PushSubscriptions
                .Where(ps => !ps.IsActive || 
                            (ps.LastUsedAt.HasValue && ps.LastUsedAt < cutoffDate) ||
                            (!ps.LastUsedAt.HasValue && ps.CreatedAt < cutoffDate))
                .ToListAsync();

            if (!inactiveSubscriptions.Any())
                return 0;

            _context.PushSubscriptions.RemoveRange(inactiveSubscriptions);
            await _context.SaveChangesAsync();
            return inactiveSubscriptions.Count;
        }

        #endregion

        #region Helper Methods

        private PushMessage CreatePushMessage(PushNotificationDto notification)
        {
            var payload = new
            {
                notification = new
                {
                    title = notification.Title,
                    body = notification.Body,
                    icon = notification.Icon,
                    badge = notification.Badge,
                    image = notification.Image,
                    tag = notification.Tag,
                    data = notification.Data,
                    actions = notification.Actions,
                    url = notification.Url
                }
            };

            var json = JsonSerializer.Serialize(payload);
            return new PushMessage(json);
        }

        private static PushSubscriptionResponseDto MapToResponseDto(Models.Entities.PushSubscription subscription)
        {
            return new PushSubscriptionResponseDto
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                Endpoint = subscription.Endpoint,
                IsActive = subscription.IsActive,
                CreatedAt = subscription.CreatedAt,
                LastUsedAt = subscription.LastUsedAt
            };
        }

        #endregion
    }
}

