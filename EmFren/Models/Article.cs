using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmFren.Models
{
    [Table("articles")]
    public class Article
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("article_id")]
        [StringLength(50)]
        public string? ArticleId { get; set; }

        [Column("vehicle_id")]
        public int? VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

        [Column("measure")]
        [StringLength(50)]
        public string? Measure { get; set; }

        [Column("type")]
        [StringLength(100)]
        public string? Type { get; set; }

        [Column("friction")]
        [StringLength(100)]
        public string? Friction { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }

        [Column("stock")]
        public bool? Stock { get; set; }

        [Column("discount")]
        public decimal? Discount { get; set; }
    }
}