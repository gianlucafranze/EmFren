using EmFren.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace EmFren.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }


        // =====================================================
        // USER WHO CREATED THE NOTIFICATION
        // =====================================================

        [Column("user_id")]
        public int? UserId { get; set; }


        // =====================================================
        // RELATED ORDER
        // =====================================================

        [Column("order_id")]
        public int? OrderId { get; set; }


        // =====================================================
        // NOTIFICATION TITLE
        // =====================================================

        [Required]
        [MaxLength(50)]
        public string Title { get; set; } = string.Empty;


        // =====================================================
        // NOTIFICATION MESSAGE
        // =====================================================

        [Required]
        [MaxLength(100)]
        public string Message { get; set; } = string.Empty;


        // =====================================================
        // READ STATUS
        // =====================================================

        [Column("is_read")]
        public bool IsRead { get; set; } = false;


        // =====================================================
        // CREATED DATE
        // =====================================================

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // =====================================================
        // RELATIONSHIPS
        // =====================================================

        public Order? Order { get; set; }


        public ICollection<NotificationUser> NotificationUsers { get; set; }
            = new List<NotificationUser>();
    }
}