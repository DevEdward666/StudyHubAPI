﻿﻿﻿using Microsoft.EntityFrameworkCore;
using Study_Hub.Data;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using StudyHubApi.Services.Interfaces;

namespace StudyHubApi.Services
{
    public class TableService : ITableService
    {
        private readonly ApplicationDbContext _context;

        public TableService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudyTableDto>> GetAllTablesAsync()
        {
            var tables = await _context.StudyTables
                .Include(t => t.TableSessions.Where(s => s.Status.ToLower() == "active"))
                    .ThenInclude(s => s.User)
                .Include(t => t.TableSessions.Where(s => s.Status.ToLower() == "active"))
                    .ThenInclude(s => s.Subscription)
                        .ThenInclude(sub => sub.Package)
                .ToListAsync();


            return tables.Select(MapToStudyTableDtoWithSession).ToList();
        }

        // Update or create this mapping method
        private StudyTableDto MapToStudyTableDtoWithSession(StudyTable table)
        {
            var dto = MapToStudyTableDto(table); // Your existing mapping
    
            // Add active session data with subscription support
            var activeSession = table.TableSessions?.FirstOrDefault(s => s.Status.ToLower() == "active");
            if (activeSession != null)
            {
                dto.CurrentSession = new CurrentSessionDto
                {
                    Id = activeSession.Id,
                    StartTime = activeSession.StartTime,
                    EndTime = activeSession.EndTime, // Keep null for subscription sessions
                    CustomerName = activeSession.User?.Name ?? "Guest",
                    IsSubscriptionBased = activeSession.IsSubscriptionBased,
                    SubscriptionId = activeSession.SubscriptionId,
                    Subscription = activeSession.Subscription != null ? new UserSubscriptionDto
                    {
                        Id = activeSession.Subscription.Id,
                        UserId = activeSession.Subscription.UserId,
                        PackageId = activeSession.Subscription.PackageId,
                        PackageName = activeSession.Subscription.Package?.Name,
                        TotalHours = activeSession.Subscription.TotalHours,
                        RemainingHours = activeSession.Subscription.RemainingHours,
                        HoursUsed = activeSession.Subscription.HoursUsed,
                        Status = activeSession.Subscription.Status,
                        PurchaseDate = activeSession.Subscription.PurchaseDate,
                        ActivationDate = activeSession.Subscription.ActivationDate,
                        ExpiryDate = activeSession.Subscription.ExpiryDate
                    } : null
                };
            }
    
            return dto;
        }
        public async Task<StudyTableDto?> GetTableByQRAsync(string qrCode)
        {
            var table = await _context.StudyTables
                .FirstOrDefaultAsync(st => st.QrCode == qrCode);

            return table != null ? MapToStudyTableDto(table) : null;
        }

