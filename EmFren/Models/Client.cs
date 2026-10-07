using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmFren.Models
{
    [Table("client")]
    public class Client
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Column("cellphone_number")]
        [StringLength(30)]
        public string? CellphoneNumber { get; set; }

        [Column("email")]
        [StringLength(50)]
        public string? Email { get; set; }

        [Column("document_number")]
        [StringLength(30)]
        public string? DocumentNumber { get; set; }

        [Column("branch_client_id")]
        public int? BranchClientId { get; set; }

        [ForeignKey("BranchClientId")]
        public BranchClient? BranchClient { get; set; }

        [Column("client_active")]
        public bool? ClientActive { get; set; }
    }
}