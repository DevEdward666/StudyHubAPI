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

                bool shouldEndSession = false;
                string endReason = "";

                // Calculate hours used in THIS session so far
                var sessionElapsedHours = (decimal)(now - session.StartTime).TotalHours;
                
                // Calculate effective remaining hours after accounting for current session
                var effectiveRemainingHours = session.Subscription.RemainingHours - sessionElapsedHours;

                // Check 1: Hours consumed (existing logic)
                if (effectiveRemainingHours <= 0)
                {
                    shouldEndSession = true;
                    endReason = "Hours depleted";
                    
                    _logger.LogInformation(
                        "Session {SessionId} - Hours depleted. Remaining: {Remaining}h, Elapsed: {Elapsed}h, Effective: {Effective}h",
                        session.Id,
                        session.Subscription.RemainingHours,
                        sessionElapsedHours,
                        effectiveRemainingHours);
                }

                // Check 2: Expiry date reached (for monthly/weekly/daily packages)
                if (session.Subscription.ExpiryDate.HasValue && now >= session.Subscription.ExpiryDate.Value)
                {
                    shouldEndSession = true;
                    endReason = shouldEndSession && endReason != "" 
                        ? "Hours depleted & Subscription expired" 
                        : "Subscription period expired";
                    
                    _logger.LogInformation(
                        "Session {SessionId} - Subscription expired. ExpiryDate: {ExpiryDate}, Current: {Now}",
                        session.Id,
                        session.Subscription.ExpiryDate.Value,
                        now);
                }

                _logger.LogDebug(
                    "Session {SessionId}: RemainingHours={Remaining}h, SessionElapsed={Elapsed}h, Effective={Effective}h, ExpiryDate={Expiry}, ShouldEnd={ShouldEnd}, Reason={Reason}",
                    session.Id,
                    session.Subscription.RemainingHours,
                    sessionElapsedHours,
                    effectiveRemainingHours,
                    session.Subscription.ExpiryDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "None",
                    shouldEndSession,
                    endReason);

                // End session if either condition is met
                if (shouldEndSession)
                {
                    sessionsToEnd.Add(session);
                    _logger.LogInformation(
                        "Subscription session {SessionId} will be ended. User: {UserName}, Reason: {Reason}",
                        session.Id,
                        session.User?.Name ?? "Unknown",
                        endReason);
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
                    
                    // Determine end reason for this specific session
                    var sessionElapsedHours = (decimal)(now - session.StartTime).TotalHours;
                    var effectiveRemainingHours = session.Subscription!.RemainingHours - sessionElapsedHours;
                    var hoursExpired = effectiveRemainingHours <= 0;
                    var dateExpired = session.Subscription.ExpiryDate.HasValue && now >= session.Subscription.ExpiryDate.Value;
                    
                    string endReason = "";
                    string notificationTitle = "";
                    string notificationMessage = "";
                    
                    if (hoursExpired && dateExpired)
                    {
                        endReason = "Hours depleted & Subscription expired";
                        notificationTitle = "Subscription Session Ended";
                        notificationMessage = $"Subscription session ended for table {session.Table?.TableNumber ?? session.TableId.ToString()} - Both hours depleted and subscription period expired";
                    }
                    else if (hoursExpired)
                    {
                        endReason = "Hours depleted";
                        notificationTitle = "Subscription Session Ended - Hours Depleted";
                        notificationMessage = $"Subscription session ended for table {session.Table?.TableNumber ?? session.TableId.ToString()} - User ran out of hours";
                    }
                    else if (dateExpired)
                    {
                        endReason = "Subscription period expired";
                        notificationTitle = "Subscription Session Ended - Period Expired";
                        notificationMessage = $"Subscription session ended for table {session.Table?.TableNumber ?? session.TableId.ToString()} - Subscription period has expired on {session.Subscription.ExpiryDate.Value:MMM dd, yyyy hh:mm tt}";
                    }
                    
                    _logger.LogInformation(
                        "Ending subscription session {SessionId}. Hours used in session: {Hours}h, Remaining before update: {Remaining}h, Reason: {Reason}",
                        session.Id,
                        hoursUsedInSession,
                        session.Subscription?.RemainingHours ?? 0,
                        endReason);

                    // Update subscription hours
                    if (session.Subscription != null)
                    {
                        session.Subscription.HoursUsed += hoursUsedInSession;
                        session.Subscription.RemainingHours = Math.Max(0, session.Subscription.TotalHours - session.Subscription.HoursUsed);
                        
                        // Only mark as expired if either hours depleted or date expired
                        if (hoursExpired || dateExpired)
                        {
                            session.Subscription.Status = "Expired";
                        }
                        
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
                        Title = notificationTitle,
                        Message = notificationMessage,
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
                            RemainingHours = session.Subscription?.RemainingHours ?? 0m,
                            EndReason = endReason,
                            ExpiryDate = session.Subscription?.ExpiryDate
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

