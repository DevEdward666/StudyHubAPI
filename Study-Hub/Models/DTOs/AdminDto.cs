using Study_Hub.Models.DTOs;
using Study_Hub.Models.Entities;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TransactionWithUserDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TableId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public UserDto User { get; set; }
        public StudyTable Tables { get; set; }
        public decimal Amount { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public decimal? Cash { get; set; }
        public decimal? Change { get; set; }
        public RateDto Rates { get; set; }
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
    public class UpdateTableRequestDto
    {
        [Required]
        public string TableID { get; set; }
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
    public class SelectedTableRequestDto
    {
        public string TableID { get; set; }
    }
    public class CreateTableResponseDto
    {
        public Guid TableId { get; set; }
        public string QrCode { get; set; }
    }
    public class UpdateTableResponseDto
    {
        public string TableNumber { get; set; }
        public string QrCode { get; set; }
    }
    public class SelectedTableResponseDto
    {
        public string TableID { get; set; }
        public string TableNumber { get; set; }

        public decimal HourlyRate { get; set; }

        public string Location { get; set; }

        public int Capacity { get; set; }
        public string QrCode { get; set; }
    }
    public class MakeUserAdminRequestDto
    {
        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }
    }

    public class CreateUserRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Role { get; set; }

        public string? Password { get; set; }
    }

    public class CreateUserResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string? Id { get; set; }
        public string? CreatedAt { get; set; }
        public bool? IsAdmin { get; set; }
        public bool? HasActiveSession{ get; set; }
        
    }

    public class UpdateUserRequestDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Role { get; set; }

        public string? Phone { get; set; }
    }

    public class UpdateUserResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string? Phone { get; set; }
        public string? Id { get; set; }
        public string? CreatedAt { get; set; }
        public bool? IsAdmin { get; set; }
        public bool? HasActiveSession{ get; set; }
    }

    public class ChangeUserPasswordRequestDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; }
    }

    public class ChangeUserPasswordResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
    }

    public class AdminAddCreditsRequestDto{
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal Amount { get; set; }

        public string? Notes { get; set; }
    }

    public class AdminAddCreditsResponseDto
    {
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public TransactionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal NewBalance { get; set; }
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