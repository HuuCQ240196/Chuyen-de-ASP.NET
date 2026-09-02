using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProLap.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [StringLength(100)]
        public string CPU { get; set; } = string.Empty;

        [StringLength(100)]
        public string RAM { get; set; } = string.Empty;

        [StringLength(100)]
        public string Storage { get; set; } = string.Empty;

        [StringLength(100)]
        public string GPU { get; set; } = string.Empty;

        [StringLength(100)]
        public string Screen { get; set; } = string.Empty;

        public int Stock { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // Khóa ngoại đến Brand
        public int BrandId { get; set; }
        public Brand? Brand { get; set; }

        // Khóa ngoại đến Category
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Danh sách hình ảnh của sản phẩm
        public ICollection<ProductImage> ProductImages { get; set; }
            = new List<ProductImage>();
    }
}