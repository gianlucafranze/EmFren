using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmFren.Models
{
    public class NotificationUser
    {
        [Key]
        public int Id { get; set; }

        [Column("notification_id")]
        public int NotificationId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        public Notification? Notification { get; set; }

        public User? User { get; set; }
    }
}