using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng Products: Mẫu gọng kính (thông tin chung, giá niêm yết, dáng kính, chất liệu)
    [Table("Products")]
    // Class định nghĩa: Product
    public class Product
    {
        [Key]
        // Thuộc tính: ProductId
        public int ProductId { get; set; }

        [Required]
        [StringLength(50)]
        // Thuộc tính: ProductCode
        public string ProductCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        // Thuộc tính: ProductName
        public string ProductName { get; set; } = string.Empty;

        // Thuộc tính: CategoryId
        public int CategoryId { get; set; }

        // Thuộc tính: BrandId
        public int BrandId { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        // Thuộc tính: Price
        public decimal Price { get; set; }

        [Required]
        [StringLength(10)]
        // Thuộc tính: Gender
        public string Gender { get; set; } = "Unisex"; // 'Unisex', 'Nữ', 'Nam'

        [StringLength(20)]
        // Thuộc tính: FrameShape
        public string? FrameShape { get; set; } // 'Chữ nhật', 'Vuông', 'Tròn', 'Oval', 'Mắt mèo', 'Đa giác'

        [StringLength(20)]
        // Thuộc tính: Material
        public string? Material { get; set; } // 'Nhựa', 'Kim loại', 'Titan'

        // Thuộc tính: Description
        public string? Description { get; set; }

        // Thuộc tính: IsActive
        public bool IsActive { get; set; } = true;

        // Thuộc tính: CreatedAt
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigations
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;

        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; } = null!;

        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    }
}

