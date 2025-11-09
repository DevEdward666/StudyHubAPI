using Microsoft.EntityFrameworkCore;
using Study_Hub.Data;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using StudyHubApi.Services.Interfaces;

namespace StudyHubApi.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Package Management

        public async Task<List<SubscriptionPackageDto>> GetAllPackagesAsync()
        {
            var packages = await _context.SubscriptionPackages
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            return packages.Select(MapToPackageDto).ToList();
        }

        public async Task<List<SubscriptionPackageDto>> GetActivePackagesAsync()
        {
            var packages = await _context.SubscriptionPackages
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            return packages.Select(MapToPackageDto).ToList();
        }

        public async Task<SubscriptionPackageDto?> GetPackageByIdAsync(Guid packageId)
        {
            var package = await _context.SubscriptionPackages.FindAsync(packageId);
            return package != null ? MapToPackageDto(package) : null;
        }

        public async Task<SubscriptionPackageDto> CreatePackageAsync(CreateSubscriptionPackageDto dto, Guid createdBy)
        {
            var package = new SubscriptionPackage
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                PackageType = dto.PackageType,
                DurationValue = dto.DurationValue,
                TotalHours = dto.TotalHours,
                Price = dto.Price,
                Description = dto.Description,
                DisplayOrder = dto.DisplayOrder,
                IsActive = true,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SubscriptionPackages.Add(package);
            await _context.SaveChangesAsync();

            return MapToPackageDto(package);
        }

        public async Task<SubscriptionPackageDto> UpdatePackageAsync(Guid packageId, UpdateSubscriptionPackageDto dto)
        {
            var package = await _context.SubscriptionPackages.FindAsync(packageId);
            if (package == null)
                throw new InvalidOperationException("Package not found");

            if (dto.Name != null) package.Name = dto.Name;
            if (dto.Price.HasValue) package.Price = dto.Price.Value;
            if (dto.Description != null) package.Description = dto.Description;
            if (dto.IsActive.HasValue) package.IsActive = dto.IsActive.Value;
            if (dto.DisplayOrder.HasValue) package.DisplayOrder = dto.DisplayOrder.Value;

            package.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToPackageDto(package);
        }

        public async Task<bool> DeletePackageAsync(Guid packageId)
        {
            var package = await _context.SubscriptionPackages.FindAsync(packageId);
            if (package == null)
                return false;

            // Soft delete - just mark as inactive
            package.IsActive = false;
            package.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region User Subscription Management

        public async Task<List<UserSubscriptionDto>> GetUserSubscriptionsAsync(Guid userId)
        {
            var subscriptions = await _context.UserSubscriptions
                .Include(s => s.Package)
                .Include(s => s.User)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return subscriptions.Select(MapToSubscriptionDto).ToList();
        }

        public async Task<List<UserSubscriptionDto>> GetActiveUserSubscriptionsAsync(Guid userId)
        {
            var subscriptions = await _context.UserSubscriptions
                .Include(s => s.Package)
                .Include(s => s.User)
                .Where(s => s.UserId == userId && s.Status == "Active" && s.RemainingHours > 0)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return subscriptions.Select(MapToSubscriptionDto).ToList();
        }

        public async Task<UserSubscriptionDto?> GetSubscriptionByIdAsync(Guid subscriptionId)
        {
            var subscription = await _context.UserSubscriptions
                .Include(s => s.Package)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);

            return subscription != null ? MapToSubscriptionDto(subscription) : null;
        }

        public async Task<UserSubscriptionDto> PurchaseSubscriptionAsync(Guid userId, PurchaseSubscriptionDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var package = await _context.SubscriptionPackages.FindAsync(dto.PackageId);
                if (package == null || !package.IsActive)
                    throw new InvalidOperationException("Package not found or not active");

                var subscription = new UserSubscription
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PackageId = dto.PackageId,
                    TotalHours = package.TotalHours,
                    RemainingHours = package.TotalHours,
                    HoursUsed = 0,
                    PurchaseDate = DateTime.UtcNow,
                    Status = "Active",
                    PurchaseAmount = package.Price,
                    PaymentMethod = dto.PaymentMethod,
                    TransactionReference = dto.TransactionReference,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserSubscriptions.Add(subscription);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Reload with navigation properties
                subscription = await _context.UserSubscriptions
                    .Include(s => s.Package)
                    .Include(s => s.User)
                    .FirstAsync(s => s.Id == subscription.Id);

                return MapToSubscriptionDto(subscription);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<UserSubscriptionDto> AdminPurchaseSubscriptionAsync(AdminPurchaseSubscriptionDto dto, Guid adminId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var package = await _context.SubscriptionPackages.FindAsync(dto.PackageId);
                if (package == null || !package.IsActive)
                    throw new InvalidOperationException("Package not found or not active");

                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                    throw new InvalidOperationException("User not found");

                var subscription = new UserSubscription
                {
                    Id = Guid.NewGuid(),
                    UserId = dto.UserId,
                    PackageId = dto.PackageId,
                    TotalHours = package.TotalHours,
                    RemainingHours = package.TotalHours,
                    HoursUsed = 0,
                    PurchaseDate = DateTime.UtcNow,
                    Status = "Active",
                    PurchaseAmount = package.Price,
                    PaymentMethod = dto.PaymentMethod,
                    TransactionReference = dto.TransactionReference,
                    Notes = dto.Notes,
                    CreatedBy = adminId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserSubscriptions.Add(subscription);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Reload with navigation properties
                subscription = await _context.UserSubscriptions
                    .Include(s => s.Package)
                    .Include(s => s.User)
                    .FirstAsync(s => s.Id == subscription.Id);

                return MapToSubscriptionDto(subscription);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CancelSubscriptionAsync(Guid subscriptionId, Guid userId)
        {
            var subscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId);

            if (subscription == null)
                return false;

            subscription.Status = "Cancelled";
            subscription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SubscriptionUsageDto> GetSubscriptionUsageAsync(Guid subscriptionId)
        {
            var subscription = await _context.UserSubscriptions.FindAsync(subscriptionId);
            if (subscription == null)
                throw new InvalidOperationException("Subscription not found");

            var lastSession = await _context.TableSessions
                .Where(s => s.SubscriptionId == subscriptionId)
                .OrderByDescending(s => s.StartTime)
                .FirstOrDefaultAsync();

            return new SubscriptionUsageDto
            {
                SubscriptionId = subscription.Id,
                HoursConsumed = subscription.HoursUsed,
                RemainingHours = subscription.RemainingHours,
                SessionStartTime = lastSession?.StartTime ?? subscription.CreatedAt,
                SessionEndTime = lastSession?.EndTime
            };
        }

        #endregion

        #region Admin Functions

        public async Task<List<UserSubscriptionWithUserDto>> GetAllUserSubscriptionsAsync()
        {
            var subscriptions = await _context.UserSubscriptions
                .Include(s => s.Package)
                .Include(s => s.User)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return subscriptions.Select(MapToSubscriptionWithUserDto).ToList();
        }

        public async Task<List<UserSubscriptionWithUserDto>> GetSubscriptionsByStatusAsync(string status)
        {
            var subscriptions = await _context.UserSubscriptions
                .Include(s => s.Package)
                .Include(s => s.User)
                .Where(s => s.Status.ToLower() == status.ToLower())
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return subscriptions.Select(MapToSubscriptionWithUserDto).ToList();
        }

        #endregion

        #region Mapping Methods

        private SubscriptionPackageDto MapToPackageDto(SubscriptionPackage package)
        {
            return new SubscriptionPackageDto
            {
                Id = package.Id,
                Name = package.Name,
                PackageType = package.PackageType,
                DurationValue = package.DurationValue,
                TotalHours = package.TotalHours,
                Price = package.Price,
                Description = package.Description,
                IsActive = package.IsActive,
                DisplayOrder = package.DisplayOrder,
                CreatedAt = package.CreatedAt,
                UpdatedAt = package.UpdatedAt
            };
        }

        private UserSubscriptionDto MapToSubscriptionDto(UserSubscription subscription)
        {
            var percentageUsed = subscription.TotalHours > 0
                ? (subscription.HoursUsed / subscription.TotalHours) * 100
                : 0;

            return new UserSubscriptionDto
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                PackageId = subscription.PackageId,
                PackageName = subscription.Package.Name,
                PackageType = subscription.Package.PackageType,
                TotalHours = subscription.TotalHours,
                RemainingHours = subscription.RemainingHours,
                HoursUsed = subscription.HoursUsed,
                PercentageUsed = percentageUsed,
                PurchaseDate = subscription.PurchaseDate,
                ActivationDate = subscription.ActivationDate,
                ExpiryDate = subscription.ExpiryDate,
                Status = subscription.Status,
                PurchaseAmount = subscription.PurchaseAmount,
                PaymentMethod = subscription.PaymentMethod,
                TransactionReference = subscription.TransactionReference,
                Notes = subscription.Notes,
                CreatedAt = subscription.CreatedAt,
                UpdatedAt = subscription.UpdatedAt
            };
        }

        private UserSubscriptionWithUserDto MapToSubscriptionWithUserDto(UserSubscription subscription)
        {
            var dto = MapToSubscriptionDto(subscription);
            
            return new UserSubscriptionWithUserDto
            {
                Id = dto.Id,
                UserId = dto.UserId,
                PackageId = dto.PackageId,
                PackageName = dto.PackageName,
                PackageType = dto.PackageType,
                TotalHours = dto.TotalHours,
                RemainingHours = dto.RemainingHours,
                HoursUsed = dto.HoursUsed,
                PercentageUsed = dto.PercentageUsed,
                PurchaseDate = dto.PurchaseDate,
                ActivationDate = dto.ActivationDate,
                ExpiryDate = dto.ExpiryDate,
                Status = dto.Status,
                PurchaseAmount = dto.PurchaseAmount,
                PaymentMethod = dto.PaymentMethod,
                TransactionReference = dto.TransactionReference,
                Notes = dto.Notes,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                User = new UserDto
                {
                    Id = subscription.User.Id,
                    Email = subscription.User.Email,
                    Name = subscription.User.Name,
                    Role = subscription.User.Role,
                    EmailVerified = subscription.User.EmailVerified,
                    Image = subscription.User.Image,
                    CreatedAt = subscription.User.CreatedAt,
                    UpdatedAt = subscription.User.UpdatedAt
                }
            };
        }

        #endregion
    }
}

