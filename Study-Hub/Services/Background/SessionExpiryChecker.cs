using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Study_Hub.Data;
using Study_Hub.Models.Entities;
using Study_Hub.Hubs;

namespace Study_Hub.Services.Background
{
    public class SessionExpiryChecker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SessionExpiryChecker> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

        public SessionExpiryChecker(
            IServiceScopeFactory scopeFactory, 
            ILogger<SessionExpiryChecker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SessionExpiryChecker started. Checking every {Interval} minutes.", _interval.TotalMinutes);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckForExpiredSessions(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while checking expired sessions.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task CheckForExpiredSessions(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

            var now = DateTime.UtcNow;
            
            var expiredSessions = await context.TableSessions
                .Include(s => s.Table)
                .Include(s => s.User)
                .Include(s => s.Rate)
                .Where(s => s.Status == "active" && s.EndTime.HasValue && s.EndTime <= now)
                .ToListAsync(ct);

            if (!expiredSessions.Any())
            {
                _logger.LogInformation("No expired sessions found at {Time}", now);
                return;
            }

            _logger.LogInformation("Found {Count} expired sessions to process", expiredSessions.Count);

            foreach (var session in expiredSessions)
            {
                try
                {
                    // Compute amount similar to EndTableSessionAsync
                    var duration = session.EndTime.Value - session.StartTime;
                    var hoursUsed = Math.Ceiling(duration.TotalHours);
                    var tableRate = session.Rate?.Price ?? session.Table.HourlyRate;
                    var creditsUsed = (decimal)(hoursUsed * (double)tableRate);

                    // Update user credits
                    var userCredits = await context.UserCredit
                        .FirstOrDefaultAsync(uc => uc.UserId == session.UserId, ct);
                    
                    if (userCredits != null)
                    {
                        userCredits.Balance = Math.Max(0, userCredits.Balance - creditsUsed);
                        userCredits.TotalSpent += creditsUsed;
                        userCredits.UpdatedAt = DateTime.UtcNow;
                    }

                    // Update session
                    session.Amount = creditsUsed;
                    session.Status = "completed";
                    session.UpdatedAt = DateTime.UtcNow;

                    // Free table
                    if (session.Table != null)
                    {
                        session.Table.IsOccupied = false;
                        session.Table.CurrentUserId = null;
                        session.Table.UpdatedAt = DateTime.UtcNow;
                    }

                    // Create notification for admin
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = session.UserId, // Link to the user who had the session
                        Title = "Session Expired",
                        Message = $"Session ended for table {session.Table?.TableNumber ?? session.TableId.ToString()}",
                        Type = NotificationType.Session,
                        Priority = NotificationPriority.High,
                        IsRead = false,
                        Data = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            SessionId = session.Id,
                            TableId = session.TableId,
                            TableNumber = session.Table?.TableNumber,
                            UserName = session.User?.Name,
                            Duration = duration.TotalHours,
                            Amount = creditsUsed
                        }),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    context.Notifications.Add(notification);

                    await context.SaveChangesAsync(ct);

                    _logger.LogInformation(
                        "Session {SessionId} expired for table {TableNumber}. User: {UserId}", 
                        session.Id, 
                        session.Table?.TableNumber, 
                        session.UserId);

                    // Notify connected admins via SignalR
                    await hubContext.Clients.Group("admins").SendAsync("SessionEnded", new
                    {
                        Id = notification.Id,
                        SessionId = session.Id,
                        TableId = session.TableId,
                        TableNumber = session.Table?.TableNumber,
                        UserName = session.User?.Name ?? session.User?.Email ?? "Guest",
                        Message = notification.Message,
                        Duration = duration.TotalHours,
                        Amount = creditsUsed,
                        CreatedAt = notification.CreatedAt
                    }, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to finalize expired session {SessionId}", session.Id);
                }
            }
        }
    }
}

