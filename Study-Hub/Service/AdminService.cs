using Microsoft.EntityFrameworkCore;
using Study_Hub.Data;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using Study_Hub.Services.Interfaces;

namespace Study_Hub.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsAdminAsync(Guid userId)
        {
            return await _context.AdminUsers.AnyAsync(au => au.UserId == userId);
        }

        public async Task<List<UserWithInfoDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
             .Include(u => u.UserCredits)
             .Include(u => u.TableSessions)
             .ToListAsync();

            return users.Select(user => new UserWithInfoDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Credits = user.UserCredits?.Balance ?? 0,
                IsAdmin = user.AdminUser != null,
                HasActiveSession = user.TableSessions?.Any(ts => ts.Status == SessionStatus.Active) ?? false, // Null check here
                CreatedAt = user.CreatedAt
            }).ToList();
        }

        public async Task<List<TransactionWithUserDto>> GetPendingTransactionsAsync()
        {
            var transactions = await _context.CreditTransactions
                .Include(ct => ct.User)
                .Where(ct => ct.Status == TransactionStatus.Pending)
                .OrderByDescending(ct => ct.CreatedAt)
                .ToListAsync();

            return transactions.Select(MapToTransactionWithUserDto).ToList();
        }

        public async Task<bool> ApproveTransactionAsync(Guid transactionId, Guid adminUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var creditTransaction = await _context.CreditTransactions.FindAsync(transactionId);
                if (creditTransaction == null || creditTransaction.Status != TransactionStatus.Pending)
                    return false;

                // Update transaction status
                creditTransaction.Status = TransactionStatus.Approved;
                creditTransaction.ApprovedBy = adminUserId;
                creditTransaction.ApprovedAt = DateTime.UtcNow;

                // Add credits to user's balance
                var userCredits = await _context.UserCredit
                    .FirstOrDefaultAsync(uc => uc.UserId == creditTransaction.UserId);

                if (userCredits == null)
                {
                    userCredits = new UserCredit
                    {
                        UserId = creditTransaction.UserId,
                        Balance = creditTransaction.Amount,
                        TotalPurchased = creditTransaction.Amount,
                        TotalSpent = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UserCredit.Add(userCredits);
                }
                else
                {
                    userCredits.Balance += creditTransaction.Amount;
                    userCredits.TotalPurchased += creditTransaction.Amount;
                    userCredits.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> RejectTransactionAsync(Guid transactionId, Guid adminUserId)
        {
            var creditTransaction = await _context.CreditTransactions.FindAsync(transactionId);
            if (creditTransaction == null || creditTransaction.Status != TransactionStatus.Pending)
                return false;

            creditTransaction.Status = TransactionStatus.Rejected;
            creditTransaction.ApprovedBy = adminUserId;
            creditTransaction.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CreateTableResponseDto> CreateStudyTableAsync(CreateTableRequestDto request)
        {
            var qrCode = $"TABLE_{request.TableNumber}_{GenerateRandomString(7)}";

            var table = new StudyTable
            {
                TableNumber = request.TableNumber,
                QrCode = qrCode,
                IsOccupied = false,
                HourlyRate = request.HourlyRate,
                Location = request.Location,
                Capacity = request.Capacity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.StudyTables.Add(table);
            await _context.SaveChangesAsync();

            return new CreateTableResponseDto
            {
                TableId = table.Id,
                QrCode = qrCode
            };
        }
        public async Task<UpdateTableResponseDto> UpdateStudyTableAsync(UpdateTableRequestDto request)
        {
            var studyTable = await _context.StudyTables.FirstOrDefaultAsync(u => u.Id.ToString() == request.TableID);
            if (studyTable == null)
                throw new InvalidOperationException("User not found");
            studyTable.TableNumber = request.TableNumber;
            studyTable.Location = request.Location;
            studyTable.Capacity = request.Capacity;
            studyTable.HourlyRate = request.HourlyRate;
            studyTable.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UpdateTableResponseDto
            {
                TableNumber = studyTable.TableNumber,
                QrCode = studyTable.QrCode
            };
        }
        public async Task<SelectedTableResponseDto> SelectedStudyTableAsync(SelectedTableRequestDto request)
        {
            var studyTable = await _context.StudyTables.FirstOrDefaultAsync(u => u.Id.ToString() == request.TableID);
            if (studyTable == null)
                throw new InvalidOperationException("User not found");
            return new SelectedTableResponseDto
            {
                TableID = request.TableID,
                TableNumber = studyTable.TableNumber,
                Location = studyTable.Location,
                Capacity = studyTable.Capacity,
                HourlyRate = studyTable.HourlyRate,
                QrCode = studyTable.QrCode
            };
        }
        public async Task<bool> MakeUserAdminAsync(string userEmail)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var existingAdmin = await _context.AdminUsers.AnyAsync(au => au.UserId == user.Id);
            if (existingAdmin)
                throw new InvalidOperationException("User is already an admin");

            var adminUser = new AdminUser
            {
                UserId = user.Id,
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.AdminUsers.Add(adminUser);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ToggleUserAdminResponseDto> ToggleUserAdminAsync(Guid userId)
        {
            var existingAdmin = await _context.AdminUsers.FirstOrDefaultAsync(au => au.UserId == userId);

            if (existingAdmin != null)
            {
                _context.AdminUsers.Remove(existingAdmin);
                await _context.SaveChangesAsync();
                return new ToggleUserAdminResponseDto { IsAdmin = false };
            }
            else
            {
                var adminUser = new AdminUser
                {
                    UserId = userId,
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.AdminUsers.Add(adminUser);
                await _context.SaveChangesAsync();
                return new ToggleUserAdminResponseDto { IsAdmin = true };
            }
        }

        public async Task<QRGenerationResponseDto> GenerateTableQRAsync(Guid tableId)
        {
            var table = await _context.StudyTables.FindAsync(tableId);
            if (table == null)
                throw new InvalidOperationException("Table not found");

            return new QRGenerationResponseDto
            {
                QrCode = table.QrCode,
                TableNumber = table.TableNumber
            };
        }

        public async Task<string> SetupDataAsync()
        {
            var existingTables = await _context.StudyTables.AnyAsync();
            if (existingTables)
                return "exists";

            var sampleTables = new[]
            {
                new StudyTable { TableNumber = "A1", Location = "Ground Floor", HourlyRate = 5, Capacity = 1, QrCode = $"TABLE_A1_{GenerateRandomString(7)}", IsOccupied = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new StudyTable { TableNumber = "B1", Location = "First Floor", HourlyRate = 8, Capacity = 4, QrCode = $"TABLE_B1_{GenerateRandomString(7)}", IsOccupied = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new StudyTable { TableNumber = "C1", Location = "Second Floor", HourlyRate = 6, Capacity = 2, QrCode = $"TABLE_C1_{GenerateRandomString(7)}", IsOccupied = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            _context.StudyTables.AddRange(sampleTables);

            var premiseCodes = new[]
            {
                new PremiseQrCode { Code = "PREMISE_MAIN_ENTRANCE", Location = "Main Entrance", ValidityHours = 8, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new PremiseQrCode { Code = "PREMISE_LIBRARY_DESK", Location = "Library Reception", ValidityHours = 12, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            _context.PremiseQrCodes.AddRange(premiseCodes);
            await _context.SaveChangesAsync();

            return "created";
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static TransactionWithUserDto MapToTransactionWithUserDto(CreditTransaction transaction)
        {
            return new TransactionWithUserDto
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                User = new UserDto
                {
                    Id = transaction.User.Id,
                    Email = transaction.User.Email,
                    Name = transaction.User.Name,
                    EmailVerified = transaction.User.EmailVerified,
                    CreatedAt = transaction.User.CreatedAt,
                    UpdatedAt = transaction.User.UpdatedAt
                },
                Amount = transaction.Amount,
                Cost = transaction.Cost,
                Status = transaction.Status,
                PaymentMethod = transaction.PaymentMethod,
                TransactionId = transaction.TransactionId,
                ApprovedBy = transaction.ApprovedBy,
                ApprovedAt = transaction.ApprovedAt,
                CreatedAt = transaction.CreatedAt
            };
        }
    }
}