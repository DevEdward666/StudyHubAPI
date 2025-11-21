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
            
            // Get all active subscription sessions
            var activeSessions = await context.TableSessions
                .Include(s => s.Table)
                .Include(s => s.User)
                .Include(s => s.Rate)
                .Include(s => s.Subscription)
                    .ThenInclude(sub => sub.Package)
                .Where(s => s.Status == "active" && s.SubscriptionId != null)
                .ToListAsync(ct);

            if (!activeSessions.Any())
            {
                _logger.LogInformation("No active subscription sessions found at {Time}", now);
                return;
            }

            var sessionsToEnd = new List<TableSession>();

            // Check each subscription session
            foreach (var session in activeSessions)
            {
                if (session.Subscription == null)
                    continue;

                // Calculate hours used in THIS session so far
                var sessionElapsedHours = (decimal)(now - session.StartTime).TotalHours;
                
                // Calculate effective remaining hours after accounting for current session
                var effectiveRemainingHours = session.Subscription.RemainingHours - sessionElapsedHours;

                _logger.LogDebug(
                    "Session {SessionId}: RemainingHours={Remaining}h, SessionElapsed={Elapsed}h, Effective={Effective}h",
                    session.Id,
                    session.Subscription.RemainingHours,
                    sessionElapsedHours,
                    effectiveRemainingHours);

                // If effective remaining hours <= 0, session should end
                if (effectiveRemainingHours <= 0)
                {
                    sessionsToEnd.Add(session);
                    _logger.LogInformation(
                        "Subscription session {SessionId} will be ended. User: {UserName}, Remaining: {Remaining}h, Elapsed: {Elapsed}h, Effective: {Effective}h",
                        session.Id,
                        session.User?.Name ?? "Unknown",
                        session.Subscription.RemainingHours,
                        sessionElapsedHours,
                        effectiveRemainingHours);
                }
            }

            if (!sessionsToEnd.Any())
            {
                _logger.LogInformation("No sessions need to be ended at {Time}", now);
                return;
            }

            _logger.LogInformation("Found {Count} sessions to end", sessionsToEnd.Count);

            foreach (var session in sessionsToEnd)
            {
                try
                {
                    // Calculate actual hours used in this session
                    var sessionDuration = (now - session.StartTime).TotalHours;
                    var hoursUsedInSession = (decimal)sessionDuration;
                    
                    _logger.LogInformation(
                        "Ending subscription session {SessionId}. Hours used in session: {Hours}h, Remaining before update: {Remaining}h",
                        session.Id,
                        hoursUsedInSession,
                        session.Subscription?.RemainingHours ?? 0);

                    // Update subscription hours
                    if (session.Subscription != null)
                    {
                        session.Subscription.HoursUsed += hoursUsedInSession;
                        session.Subscription.RemainingHours = Math.Max(0, session.Subscription.TotalHours - session.Subscription.HoursUsed);
                        session.Subscription.Status = "Expired"; // Mark as expired since hours are depleted
                        session.Subscription.UpdatedAt = DateTime.UtcNow;
                    }

                    // Update session
                    session.HoursConsumed = hoursUsedInSession;
                    session.Amount = 0; // No charge for subscription sessions
                    session.Status = "completed";
                    session.EndTime = now;
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
                        UserId = session.UserId,
                        Title = "Subscription Session Ended - Hours Depleted",
                        Message = $"Subscription session ended for table {session.Table?.TableNumber ?? session.TableId.ToString()} - User ran out of hours",
                        Type = NotificationType.Session,
                        Priority = NotificationPriority.High,
                        IsRead = false,
                        Data = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            SessionId = session.Id,
                            TableId = session.TableId,
                            TableNumber = session.Table?.TableNumber,
                            UserName = session.User?.Name,
                            Duration = hoursUsedInSession,
                            Amount = 0m,
                            IsSubscription = true,
                            SubscriptionId = session.SubscriptionId,
                            RemainingHours = session.Subscription?.RemainingHours ?? 0m
                        }),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    context.Notifications.Add(notification);

                    await context.SaveChangesAsync(ct);

                    _logger.LogInformation(
                        "Subscription session {SessionId} ended for table {TableNumber}. User: {UserName}, Final remaining hours: {Remaining}h", 
                        session.Id, 
                        session.Table?.TableNumber, 
                        session.User?.Name ?? session.User?.Email ?? "Unknown",
                        session.Subscription?.RemainingHours ?? 0);

                    // Notify connected admins via SignalR
                    var signalRPayload = new
                    {
                        Id = notification.Id,
                        SessionId = session.Id,
                        TableId = session.TableId,
                        TableNumber = session.Table?.TableNumber,
                        UserName = session.User?.Name ?? session.User?.Email ?? "Guest",
                        Message = notification.Message,
                        Duration = (double)hoursUsedInSession,
                        Amount = 0m,
                        CreatedAt = notification.CreatedAt
                    };

                    _logger.LogInformation(
                        "📡 Sending SessionEnded notification to 'admins' group - Table {TableNumber}, User: {UserName}", 
                        session.Table?.TableNumber, 
                        session.User?.Name ?? session.User?.Email ?? "Guest");

                    await hubContext.Clients.Group("admins").SendAsync("SessionEnded", signalRPayload, ct);
                    
                    _logger.LogInformation("✅ SessionEnded notification sent successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to finalize expired session {SessionId}", session.Id);
                }
            }
        }
    }
}

