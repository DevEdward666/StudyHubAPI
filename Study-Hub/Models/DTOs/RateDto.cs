using System.ComponentModel.DataAnnotations;

namespace Study_Hub.Models.DTOs
{
    public class RateDto
    {
        public Guid Id { get; set; }
        public int Hours { get; set; }
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
        [Range(1, 24, ErrorMessage = "Hours must be between 1 and 24")]
        public int Hours { get; set; }

        [Required]
        [Range(0.01, 10000, ErrorMessage = "Price must be greater than 0")]
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
        [Range(1, 24, ErrorMessage = "Hours must be between 1 and 24")]
        public int Hours { get; set; }

        [Required]
        [Range(0.01, 10000, ErrorMessage = "Price must be greater than 0")]
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

