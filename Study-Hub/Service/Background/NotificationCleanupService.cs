using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Study_Hub.Service.Interface;

namespace Study_Hub.Service.Background
{
    public class NotificationCleanupService : BackgroundService
    {
        private readonly ILogger<NotificationCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Run every 6 hours

        public NotificationCleanupService(
            ILogger<NotificationCleanupService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredNotifications();
                    await CleanupOldNotifications();
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up notifications");
                    // Wait a shorter time before retrying on error
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
            }

            _logger.LogInformation("Notification Cleanup Service stopped");
        }

        private async Task CleanupExpiredNotifications()
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                var result = await notificationService.DeleteExpiredNotificationsAsync();
                if (result)
                {
                    _logger.LogInformation("Expired notifications cleaned up successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired notifications");
            }
        }

        private async Task CleanupOldNotifications()
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                // Delete notifications older than 90 days
                var cutoffDate = DateTime.UtcNow.AddDays(-90);
                var deletedCount = await notificationService.DeleteOldNotificationsAsync(cutoffDate);
                
                if (deletedCount > 0)
                {
                    _logger.LogInformation("Cleaned up {DeletedCount} old notifications", deletedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old notifications");
            }
        }
    }
}