using Study_Hub.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Study_Hub.Models.Entities
{
    public enum TransactionStatus
    {
        Pending,
        Approved,
        Rejected
    }

    [Table("credit_transactions")]
    public class CreditTransaction
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("amount", TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column("cost", TypeName = "decimal(10,2)")]
        public decimal Cost { get; set; }

        [Required]
        [Column("status")]
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        [Required]
        [Column("payment_method")]
        [StringLength(100)]
        public string PaymentMethod { get; set; }

        [Required]
        [Column("transaction_id")]
        [StringLength(255)]
        public string TransactionId { get; set; }

        [Column("approved_by")]
        public Guid? ApprovedBy { get; set; }

        [Column("approved_at")]
        public DateTime? ApprovedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ApprovedBy")]
        public virtual User? ApprovedByUser { get; set; }
    }
}