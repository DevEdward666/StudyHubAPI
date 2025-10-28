using System.ComponentModel.DataAnnotations;

namespace Study_Hub.Models.DTOs
{
    // Get Settings Response
    public class GlobalSettingDto
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DataType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public bool IsEncrypted { get; set; }
        public string? ValidationRegex { get; set; }
        public string? DefaultValue { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? AllowedValues { get; set; }
        public Guid? UpdatedBy { get; set; }
        public string? UpdatedByEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // Create Setting Request
    public class CreateGlobalSettingRequestDto
    {
        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string DataType { get; set; } = "string";

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "general";

        public bool IsPublic { get; set; } = false;

        public bool IsEncrypted { get; set; } = false;

        [StringLength(500)]
        public string? ValidationRegex { get; set; }

        public string? DefaultValue { get; set; }

        public decimal? MinValue { get; set; }

        public decimal? MaxValue { get; set; }

        public string? AllowedValues { get; set; }
    }

    // Update Setting Request
    public class UpdateGlobalSettingRequestDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Value { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ChangeReason { get; set; }
    }

    // Bulk Update Settings Request
    public class BulkUpdateSettingsRequestDto
    {
        [Required]
        public Dictionary<string, string> Settings { get; set; } = new();

        [StringLength(500)]
        public string? ChangeReason { get; set; }
    }

    // Get Settings by Category Request
    public class GetSettingsByCategoryRequestDto
    {
        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;
    }

    // Get Setting by Key Request
    public class GetSettingByKeyRequestDto
    {
        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;
    }

    // Setting History DTO
    public class GlobalSettingHistoryDto
    {
        public Guid Id { get; set; }
        public Guid SettingId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string NewValue { get; set; } = string.Empty;
        public Guid ChangedBy { get; set; }
        public string ChangedByEmail { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? ChangeReason { get; set; }
    }

    // Initialize Default Settings Request
    public class InitializeDefaultSettingsRequestDto
    {
        public bool OverwriteExisting { get; set; } = false;
    }

    // Validate Setting Request
    public class ValidateSettingRequestDto
    {
        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;
    }

    // Validate Setting Response
    public class ValidateSettingResponseDto
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuggestedValue { get; set; }
    }

    // Delete Setting Request
    public class DeleteGlobalSettingRequestDto
    {
        [Required]
        public Guid Id { get; set; }
    }

    // Export Settings Response
    public class ExportSettingsResponseDto
    {
        public Dictionary<string, GlobalSettingDto> Settings { get; set; } = new();
        public DateTime ExportedAt { get; set; }
        public string ExportedBy { get; set; } = string.Empty;
    }

    // Import Settings Request
    public class ImportSettingsRequestDto
    {
        [Required]
        public Dictionary<string, string> Settings { get; set; } = new();

        public bool OverwriteExisting { get; set; } = false;

        [StringLength(500)]
        public string? ImportReason { get; set; }
    }

    // Settings Summary DTO
    public class SettingsSummaryDto
    {
        public int TotalSettings { get; set; }
        public Dictionary<string, int> ByCategory { get; set; } = new();
        public int PublicSettings { get; set; }
        public int EncryptedSettings { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? LastUpdatedBy { get; set; }
    }
}

