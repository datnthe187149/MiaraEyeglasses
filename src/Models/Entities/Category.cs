using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng Categories: Danh mục sản phẩm kính (Gọng kính cận, Kính chống ánh sáng xanh...)
    [Table("Categories")]
    // Class định nghĩa: Category
    public class Category
    {
        [Key]
        // Thuộc tính: CategoryId
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        // Thuộc tính: CategoryName
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        // Thuộc tính: Slug
        public string Slug { get; set; } = string.Empty;

        [StringLength(500)]
        // Thuộc tính: Description
        public string? Description { get; set; }

        // Thuộc tính: IsActive
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

