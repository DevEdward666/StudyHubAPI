using Study_Hub.Models.DTOs;

namespace Study_Hub.Services.Interfaces
{
    public interface IGlobalSettingsService
    {
        // CRUD Operations
        Task<GlobalSettingDto> CreateSettingAsync(Guid adminUserId, CreateGlobalSettingRequestDto request);
        Task<GlobalSettingDto> UpdateSettingAsync(Guid adminUserId, UpdateGlobalSettingRequestDto request);
        Task<bool> DeleteSettingAsync(Guid settingId);
        Task<GlobalSettingDto?> GetSettingByIdAsync(Guid settingId);
        Task<GlobalSettingDto?> GetSettingByKeyAsync(string key);
        Task<List<GlobalSettingDto>> GetAllSettingsAsync();
        Task<List<GlobalSettingDto>> GetSettingsByCategoryAsync(string category);
        Task<List<GlobalSettingDto>> GetPublicSettingsAsync();

        // Bulk Operations
        Task<Dictionary<string, GlobalSettingDto>> BulkUpdateSettingsAsync(Guid adminUserId, BulkUpdateSettingsRequestDto request);
        Task<bool> InitializeDefaultSettingsAsync(Guid adminUserId, bool overwriteExisting = false);

        // History
        Task<List<GlobalSettingHistoryDto>> GetSettingHistoryAsync(Guid settingId);
        Task<List<GlobalSettingHistoryDto>> GetRecentChangesAsync(int count = 50);

        // Validation
        Task<ValidateSettingResponseDto> ValidateSettingAsync(ValidateSettingRequestDto request);

        // Import/Export
        Task<ExportSettingsResponseDto> ExportSettingsAsync(string? category = null);
        Task<Dictionary<string, GlobalSettingDto>> ImportSettingsAsync(Guid adminUserId, ImportSettingsRequestDto request);

        // Summary & Analytics
        Task<SettingsSummaryDto> GetSettingsSummaryAsync();

        // Helper Methods
        Task<string?> GetSettingValueAsync(string key);
        Task<T?> GetSettingValueAsync<T>(string key);
        Task<bool> SetSettingValueAsync(Guid adminUserId, string key, string value, string? changeReason = null);
    }
}

