using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng ProductImages: Hình ảnh sản phẩm (ảnh chung của mẫu hoặc ảnh riêng theo biến thể màu)
    /// </summary>
    [Table("ProductImages")]
    public class ProductImage
    {
        [Key]
        public int ImageId { get; set; }

        public int ProductId { get; set; }

        public int? VariantId { get; set; }

        [Required]
        [StringLength(300)]
        public string ImageUrl { get; set; } = string.Empty;

        public int SortOrder { get; set; } = 0;

        public bool IsPrimary { get; set; } = false;

        // Navigations
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("VariantId")]
        public virtual ProductVariant? Variant { get; set; }
    }
}

