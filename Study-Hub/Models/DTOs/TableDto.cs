﻿﻿﻿﻿﻿using Study_Hub.Models.Entities;
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
        public DateTime? EndTime { get; set; } // Nullable for subscription sessions
        public string? CustomerName { get; set; }
        public bool IsSubscriptionBased { get; set; } // Flag for subscription sessions
        public Guid? SubscriptionId { get; set; } // Link to subscription
        public UserSubscriptionDto? Subscription { get; set; } // Subscription details
    }

    public class StartSessionRequestDto
    {
        [Required]
        public Guid TableId { get; set; }

        [Required]
        public string? QrCode { get; set; }
        [Required]
        public string? userId { get; set; }
        [Required]
        public DateTime endTime { get; set; }
        [Required]
        public int hours { get; set; }
        [Required]
        public decimal amount { get; set; }
        
        public string? PaymentMethod { get; set; }
        
        public decimal? Cash { get; set; }
        
        public decimal? Change { get; set; }
        
        public Guid? RateId { get; set; }
    }

    public class SessionWithTableDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TableId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public StudyTableDto Table { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EndSessionResponseDto
    {
        public Guid SessionId { get; set; }
        public decimal Amount { get; set; }
        public long Duration { get; set; } // Duration in milliseconds
        public double Hours { get; set; }
        public decimal Rate { get; set; }
        public string TableNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal? Cash { get; set; }
        public decimal? Change { get; set; }
    }

    public class ChangeTableRequestDto
    {
        [Required]
        public Guid SessionId { get; set; }

        [Required]
        public Guid NewTableId { get; set; }
    }

    public class ChangeTableResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid NewSessionId { get; set; }
    }
}