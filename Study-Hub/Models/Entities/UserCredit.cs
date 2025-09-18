using Study_Hub.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Study_Hub.Models.Entities
{
    [Table("user_credits")]
    public class UserCredit
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("balance", TypeName = "decimal(10,2)")]
        public decimal Balance { get; set; }

        [Column("total_purchased", TypeName = "decimal(10,2)")]
        public decimal TotalPurchased { get; set; }

        [Column("total_spent", TypeName = "decimal(10,2)")]
        public decimal TotalSpent { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}