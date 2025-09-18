using Study_Hub.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Study_Hub.Models.Entities
{
    [Table("auth_accounts")]
    public class AuthAccount
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("provider")]
        [StringLength(255)]
        public string Provider { get; set; }

        [Required]
        [Column("provider_id")]
        [StringLength(255)]
        public string ProviderId { get; set; }

        [Column("provider_account_id")]
        [StringLength(255)]
        public string? ProviderAccountId { get; set; }

        [Column("secret")]
        public string? Secret { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}