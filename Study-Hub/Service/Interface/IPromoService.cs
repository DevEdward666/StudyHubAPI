using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;

namespace Study_Hub.Services.Interfaces
{
    public interface IPromoService
    {
        // Admin operations
        Task<PromoDto> CreatePromoAsync(Guid adminUserId, CreatePromoRequestDto request);
        Task<PromoDto> UpdatePromoAsync(Guid adminUserId, UpdatePromoRequestDto request);
        Task<bool> DeletePromoAsync(Guid promoId);
        Task<PromoDto> TogglePromoStatusAsync(Guid promoId, PromoStatus status);
        Task<List<PromoDto>> GetAllPromosAsync(bool includeInactive = false);
        Task<PromoDto?> GetPromoByIdAsync(Guid promoId);
        Task<PromoDto?> GetPromoByCodeAsync(string code);

        // User operations
        Task<ApplyPromoResponseDto> ValidatePromoAsync(Guid userId, ValidatePromoRequestDto request);
        Task<ApplyPromoResponseDto> ApplyPromoAsync(Guid userId, Guid transactionId, ApplyPromoRequestDto request);

        // Analytics
        Task<List<PromoUsageDto>> GetPromoUsageHistoryAsync(Guid promoId);
        Task<PromoStatisticsDto> GetPromoStatisticsAsync(Guid promoId);
        Task<List<PromoStatisticsDto>> GetAllPromoStatisticsAsync();
    }
}

