using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Study_Hub.Models.DTOs
{
    public class UserWithInfoDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string? Name { get; set; }
        public decimal Credits { get; set; }
        public bool IsAdmin { get; set; }
        public bool HasActiveSession { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TransactionWithUserDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserDto User { get; set; }
        public decimal Amount { get; set; }
        public decimal Cost { get; set; }
        public TransactionStatus Status { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTableRequestDto
    {
        [Required]
        public string TableNumber { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Hourly rate must be positive")]
        public decimal HourlyRate { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be positive")]
        public int Capacity { get; set; }
    }

    public class CreateTableResponseDto
    {
        public Guid TableId { get; set; }
        public string QrCode { get; set; }
    }

    public class MakeUserAdminRequestDto
    {
        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }
    }

    public class ToggleUserAdminRequestDto
    {
        [Required]
        public Guid UserId { get; set; }
    }

    public class ToggleUserAdminResponseDto
    {
        public bool IsAdmin { get; set; }
    }

    public class ApproveTransactionRequestDto
    {
        [Required]
        public Guid TransactionId { get; set; }
    }

    public class QRGenerationResponseDto
    {
        public string QrCode { get; set; }
        public string TableNumber { get; set; }
    }
}