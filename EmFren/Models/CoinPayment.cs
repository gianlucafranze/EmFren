using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmFren.Models
{
    [Table("coin_payment")]
    public class CoinPayment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column("coin_name")]
        public string CoinName { get; set; } = string.Empty;
    }
}