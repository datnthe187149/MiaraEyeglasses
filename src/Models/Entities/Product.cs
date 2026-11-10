using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng Products: Mẫu gọng kính (thông tin chung, giá niêm yết, dáng kính, chất liệu)
    /// </summary>
    [Table("Products")]
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public int BrandId { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = "Unisex"; // 'Unisex', 'Nữ', 'Nam'

        [StringLength(20)]
        public string? FrameShape { get; set; } // 'Chữ nhật', 'Vuông', 'Tròn', 'Oval', 'Mắt mèo', 'Đa giác'

        [StringLength(20)]
        public string? Material { get; set; } // 'Nhựa', 'Kim loại', 'Titan'

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

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

