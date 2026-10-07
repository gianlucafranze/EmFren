using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmFren.Models
{
    public class Staff
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? DocumentNumber { get; set; }

        [MaxLength(50)]
        public string? Email { get; set; }

        [MaxLength(150)]
        public string? Ubicacion { get; set; }

        public int? DepartmentId { get; set; }

        [MaxLength(30)]
        public string? CellphoneNumber { get; set; }

        public bool? StaffActive { get; set; }

        // Relationship with Department
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }
    }
}