        public async Task<Guid> StartTableSessionAsync(Guid userId, StartSessionRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Verify QR code matches table
                var table = await _context.StudyTables.FindAsync(request.TableId);
                if (table == null)
                    throw new InvalidOperationException("Invalid QR code for this table");

                if (table.IsOccupied)
                    throw new InvalidOperationException("Table is already occupied");

                // Check user has credits - use provided userId or fall back to authenticated userId
                var targetUserId = !string.IsNullOrEmpty(request.userId) && Guid.TryParse(request.userId, out var parsedUserId)
                    ? parsedUserId
                    : userId;
                
               
                // Mark table as occupied
                table.IsOccupied = true;
                table.CurrentUserId = targetUserId;
                table.UpdatedAt = DateTime.UtcNow;
                
                // Create session
                var startTime = DateTime.UtcNow;
                var endTime = startTime.AddHours(request.hours);
                
                var session = new TableSession
                {
                    UserId = targetUserId,
                    TableId = request.TableId,
                    StartTime = startTime,
                    EndTime = endTime,
                    Amount = request.amount,
                    Status = "active",
                    PaymentMethod = request.PaymentMethod,
                    Cash = request.Cash,
                    Change = request.Change,
                    RateId = request.RateId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.TableSessions.Add(session);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return session.Id;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<EndSessionResponseDto> EndTableSessionAsync(Guid userId, Guid sessionId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                
                var session = await _context.TableSessions
                    .Include(ts => ts.Table)
                    .Include(ts => ts.User)
                    .Include(ts => ts.Rate)
                    .Include(ts => ts.Subscription)
                        .ThenInclude(s => s.Package)
                    .FirstOrDefaultAsync(ts => ts.Id == sessionId);

                if (session == null)
                    throw new InvalidOperationException("Session not found or unauthorized");

                if (session.Status != "active")
                    throw new InvalidOperationException("Session is not active");

                // If this is a subscription session, use EndSubscriptionSessionAsync logic
                if (session.IsSubscriptionBased && session.Subscription != null)
                {
                    var endTime = DateTime.UtcNow;
                    var duration = endTime - session.StartTime;
                    var hoursUsed = (decimal)duration.TotalHours; // Precise hours for subscription

                    // Update subscription hours
                    var subscription = session.Subscription;
                    subscription.HoursUsed += hoursUsed;
                    subscription.RemainingHours = Math.Max(0, subscription.TotalHours - subscription.HoursUsed);
                    subscription.UpdatedAt = DateTime.UtcNow;

                    // Check if subscription is depleted
                    if (subscription.RemainingHours <= 0)
                    {
                        subscription.Status = "Expired";
                    }

                    // Update session
                    session.EndTime = endTime;
                    session.HoursConsumed = hoursUsed;
                    session.Amount = 0; // No additional charge for subscription
                    session.Status = "completed";
                    session.UpdatedAt = DateTime.UtcNow;

                    // Free up table
                    session.Table.IsOccupied = false;
                    session.Table.CurrentUserId = null;
                    session.Table.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new EndSessionResponseDto
                    {
                        SessionId = session.Id,
                        Amount = 0,
                        Duration = (long)duration.TotalMilliseconds,
                        Hours = (double)hoursUsed,
                        Rate = 0,
                        TableNumber = session.Table.TableNumber,
                        CustomerName = session.User?.Name ?? session.User?.Email ?? "Guest",
                        StartTime = session.StartTime,
                        EndTime = endTime,
                        PaymentMethod = $"Subscription: {subscription.Package?.Name}",
                        Cash = null,
                        Change = null
                    };
                }

                // For non-subscription sessions, use ceiling for billing
                var nonSubEndTime = DateTime.UtcNow;
                var nonSubDuration = nonSubEndTime - session.StartTime;
                var nonSubHoursUsed = Math.Ceiling(nonSubDuration.TotalHours);
                var creditsUsed = (decimal)(nonSubHoursUsed * (double)session.Table.HourlyRate);

                // Update user credits
                var userCredits = await _context.UserCredit
                    .FirstOrDefaultAsync(uc => uc.UserId == userId);

                if (userCredits != null)
                {
                    userCredits.Balance = Math.Max(0, userCredits.Balance - creditsUsed);
                    userCredits.TotalSpent += creditsUsed;
                    userCredits.UpdatedAt = DateTime.UtcNow;
                }

                // Update session
                session.EndTime = nonSubEndTime;
                session.Amount = creditsUsed;
                session.Status = "completed";
                session.UpdatedAt = DateTime.UtcNow;

                // Free up table
                session.Table.IsOccupied = false;
                session.Table.CurrentUserId = null;
                session.Table.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Return complete receipt information
                return new EndSessionResponseDto
                {
                    SessionId = session.Id,
                    Amount = creditsUsed,
                    Duration = (long)nonSubDuration.TotalMilliseconds,
                    Hours = nonSubHoursUsed,
                    Rate = session.Rate?.Price ?? session.Table.HourlyRate,
                    TableNumber = session.Table.TableNumber,
                    CustomerName = session.User.Name ?? session.User.Email,
                    StartTime = session.StartTime,
                    EndTime = nonSubEndTime,
                    PaymentMethod = session.PaymentMethod,
                    Cash = session.Cash,
                    Change = session.Change
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<SessionWithTableDto?> GetUserActiveSessionAsync(Guid userId)
        {
            var session = await _context.TableSessions
                .Include(ts => ts.Table)
                .FirstOrDefaultAsync(ts => ts.UserId == userId && ts.Status == "active");

            return session != null ? MapToSessionWithTableDto(session) : null;
        }

        public async Task<ChangeTableResponseDto> ChangeTableAsync(Guid userId, ChangeTableRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get the current session
                var currentSession = await _context.TableSessions
                    .Include(ts => ts.Table)
                    .FirstOrDefaultAsync(ts => ts.Id == request.SessionId);

                if (currentSession == null)
                    throw new InvalidOperationException("Session not found or unauthorized");

                if (currentSession.Status != "active")
                    throw new InvalidOperationException("Session is not active");

                // Get the new table
                var newTable = await _context.StudyTables.FindAsync(request.NewTableId);
                if (newTable == null)
                    throw new InvalidOperationException("New table not found");

                if (newTable.IsOccupied)
                    throw new InvalidOperationException("New table is already occupied");

                // Get the old table
                var oldTable = currentSession.Table;

                // Free up the old table
                oldTable.IsOccupied = false;
                oldTable.CurrentUserId = null;
                oldTable.UpdatedAt = DateTime.UtcNow;

                // Occupy the new table
                newTable.IsOccupied = true;
                newTable.CurrentUserId = userId;
                newTable.UpdatedAt = DateTime.UtcNow;

                // Move the active session to the new table (do NOT end it)
                currentSession.TableId = request.NewTableId;
                currentSession.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ChangeTableResponseDto
                {
                    Success = true,
                    Message = $"Successfully moved session from {oldTable.TableNumber} to {newTable.TableNumber}",
                    NewSessionId = currentSession.Id
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Failed to change table: {ex.Message}");
            }
        }

        public async Task<Guid> StartSubscriptionSessionAsync(Guid userId, StartSubscriptionSessionDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Verify table exists and is available
                var table = await _context.StudyTables.FindAsync(request.TableId);
                if (table == null)
                    throw new InvalidOperationException("Table not found");

                if (table.IsOccupied)
                    throw new InvalidOperationException("Table is already occupied");

                // Get and validate subscription
                var subscription = await _context.UserSubscriptions
                    .Include(s => s.Package)
                    .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId);

                if (subscription == null)
                    throw new InvalidOperationException("Subscription not found");

                if (subscription.Status != "Active")
                    throw new InvalidOperationException("Subscription is not active");

                if (subscription.RemainingHours <= 0)
                    throw new InvalidOperationException("No remaining hours in subscription");

                // Determine user (allow admin to assign)
                var targetUserId = !string.IsNullOrEmpty(request.UserId) && Guid.TryParse(request.UserId, out var parsedUserId)
                    ? parsedUserId
                    : userId;

                // Verify subscription belongs to target user
                if (subscription.UserId != targetUserId)
                    throw new InvalidOperationException("Subscription does not belong to this user");

                // Mark table as occupied
                table.IsOccupied = true;
                table.CurrentUserId = targetUserId;
                table.UpdatedAt = DateTime.UtcNow;

                // Create session - no end time, will be calculated when ended
                var startTime = DateTime.UtcNow;

                // Activate subscription on first use
                if (!subscription.ActivationDate.HasValue)
                {
                    subscription.ActivationDate = startTime;
                }

                var session = new TableSession
                {
                    UserId = targetUserId,
                    TableId = request.TableId,
                    StartTime = startTime,
                    EndTime = null, // Will be set when session ends
                    Amount = 0, // Will be calculated on end
                    Status = "active",
                    IsSubscriptionBased = true,
                    SubscriptionId = request.SubscriptionId,
                    HoursConsumed = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.TableSessions.Add(session);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return session.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<EndSessionResponseDto> EndSubscriptionSessionAsync(Guid userId, Guid sessionId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var session = await _context.TableSessions
                    .Include(ts => ts.Table)
                    .Include(ts => ts.User)
                    .Include(ts => ts.Subscription)
                        .ThenInclude(s => s.Package)
                    .FirstOrDefaultAsync(ts => ts.Id == sessionId);

                if (session == null)
                    throw new InvalidOperationException("Session not found");

                if (session.Status != "active")
                    throw new InvalidOperationException("Session is not active");

                if (!session.IsSubscriptionBased || session.Subscription == null)
                    throw new InvalidOperationException("This is not a subscription-based session");

                var endTime = DateTime.UtcNow;
                var duration = endTime - session.StartTime;
                var hoursUsed = (decimal)duration.TotalHours; // Use actual hours, not ceiling

                // Update subscription hours
                var subscription = session.Subscription;
                subscription.HoursUsed += hoursUsed;
                subscription.RemainingHours = Math.Max(0, subscription.TotalHours - subscription.HoursUsed);
                subscription.UpdatedAt = DateTime.UtcNow;

                // Check if subscription is depleted
                if (subscription.RemainingHours <= 0)
                {
                    subscription.Status = "Expired";
                }

                // Update session
                session.EndTime = endTime;
                session.HoursConsumed = hoursUsed;
                session.Amount = 0; // No additional charge for subscription
                session.Status = "completed";
                session.UpdatedAt = DateTime.UtcNow;

                // Free up table
                session.Table.IsOccupied = false;
                session.Table.CurrentUserId = null;
                session.Table.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new EndSessionResponseDto
                {
                    SessionId = session.Id,
                    Amount = 0,
                    Duration = (long)duration.TotalMilliseconds,
                    Hours = Math.Ceiling((double)hoursUsed),
                    Rate = 0,
                    TableNumber = session.Table.TableNumber,
                    CustomerName = session.User.Name ?? session.User.Email,
                    StartTime = session.StartTime,
                    EndTime = endTime,
                    PaymentMethod = "Subscription",
                    Cash = null,
                    Change = null
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static StudyTableDto MapToStudyTableDto(StudyTable table)
        {
            return new StudyTableDto
            {
                Id = table.Id,
                TableNumber = table.TableNumber,
                QrCode = table.QrCode,
                QrCodeImage = table.QrCodeImage,
                IsOccupied = table.IsOccupied,
                CurrentUserId = table.CurrentUserId,
                HourlyRate = table.HourlyRate,
                Location = table.Location,
                Capacity = table.Capacity,
                CreatedAt = table.CreatedAt
            };
        }

        private static SessionWithTableDto MapToSessionWithTableDto(TableSession session)
        {
            return new SessionWithTableDto
            {
                Id = session.Id,
                UserId = session.UserId,
                TableId = session.TableId,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Amount = session.Amount,
                Status = session.Status,
                Table = MapToStudyTableDto(session.Table),
                CreatedAt = session.CreatedAt
            };
        }
    }
}