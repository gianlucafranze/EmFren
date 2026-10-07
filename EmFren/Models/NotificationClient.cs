using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmFren.Models
{
    public class NotificationClient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("order_id")]
        public int? OrderId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relationships

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        public ICollection<NotificationUserClient> NotificationUsersClient { get; set; }
            = new List<NotificationUserClient>();
    }
}