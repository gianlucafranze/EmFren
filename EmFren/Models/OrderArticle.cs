using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmFren.Models
{
    public class OrderArticle
    {
        [Key]
        public int Id { get; set; }


        // =====================================================
        // ORDER
        // =====================================================

        [Required]
        public int IdOrder { get; set; }

        [ForeignKey("IdOrder")]
        public Order? Order { get; set; }


        // =====================================================
        // ARTICLE
        // =====================================================

        [Required]
        public int IdArticle { get; set; }

        [ForeignKey("IdArticle")]
        public Article? Article { get; set; }


        // =====================================================
        // QUANTITY
        // =====================================================

        public int? Quantity { get; set; }
    }
}