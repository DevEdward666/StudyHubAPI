using Study_Hub.Models.DTOs;

namespace Study_Hub.Services.Interfaces
{
    public interface IRateService
    {
        Task<List<RateDto>> GetAllRatesAsync();
        Task<List<RateDto>> GetActiveRatesAsync();
        Task<RateDto?> GetRateByIdAsync(Guid id);
        Task<RateDto> CreateRateAsync(Guid adminUserId, CreateRateRequestDto request);
        Task<RateDto> UpdateRateAsync(Guid adminUserId, UpdateRateRequestDto request);
        Task<bool> DeleteRateAsync(Guid id);
        Task<RateDto?> GetRateByHoursAsync(int hours);
    }
}

