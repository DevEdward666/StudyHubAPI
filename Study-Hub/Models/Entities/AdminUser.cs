using Study_Hub.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Study_Hub.Models.Entities
{
    public enum UserRole
    {
        Admin,
        SuperAdmin
    }

    [Table("admin_users")]
    public class AdminUser
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("role")]
        public UserRole Role { get; set; } = UserRole.Admin;

        [Column("permissions")]
        public string[] Permissions { get; set; } = { "approve_transactions", "manage_tables", "manage_users" };

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}