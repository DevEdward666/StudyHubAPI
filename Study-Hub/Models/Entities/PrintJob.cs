using System;
using System.ComponentModel.DataAnnotations;

namespace Study_Hub.Models.Entities
{
    /// <summary>
    /// Represents a print job in the queue for cloud deployment
    /// Used when backend is on Render.com but printer is local
    /// </summary>
    public class PrintJob
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// JSON serialized ReceiptDto
        /// </summary>
        [Required]
        public string ReceiptData { get; set; } = string.Empty;

        /// <summary>
        /// Status: Pending, Processing, Completed, Failed
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// When the print job was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the job was actually printed (null if not yet printed)
        /// </summary>
        public DateTime? PrintedAt { get; set; }

        /// <summary>
        /// Number of times we've tried to print this job
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Error message if print failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Session ID this receipt is for (for tracking)
        /// </summary>
        public Guid? SessionId { get; set; }

        /// <summary>
        /// User ID who initiated the print (for tracking)
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Priority (1-10, higher = more urgent)
        /// </summary>
        public int Priority { get; set; } = 5;
    }
}

