using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmFren.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }


        // =====================================================
        // USER
        // =====================================================

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }


        // =====================================================
        // ORDER DATE
        // =====================================================

        public DateTime? DateOrder { get; set; }


        // =====================================================
        // PAYMENT METHOD
        // =====================================================

        [Required]
        public int CoinPaymentId { get; set; }

        [ForeignKey("CoinPaymentId")]
        public CoinPayment? CoinPayment { get; set; }


        // =====================================================
        // TOTAL PURCHASE
        // =====================================================

        [Column(TypeName = "decimal(12,2)")]
        public decimal? TotalPurchase { get; set; }

        // =====================================================
        // DEBTOR CLIENT
        // =====================================================

        [Column("is_debtor_client")]
        public bool? IsDebtorClient { get; set; }

        // =====================================================
        // ORDER ARTICLES
        // =====================================================

        public ICollection<OrderArticle> OrderArticles { get; set; }
            = new List<OrderArticle>();
    }
}