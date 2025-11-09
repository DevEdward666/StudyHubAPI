using System.ComponentModel.DataAnnotations;

namespace Study_Hub.Models.DTOs
{
    public class RateDto
    {
        public Guid Id { get; set; }
        public int Hours { get; set; }
        public string DurationType { get; set; } = "Hourly"; // Hourly, Daily, Weekly, Monthly
        public int DurationValue { get; set; } = 1; // e.g., 1, 2, 3
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateRateRequestDto
    {
        [Required]
        [Range(1, 8760, ErrorMessage = "Hours must be between 1 and 8760 (1 year)")]
        public int Hours { get; set; }

        [Required]
        [StringLength(50)]
        public string DurationType { get; set; } = "Hourly"; // Hourly, Daily, Weekly, Monthly

        [Required]
        [Range(1, 365, ErrorMessage = "Duration value must be between 1 and 365")]
        public int DurationValue { get; set; } = 1;

        [Required]
        [Range(0.01, 100000, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;
    }

    public class UpdateRateRequestDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [Range(1, 8760, ErrorMessage = "Hours must be between 1 and 8760 (1 year)")]
        public int Hours { get; set; }

        [Required]
        [StringLength(50)]
        public string DurationType { get; set; } = "Hourly";

        [Required]
        [Range(1, 365, ErrorMessage = "Duration value must be between 1 and 365")]
        public int DurationValue { get; set; } = 1;

        [Required]
        [Range(0.01, 100000, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }
    }

    public class DeleteRateRequestDto
    {
        [Required]
        public Guid Id { get; set; }
    }
}

