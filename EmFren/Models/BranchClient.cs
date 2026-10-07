using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmFren.Models
{
    [Table("branch_client")]
    public class BranchClient
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("country_name")]
        public string CountryName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("city")]
        public string City { get; set; } = string.Empty;
    }
}