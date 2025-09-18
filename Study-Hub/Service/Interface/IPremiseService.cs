using Study_Hub.Models.DTOs;

namespace StudyHubApi.Services.Interfaces
{
    public interface IPremiseService
    {
        Task<List<PremiseQrCodeDto>> GetPremiseQRCodesAsync();
        Task<CreatePremiseResponseDto> CreatePremiseQRCodeAsync(CreatePremiseQRRequestDto request);
        Task<PremiseAccessDto> ActivatePremiseAccessAsync(Guid userId, ActivatePremiseRequestDto request);
        Task<ActivePremiseAccessDto?> CheckPremiseAccessAsync(Guid userId);
        Task CleanupExpiredAccessAsync(Guid userId);
    }
}