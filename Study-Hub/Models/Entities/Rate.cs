using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Study_Hub.Models.Entities
{
    public enum RateDurationType
    {
        Hourly,
        Daily,
        Weekly,
        Monthly
    }

    [Table("rates")]
    public class Rate
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("hours")]
        public int Hours { get; set; }

        [Column("duration_type")]
        [StringLength(50)]
        public string DurationType { get; set; } = "Hourly"; // Hourly, Daily, Weekly, Monthly

        [Column("duration_value")]
        public int DurationValue { get; set; } = 1; // e.g., 1 day, 2 weeks, 1 month

        [Required]
        [Column("price", TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column("description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public Guid? CreatedBy { get; set; }
    }
}

