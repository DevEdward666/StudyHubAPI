﻿﻿﻿﻿using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Study_Hub.Data;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using Study_Hub.Services.Interfaces;
using Study_Hub.Hubs;

namespace Study_Hub.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AdminService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<bool> IsAdminAsync(Guid userId)
        {
            return await _context.AdminUsers.AnyAsync(au => au.UserId == userId);
        }

        public async Task<List<UserWithInfoDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.UserCredits)
                    .Include(u => u.TableSessions)
                    .Where(x=> x.Role != "Admin") // Exclude admin users
                    .ToListAsync();

                return users.Select(user => new UserWithInfoDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Credits = user.UserCredits?.Balance ?? 0,
                    IsAdmin = user.AdminUser != null,
                    HasActiveSession = user.TableSessions?.Any(ts => ts.Status == "active") ?? false, // Null check here
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
          
        }

        public async Task<List<TransactionWithUserDto>> GetPendingTransactionsAsync()
        {
            var transactions = await _context.TableSessions
                .Include(ts => ts.User)
                .Include(ts => ts.Table)
                .Include(ts => ts.Rate)
                .Where(ts => ts.Status == "active")
                .OrderByDescending(ts => ts.CreatedAt)
                .ToListAsync();

            return transactions.Select(MapToTransactionWithUserDto).ToList();
        }
    
        public async Task<List<TransactionWithUserDto>> GetAllTableTransactionsAsync()
        {
            var sessions = await _context.TableSessions
                .Include(ts => ts.Table)
                .Include(ts => ts.User)
                .Include(ts => ts.Rate)
                .Where(ts => ts.Status != "active")
                .OrderByDescending(ts => ts.CreatedAt)
                .ToListAsync();

            return sessions.Select(MapToTransactionWithUserDto).ToList();
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

        public async Task<AdminAddCreditsResponseDto> AddApprovedCreditsAsync(Guid adminUserId, AdminAddCreditsRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Verify user exists
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                    throw new InvalidOperationException("User not found");

                // Create a pre-approved transaction
                var creditTransaction = new CreditTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Amount = request.Amount,
                    Cost = request.Amount, // Admin added credits have no cost markup
                    Status = TransactionStatus.Approved,
                    PaymentMethod = "Admin Credit",
                    TransactionId = $"ADMIN_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}",
                    ApprovedBy = adminUserId,
                    ApprovedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.CreditTransactions.Add(creditTransaction);

                // Add credits to user's balance immediately
                var userCredits = await _context.UserCredit
                    .FirstOrDefaultAsync(uc => uc.UserId == request.UserId);

                if (userCredits == null)
                {
                    userCredits = new UserCredit
                    {
                        UserId = request.UserId,
                        Balance = request.Amount,
                        TotalPurchased = request.Amount,
                        TotalSpent = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UserCredit.Add(userCredits);
                }
                else
                {
                    userCredits.Balance += request.Amount;
                    userCredits.TotalPurchased += request.Amount;
                    userCredits.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new AdminAddCreditsResponseDto
                {
                    TransactionId = creditTransaction.Id,
                    Amount = request.Amount,
                    Status = TransactionStatus.Approved,
                    CreatedAt = creditTransaction.CreatedAt,
                    NewBalance = userCredits.Balance
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CreateUserResponseDto> CreateUserAsync(CreateUserRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (existingUser != null)
                {
                    throw new InvalidOperationException("User with this email already exists");
                }

                // Validate role
                var validRoles = new[] { "Staff", "Admin", "Super Admin" };
                if (!validRoles.Contains(request.Role))
                {
                    throw new InvalidOperationException("Invalid role. Must be Staff, Admin, or Super Admin");
                }

                // Create new user
                var newUser = new User
                {
                    Email = request.Email,
                    Name = request.Name,
                    Role = request.Role,
                    EmailVerified = false,
                    PhoneVerified = false,
                    IsAnonymous = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

           

                // If role is Admin or Super Admin, create AdminUser entry
                if (request.Role == "Admin" || request.Role == "Super Admin")
                {
                    var adminUser = new AdminUser
                    {
                        UserId = newUser.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.AdminUsers.Add(adminUser);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                var authAccount = new AuthAccount
                {
                    UserId = newUser.Id,
                    Provider = "password",
                    ProviderId = request.Email,
                    Secret = HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.AuthAccounts.Add(authAccount);
                await _context.SaveChangesAsync();
                return new CreateUserResponseDto
                {
                    UserId = newUser.Id,
                    Email = newUser.Email,
                    Name = newUser.Name ?? "",
                    Role = newUser.Role,
                    IsAdmin = newUser.Role == "Admin" || newUser.Role == "Super Admin"?  true : false,
                    HasActiveSession = false,
                    Id = newUser.Id.ToString(),
                    CreatedAt = newUser.CreatedAt.ToString()
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public async Task<UpdateUserResponseDto> UpdateUserAsync(UpdateUserRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Find existing user
                var user = await _context.Users
                    .Include(u => u.AdminUser)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId);

                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }

                // Check if email is being changed and if it's already taken by another user
                if (user.Email != request.Email)
                {
                    var emailExists = await _context.Users
                        .AnyAsync(u => u.Email == request.Email && u.Id != request.UserId);
                    
                    if (emailExists)
                    {
                        throw new InvalidOperationException("Email is already in use by another user");
                    }
                }

                // Validate role
                var validRoles = new[] { "Staff", "Admin", "Super Admin" };
                if (!validRoles.Contains(request.Role))
                {
                    throw new InvalidOperationException("Invalid role. Must be Staff, Admin, or Super Admin");
                }

                var oldRole = user.Role;
                
                // Update user properties
                user.Email = request.Email;
                user.Name = request.Name;
                user.Role = request.Role;
                user.Phone = request.Phone;
                user.UpdatedAt = DateTime.UtcNow;

                // Handle AdminUser entry based on role change
                var hasAdminEntry = user.AdminUser != null;
                var shouldHaveAdminEntry = request.Role == "Admin" || request.Role == "Super Admin";

                if (shouldHaveAdminEntry && !hasAdminEntry)
                {
                    // Create AdminUser entry
                    var adminUser = new AdminUser
                    {
                        UserId = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.AdminUsers.Add(adminUser);
                }
                else if (!shouldHaveAdminEntry && hasAdminEntry)
                {
                    // Remove AdminUser entry
                    _context.AdminUsers.Remove(user.AdminUser);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new UpdateUserResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Name = user.Name ?? "",
                    Role = user.Role,
                    Phone = user.Phone,
                    IsAdmin = user.Role == "Admin" || user.Role == "Super Admin"?  true : false,
                    HasActiveSession = false,
                    Id = user.Id.ToString(),
                    CreatedAt = user.CreatedAt.ToString()
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
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

        private static TransactionWithUserDto MapToTransactionWithUserDto(TableSession transaction)
        {
            return new TransactionWithUserDto
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                TableId = transaction.TableId,
                StartTime = transaction.StartTime,
                EndTime = transaction.EndTime,
                User = new UserDto
                {
                    Id = transaction.User.Id,
                    Email = transaction.User.Email,
                    Name = transaction.User.Name,
                    EmailVerified = transaction.User.EmailVerified,
                    CreatedAt = transaction.User.CreatedAt,
                    UpdatedAt = transaction.User.UpdatedAt,
                    Role = transaction.User.Role,
                },
                Tables = transaction.Table != null ? new StudyTable
                {
                    Id = transaction.Table.Id,
                    TableNumber = transaction.Table.TableNumber,
                    QrCode = transaction.Table.QrCode,
                    QrCodeImage = transaction.Table.QrCodeImage,
                    IsOccupied = transaction.Table.IsOccupied,
                    CurrentUserId = transaction.Table.CurrentUserId,
                    HourlyRate = transaction.Table.HourlyRate,
                    Location = transaction.Table.Location,
                    Capacity = transaction.Table.Capacity,
                    CreatedAt = transaction.Table.CreatedAt
                } : null,
                Amount = transaction.Amount,
                Cost = transaction.Amount,
                Status = transaction.Status,
                PaymentMethod = transaction.PaymentMethod,
                Cash = transaction.Cash,
                Change = transaction.Change,
                Rates = new RateDto()
                {
                    Id = transaction.Rate != null ? transaction.Rate.Id : Guid.Empty,
                    Hours = transaction.Rate != null ? transaction.Rate.Hours : 0,
                    Description = transaction.Rate != null ? transaction.Rate.Description : "",
                    CreatedAt = transaction.Rate != null ? transaction.Rate.CreatedAt : DateTime.MinValue
                },
                CreatedAt = transaction.CreatedAt
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
                Table = new StudyTableDto
                {
                    Id = session.Table.Id,
                    TableNumber = session.Table.TableNumber,
                    QrCode = session.Table.QrCode,
                    QrCodeImage = session.Table.QrCodeImage,
                    IsOccupied = session.Table.IsOccupied,
                    CurrentUserId = session.Table.CurrentUserId,
                    HourlyRate = session.Table.HourlyRate,
                    Location = session.Table.Location,
                    Capacity = session.Table.Capacity,
                    CreatedAt = session.Table.CreatedAt
                },
                CreatedAt = session.CreatedAt
            };
        }

        public async Task<string> SendTestNotificationAsync()
        {
            try
            {
                var testNotification = new
                {
                    id = Guid.NewGuid().ToString(),
                    sessionId = Guid.NewGuid().ToString(),
                    tableId = Guid.NewGuid().ToString(),
                    tableNumber = "TEST-01",
                    userName = "Test User",
                    message = "This is a test notification from SignalR",
                    duration = 2.5,
                    amount = 125.50m,
                    createdAt = DateTime.UtcNow.ToString("o")
                };

                await _hubContext.Clients.Group("admins").SendAsync("SessionEnded", testNotification);
                
                return "Test notification sent successfully to all admins";
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send test notification: {ex.Message}");
            }
        }
    }

}