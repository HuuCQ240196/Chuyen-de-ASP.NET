using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProLap.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string ImageUrl { get; set; } = string.Empty;

        // Khóa ngoại đến Product
        public int ProductId { get; set; }

        // Navigation property
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}