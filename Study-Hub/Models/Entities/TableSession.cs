﻿﻿﻿using Study_Hub.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Study_Hub.Models.Entities
{
    public enum SessionStatus
    {
        active,
        completed
    }

    [Table("table_sessions")]
    public class TableSession
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("table_id")]
        public Guid TableId { get; set; }

        [Required]
        [Column("start_time")]
        public DateTime StartTime { get; set; }

        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        [Column("amount", TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = "active";

        [Column("payment_method")]
        public string? PaymentMethod { get; set; }

        [Column("cash", TypeName = "decimal(10,2)")]
        public decimal? Cash { get; set; }

        [Column("change", TypeName = "decimal(10,2)")]
        public decimal? Change { get; set; }

        [Column("rate_id")]
        public Guid? RateId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("TableId")]
        public virtual StudyTable Table { get; set; }

        [ForeignKey("RateId")]
        public virtual Rate? Rate { get; set; }
    }
}