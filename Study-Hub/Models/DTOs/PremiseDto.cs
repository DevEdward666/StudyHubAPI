using System.ComponentModel.DataAnnotations;

namespace Study_Hub.Models.DTOs
{
    public class PremiseQrCodeDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; }
        public int ValidityHours { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePremiseQRRequestDto
    {
        [Required]
        public string Location { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Validity hours must be positive")]
        public int ValidityHours { get; set; }
    }

    public class CreatePremiseResponseDto
    {
        public Guid PremiseId { get; set; }
        public string Code { get; set; }
    }

    public class ActivatePremiseRequestDto
    {
        [Required]
        public string ActivationCode { get; set; }
    }

    public class PremiseAccessDto
    {
        public bool Success { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Location { get; set; }
        public int ValidityHours { get; set; }
    }

    public class ActivePremiseAccessDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime ActivatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public string ActivationCode { get; set; }
        public string? Location { get; set; }
        public long TimeRemaining { get; set; } // Milliseconds remaining
    }
}