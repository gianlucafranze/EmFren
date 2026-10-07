using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmFren.Models
{
    public class NotificationUserClient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("notification_id")]
        public int NotificationId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        // Relationships

        [ForeignKey(nameof(NotificationId))]
        public NotificationClient? Notification { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}