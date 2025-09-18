using Study_Hub.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Study_Hub.Models.Entities
{
    [Table("study_tables")]
    public class StudyTable
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("table_number")]
        [StringLength(50)]
        public string TableNumber { get; set; }

        [Required]
        [Column("qr_code")]
        public string QrCode { get; set; }

        [Column("qr_code_image")]
        public string? QrCodeImage { get; set; }

        [Column("is_occupied")]
        public bool IsOccupied { get; set; }

        [Column("current_user_id")]
        public Guid? CurrentUserId { get; set; }

        [Required]
        [Column("hourly_rate", TypeName = "decimal(10,2)")]
        public decimal HourlyRate { get; set; }

        [Required]
        [Column("location")]
        [StringLength(255)]
        public string Location { get; set; }

        [Required]
        [Column("capacity")]
        public int Capacity { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CurrentUserId")]
        public virtual User? CurrentUser { get; set; }
        public virtual ICollection<TableSession> TableSessions { get; set; } = new List<TableSession>();
    }
}