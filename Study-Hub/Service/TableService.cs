using Microsoft.EntityFrameworkCore;
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
            var tables = await _context.StudyTables.ToListAsync();
            return tables.Select(MapToStudyTableDto).ToList();
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
                if (table == null || table.QrCode != request.QrCode)
                    throw new InvalidOperationException("Invalid QR code for this table");

                if (table.IsOccupied)
                    throw new InvalidOperationException("Table is already occupied");

                // Check user has credits
                var userCredits = await _context.UserCredit
                    .FirstOrDefaultAsync(uc => uc.UserId == userId);

                if (userCredits == null || userCredits.Balance < table.HourlyRate)
                    throw new InvalidOperationException("Insufficient credits");

                // Mark table as occupied
                table.IsOccupied = true;
                table.CurrentUserId = userId;
                table.UpdatedAt = DateTime.UtcNow;

                // Create session
                var session = new TableSession
                {
                    UserId = userId,
                    TableId = request.TableId,
                    StartTime = DateTime.UtcNow,
                    CreditsUsed = 0,
                    Status = SessionStatus.active,
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
                    .FirstOrDefaultAsync(ts => ts.Id == sessionId && ts.UserId == userId);

                if (session == null)
                    throw new InvalidOperationException("Session not found or unauthorized");

                if (session.Status != SessionStatus.active)
                    throw new InvalidOperationException("Session is not active");

                var endTime = DateTime.UtcNow;
                var duration = endTime - session.StartTime;
                var hoursUsed = Math.Ceiling(duration.TotalHours);
                var creditsUsed = (decimal)(hoursUsed * (double)session.Table.HourlyRate);

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
                session.EndTime = endTime;
                session.CreditsUsed = creditsUsed;
                session.Status = SessionStatus.completed;
                session.UpdatedAt = DateTime.UtcNow;

                // Free up table
                session.Table.IsOccupied = false;
                session.Table.CurrentUserId = null;
                session.Table.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new EndSessionResponseDto
                {
                    CreditsUsed = creditsUsed,
                    Duration = (long)duration.TotalMilliseconds
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
                .FirstOrDefaultAsync(ts => ts.UserId == userId && ts.Status == SessionStatus.active);

            return session != null ? MapToSessionWithTableDto(session) : null;
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
                CreditsUsed = session.CreditsUsed,
                Status = session.Status,
                Table = MapToStudyTableDto(session.Table),
                CreatedAt = session.CreatedAt
            };
        }
    }
}