using Study_Hub.Models.DTOs;

namespace StudyHubApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserCreditsDto> InitializeUserCreditsAsync(Guid userId);
        Task<UserCreditsDto?> GetUserCreditsAsync(Guid userId);
        Task<Guid> PurchaseCreditsAsync(Guid userId, PurchaseCreditsRequestDto request);
        Task<List<CreditTransactionDto>> GetUserTransactionsAsync(Guid userId);
        Task<List<SessionWithTableDto>> GetUserSessionsAsync(Guid userId);
    }
}