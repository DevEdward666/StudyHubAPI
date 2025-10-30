﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Study_Hub.Models.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("email")]
        [StringLength(255)]
        public string Email { get; set; }

        [Column("email_verified")]
        public bool EmailVerified { get; set; }

        [Column("name")]
        [StringLength(255)]
        public string? Name { get; set; }

        [Column("image")]
        public string? Image { get; set; }

        [Column("phone")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Column("phone_verified")]
        public bool PhoneVerified { get; set; }

        [Column("is_anonymous")]
        public bool IsAnonymous { get; set; }

        [Column("role")]
        [StringLength(50)]
        public string Role { get; set; } = "Staff";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<AuthAccount> AuthAccounts { get; set; } = new List<AuthAccount>();
        //public virtual ICollection<AuthSession> AuthSessions { get; set; } = new List<AuthSession>();
        public virtual UserCredit? UserCredits { get; set; }
        public virtual AdminUser? AdminUser { get; set; }
        public virtual ICollection<CreditTransaction> CreditTransactions { get; set; } = new List<CreditTransaction>();
        public virtual ICollection<TableSession> TableSessions { get; set; } = new List<TableSession>();
        public virtual ICollection<PremiseActivation> PremiseActivations { get; set; } = new List<PremiseActivation>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}