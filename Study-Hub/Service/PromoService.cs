using Microsoft.EntityFrameworkCore;
using Study_Hub.Data;
using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using Study_Hub.Services.Interfaces;

namespace Study_Hub.Service
{
    public class PromoService : IPromoService
    {
        private readonly ApplicationDbContext _context;

        public PromoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ADMIN OPERATIONS

        public async Task<PromoDto> CreatePromoAsync(Guid adminUserId, CreatePromoRequestDto request)
        {
            // Validate promo code uniqueness
            var existingPromo = await _context.Promos
                .FirstOrDefaultAsync(p => p.Code.ToLower() == request.Code.ToLower() && !p.IsDeleted);
            
            if (existingPromo != null)
                throw new InvalidOperationException($"Promo code '{request.Code}' already exists");

            // Validate type-specific fields
            ValidatePromoFields(request.Type, request.PercentageBonus, request.FixedBonusAmount, 
                request.BuyAmount, request.GetAmount);

            // Determine initial status
            var status = PromoStatus.Active;
            if (request.StartDate.HasValue && request.StartDate.Value > DateTime.UtcNow)
                status = PromoStatus.Scheduled;
            else if (request.EndDate.HasValue && request.EndDate.Value < DateTime.UtcNow)
                status = PromoStatus.Expired;

            // Ensure DateTimes are UTC for PostgreSQL
            DateTime? startDateUtc = request.StartDate.HasValue 
                ? DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc) 
                : null;
            DateTime? endDateUtc = request.EndDate.HasValue 
                ? DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc) 
                : null;

            var promo = new Promo
            {
                Id = Guid.NewGuid(),
                Code = request.Code.ToUpper(),
                Name = request.Name,
                Description = request.Description,
                Type = request.Type,
                Status = status,
                PercentageBonus = request.PercentageBonus,
                FixedBonusAmount = request.FixedBonusAmount,
                BuyAmount = request.BuyAmount,
                GetAmount = request.GetAmount,
                MinPurchaseAmount = request.MinPurchaseAmount,
                MaxDiscountAmount = request.MaxDiscountAmount,
                UsageLimit = request.UsageLimit,
                UsagePerUser = request.UsagePerUser,
                StartDate = startDateUtc,
                EndDate = endDateUtc,
                CreatedBy = adminUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Promos.Add(promo);
            await _context.SaveChangesAsync();

            return await MapToPromoDto(promo);
        }

        public async Task<PromoDto> UpdatePromoAsync(Guid adminUserId, UpdatePromoRequestDto request)
        {
            var promo = await _context.Promos.FindAsync(request.PromoId);
            if (promo == null || promo.IsDeleted)
                throw new InvalidOperationException("Promo not found");

            // Update fields if provided
            if (request.Name != null)
                promo.Name = request.Name;

            if (request.Description != null)
                promo.Description = request.Description;

            if (request.Status.HasValue)
                promo.Status = request.Status.Value;

            if (request.PercentageBonus.HasValue)
                promo.PercentageBonus = request.PercentageBonus;

            if (request.FixedBonusAmount.HasValue)
                promo.FixedBonusAmount = request.FixedBonusAmount;

            if (request.BuyAmount.HasValue)
                promo.BuyAmount = request.BuyAmount;

            if (request.GetAmount.HasValue)
                promo.GetAmount = request.GetAmount;

            if (request.MinPurchaseAmount.HasValue)
                promo.MinPurchaseAmount = request.MinPurchaseAmount;

            if (request.MaxDiscountAmount.HasValue)
                promo.MaxDiscountAmount = request.MaxDiscountAmount;

            if (request.UsageLimit.HasValue)
                promo.UsageLimit = request.UsageLimit;

            if (request.UsagePerUser.HasValue)
                promo.UsagePerUser = request.UsagePerUser;

            if (request.StartDate.HasValue)
                promo.StartDate = DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc);

