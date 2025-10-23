using Study_Hub.Models.Entities;
using Study_Hub.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Study_Hub.Models.DTOs
{
    public class StudyTableDto
    {
        public Guid Id { get; set; }
        public string TableNumber { get; set; }
        public string QrCode { get; set; }
        public string? QrCodeImage { get; set; }
        public bool IsOccupied { get; set; }
        public Guid? CurrentUserId { get; set; }
        public decimal HourlyRate { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public DateTime CreatedAt { get; set; }
        public CurrentSessionDto? CurrentSession { get; set; }
    }

// Add this new class
    public class CurrentSessionDto
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class StartSessionRequestDto
    {
        [Required]
        public Guid TableId { get; set; }

        [Required]
        public string? QrCode { get; set; }
        [Required]
        public DateTime endTime { get; set; }
        [Required]
        public int hours { get; set; }
    }

    public class SessionWithTableDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TableId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal CreditsUsed { get; set; }
        public string Status { get; set; }
        public StudyTableDto Table { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EndSessionResponseDto
    {
        public decimal CreditsUsed { get; set; }
        public long Duration { get; set; } // Duration in milliseconds
    }
}