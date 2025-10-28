using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Study_Hub.Models.Entities
{
    /// <summary>
    /// Global system settings that can be configured by administrators
    /// Stored as key-value pairs with metadata
    /// </summary>
    [Table("global_settings")]
    public class GlobalSetting
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("key")]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        [Column("value")]
        public string Value { get; set; } = string.Empty;

        [Column("description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column("data_type")]
        [StringLength(50)]
        public string DataType { get; set; } = "string"; // string, number, boolean, json

        [Required]
        [Column("category")]
        [StringLength(100)]
        public string Category { get; set; } = "general"; // general, payment, notification, system, etc.

        [Column("is_public")]
        public bool IsPublic { get; set; } = false; // If true, can be accessed without admin auth

        [Column("is_encrypted")]
        public bool IsEncrypted { get; set; } = false; // For sensitive data like API keys

        [Column("validation_regex")]
        [StringLength(500)]
        public string? ValidationRegex { get; set; }

        [Column("default_value")]
        public string? DefaultValue { get; set; }

        [Column("min_value")]
        public decimal? MinValue { get; set; }

        [Column("max_value")]
        public decimal? MaxValue { get; set; }

        [Column("allowed_values")]
        public string? AllowedValues { get; set; } // JSON array of allowed values

        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UpdatedBy")]
        public virtual User? UpdatedByUser { get; set; }
    }

    /// <summary>
    /// Audit log for settings changes
    /// </summary>
    [Table("global_settings_history")]
    public class GlobalSettingHistory
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("setting_id")]
        public Guid SettingId { get; set; }

        [Required]
        [Column("key")]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;

        [Column("old_value")]
        public string? OldValue { get; set; }

        [Required]
        [Column("new_value")]
        public string NewValue { get; set; } = string.Empty;

        [Required]
        [Column("changed_by")]
        public Guid ChangedBy { get; set; }

        [Column("changed_at")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [Column("change_reason")]
        [StringLength(500)]
        public string? ChangeReason { get; set; }

        // Navigation properties
        [ForeignKey("SettingId")]
        public virtual GlobalSetting Setting { get; set; } = null!;

        [ForeignKey("ChangedBy")]
        public virtual User ChangedByUser { get; set; } = null!;
    }
}