            if (request.EndDate.HasValue)
                promo.EndDate = DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc);

            promo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await MapToPromoDto(promo);
        }

        public async Task<bool> DeletePromoAsync(Guid promoId)
        {
            var promo = await _context.Promos.FindAsync(promoId);
            if (promo == null || promo.IsDeleted)
                return false;

            // Soft delete
            promo.IsDeleted = true;
            promo.Status = PromoStatus.Inactive;
            promo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PromoDto> TogglePromoStatusAsync(Guid promoId, PromoStatus status)
        {
            var promo = await _context.Promos.FindAsync(promoId);
            if (promo == null || promo.IsDeleted)
                throw new InvalidOperationException("Promo not found");

            promo.Status = status;
            promo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await MapToPromoDto(promo);
        }

        public async Task<List<PromoDto>> GetAllPromosAsync(bool includeInactive = false)
        {
            var query = _context.Promos
                .Include(p => p.Creator)
                .Where(p => !p.IsDeleted);

            if (!includeInactive)
                query = query.Where(p => p.Status == PromoStatus.Active || p.Status == PromoStatus.Scheduled);

            var promos = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var promoDtos = new List<PromoDto>();
            foreach (var promo in promos)
            {
                promoDtos.Add(await MapToPromoDto(promo));
            }

            return promoDtos;
        }

        public async Task<PromoDto?> GetPromoByIdAsync(Guid promoId)
        {
            var promo = await _context.Promos
                .Include(p => p.Creator)
                .FirstOrDefaultAsync(p => p.Id == promoId && !p.IsDeleted);

            if (promo == null)
                return null;

            return await MapToPromoDto(promo);
        }

        public async Task<PromoDto?> GetPromoByCodeAsync(string code)
        {
            var promo = await _context.Promos
                .Include(p => p.Creator)
                .FirstOrDefaultAsync(p => p.Code.ToLower() == code.ToLower() && !p.IsDeleted);

            if (promo == null)
                return null;

            return await MapToPromoDto(promo);
        }

        // USER OPERATIONS

        public async Task<ApplyPromoResponseDto> ValidatePromoAsync(Guid userId, ValidatePromoRequestDto request)
        {
            var promo = await _context.Promos
                .FirstOrDefaultAsync(p => p.Code.ToLower() == request.PromoCode.ToLower() && !p.IsDeleted);

            if (promo == null)
            {
                return new ApplyPromoResponseDto
                {
                    IsValid = false,
                    Message = "Invalid promo code",
                    OriginalAmount = request.PurchaseAmount,
                    BonusAmount = 0,
                    TotalAmount = request.PurchaseAmount
                };
            }

            // Validate promo
            var validation = await ValidatePromoForUser(promo, userId, request.PurchaseAmount);
            if (!validation.IsValid)
            {
                return new ApplyPromoResponseDto
                {
                    IsValid = false,
                    Message = validation.Message,
                    OriginalAmount = request.PurchaseAmount,
                    BonusAmount = 0,
                    TotalAmount = request.PurchaseAmount
                };
            }

            // Calculate bonus
            var bonusAmount = CalculateBonusAmount(promo, request.PurchaseAmount);

            return new ApplyPromoResponseDto
            {
                IsValid = true,
                Message = "Promo code is valid",
                OriginalAmount = request.PurchaseAmount,
                BonusAmount = bonusAmount,
                TotalAmount = request.PurchaseAmount + bonusAmount,
                PromoDetails = await MapToPromoDto(promo)
            };
        }

        public async Task<ApplyPromoResponseDto> ApplyPromoAsync(Guid userId, Guid transactionId, ApplyPromoRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var promo = await _context.Promos
                    .FirstOrDefaultAsync(p => p.Code.ToLower() == request.PromoCode.ToLower() && !p.IsDeleted);

                if (promo == null)
                {
                    return new ApplyPromoResponseDto
                    {
                        IsValid = false,
                        Message = "Invalid promo code",
                        OriginalAmount = request.PurchaseAmount,
                        BonusAmount = 0,
                        TotalAmount = request.PurchaseAmount
                    };
                }

                // Validate promo
                var validation = await ValidatePromoForUser(promo, userId, request.PurchaseAmount);
                if (!validation.IsValid)
                {
                    return new ApplyPromoResponseDto
                    {
                        IsValid = false,
                        Message = validation.Message,
                        OriginalAmount = request.PurchaseAmount,
                        BonusAmount = 0,
                        TotalAmount = request.PurchaseAmount
                    };
                }

                // Calculate bonus
                var bonusAmount = CalculateBonusAmount(promo, request.PurchaseAmount);

                // Record usage
                var promoUsage = new PromoUsage
                {
                    Id = Guid.NewGuid(),
                    PromoId = promo.Id,
                    UserId = userId,
                    TransactionId = transactionId,
                    PurchaseAmount = request.PurchaseAmount,
                    BonusAmount = bonusAmount,
                    UsedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PromoUsages.Add(promoUsage);

                // Update promo usage count
                promo.CurrentUsageCount++;
                promo.UpdatedAt = DateTime.UtcNow;

                // Update user credits with bonus
                var userCredits = await _context.UserCredit
                    .FirstOrDefaultAsync(uc => uc.UserId == userId);

                if (userCredits != null)
                {
                    userCredits.Balance += bonusAmount;
                    userCredits.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApplyPromoResponseDto
                {
                    IsValid = true,
                    Message = "Promo applied successfully",
                    OriginalAmount = request.PurchaseAmount,
                    BonusAmount = bonusAmount,
                    TotalAmount = request.PurchaseAmount + bonusAmount,
                    PromoDetails = await MapToPromoDto(promo)
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Failed to apply promo: {ex.Message}");
            }
        }

        // ANALYTICS

        public async Task<List<PromoUsageDto>> GetPromoUsageHistoryAsync(Guid promoId)
        {
            var usages = await _context.PromoUsages
                .Include(pu => pu.Promo)
                .Include(pu => pu.User)
                .Where(pu => pu.PromoId == promoId)
                .OrderByDescending(pu => pu.UsedAt)
                .ToListAsync();

            return usages.Select(u => new PromoUsageDto
            {
                Id = u.Id,
                PromoId = u.PromoId,
                PromoCode = u.Promo.Code,
                PromoName = u.Promo.Name,
                UserId = u.UserId,
                UserEmail = u.User.Email,
                TransactionId = u.TransactionId,
                PurchaseAmount = u.PurchaseAmount,
                BonusAmount = u.BonusAmount,
                UsedAt = u.UsedAt
            }).ToList();
        }

        public async Task<PromoStatisticsDto> GetPromoStatisticsAsync(Guid promoId)
        {
            var promo = await _context.Promos.FindAsync(promoId);
            if (promo == null || promo.IsDeleted)
                throw new InvalidOperationException("Promo not found");

            var usages = await _context.PromoUsages
                .Where(pu => pu.PromoId == promoId)
                .ToListAsync();

            return new PromoStatisticsDto
            {
                PromoId = promo.Id,
                Code = promo.Code,
                Name = promo.Name,
                TotalUsageCount = usages.Count,
                UniqueUsersCount = usages.Select(u => u.UserId).Distinct().Count(),
                TotalBonusGiven = usages.Sum(u => u.BonusAmount),
                TotalPurchaseAmount = usages.Sum(u => u.PurchaseAmount),
                LastUsedAt = usages.OrderByDescending(u => u.UsedAt).FirstOrDefault()?.UsedAt
            };
        }

        public async Task<List<PromoStatisticsDto>> GetAllPromoStatisticsAsync()
        {
            var promos = await _context.Promos
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            var statistics = new List<PromoStatisticsDto>();

            foreach (var promo in promos)
            {
                statistics.Add(await GetPromoStatisticsAsync(promo.Id));
            }

            return statistics.OrderByDescending(s => s.TotalUsageCount).ToList();
        }

        // HELPER METHODS

        private async Task<(bool IsValid, string Message)> ValidatePromoForUser(Promo promo, Guid userId, decimal purchaseAmount)
        {
            // Check status
            if (promo.Status != PromoStatus.Active)
                return (false, $"Promo is {promo.Status.ToString().ToLower()}");

            // Check date range
            if (promo.StartDate.HasValue && DateTime.UtcNow < promo.StartDate.Value)
                return (false, $"Promo starts on {promo.StartDate.Value:yyyy-MM-dd}");

            if (promo.EndDate.HasValue && DateTime.UtcNow > promo.EndDate.Value)
            {
                // Auto-expire
                promo.Status = PromoStatus.Expired;
                await _context.SaveChangesAsync();
                return (false, "Promo has expired");
            }

            // Check minimum purchase
            if (promo.MinPurchaseAmount.HasValue && purchaseAmount < promo.MinPurchaseAmount.Value)
                return (false, $"Minimum purchase amount is {promo.MinPurchaseAmount.Value:F2} credits");

            // Check usage limit
            if (promo.UsageLimit.HasValue && promo.CurrentUsageCount >= promo.UsageLimit.Value)
                return (false, "Promo usage limit reached");

            // Check per-user limit
            if (promo.UsagePerUser.HasValue)
            {
                var userUsageCount = await _context.PromoUsages
                    .CountAsync(pu => pu.PromoId == promo.Id && pu.UserId == userId);

                if (userUsageCount >= promo.UsagePerUser.Value)
                    return (false, "You have reached the usage limit for this promo");
            }

            return (true, "Valid");
        }

        private decimal CalculateBonusAmount(Promo promo, decimal purchaseAmount)
        {
            decimal bonusAmount = 0;

            switch (promo.Type)
            {
                case PromoType.Percentage:
                    if (promo.PercentageBonus.HasValue)
                    {
                        bonusAmount = purchaseAmount * (promo.PercentageBonus.Value / 100);
                    }
                    break;

                case PromoType.FixedAmount:
                    bonusAmount = promo.FixedBonusAmount ?? 0;
                    break;

                case PromoType.BuyXGetY:
                    if (promo.BuyAmount.HasValue && promo.GetAmount.HasValue)
                    {
                        // Check if purchase amount meets the buy requirement
                        if (purchaseAmount >= promo.BuyAmount.Value)
                        {
                            // Calculate how many times the promotion can be applied
                            var multiplier = Math.Floor(purchaseAmount / promo.BuyAmount.Value);
                            bonusAmount = promo.GetAmount.Value * multiplier;
                        }
                    }
                    break;
            }

            // Apply max discount cap if set
            if (promo.MaxDiscountAmount.HasValue && bonusAmount > promo.MaxDiscountAmount.Value)
            {
                bonusAmount = promo.MaxDiscountAmount.Value;
            }

            return Math.Round(bonusAmount, 2);
        }

        private void ValidatePromoFields(PromoType type, decimal? percentage, decimal? fixedAmount, 
            decimal? buyAmount, decimal? getAmount)
        {
            switch (type)
            {
                case PromoType.Percentage:
                    if (!percentage.HasValue || percentage.Value <= 0)
                        throw new InvalidOperationException("Percentage bonus is required for Percentage promo type");
                    break;

                case PromoType.FixedAmount:
                    if (!fixedAmount.HasValue || fixedAmount.Value <= 0)
                        throw new InvalidOperationException("Fixed bonus amount is required for FixedAmount promo type");
                    break;

                case PromoType.BuyXGetY:
                    if (!buyAmount.HasValue || buyAmount.Value <= 0)
                        throw new InvalidOperationException("Buy amount is required for BuyXGetY promo type");
                    if (!getAmount.HasValue || getAmount.Value <= 0)
                        throw new InvalidOperationException("Get amount is required for BuyXGetY promo type");
                    break;
            }
        }

        private async Task<PromoDto> MapToPromoDto(Promo promo)
        {
            if (promo.Creator == null)
            {
                await _context.Entry(promo).Reference(p => p.Creator).LoadAsync();
            }

            return new PromoDto
            {
                Id = promo.Id,
                Code = promo.Code,
                Name = promo.Name,
                Description = promo.Description,
                Type = promo.Type,
                Status = promo.Status,
                PercentageBonus = promo.PercentageBonus,
                FixedBonusAmount = promo.FixedBonusAmount,
                BuyAmount = promo.BuyAmount,
                GetAmount = promo.GetAmount,
                MinPurchaseAmount = promo.MinPurchaseAmount,
                MaxDiscountAmount = promo.MaxDiscountAmount,
                UsageLimit = promo.UsageLimit,
                UsagePerUser = promo.UsagePerUser,
                CurrentUsageCount = promo.CurrentUsageCount,
                StartDate = promo.StartDate,
                EndDate = promo.EndDate,
                CreatedAt = promo.CreatedAt,
                UpdatedAt = promo.UpdatedAt,
                CreatorEmail = promo.Creator?.Email
            };
        }
    }
}

