using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng ProductImages: Hình ảnh sản phẩm (ảnh chung của mẫu hoặc ảnh riêng theo biến thể màu)
    [Table("ProductImages")]
    // Class định nghĩa: ProductImage
    public class ProductImage
    {
        [Key]
        // Thuộc tính: ImageId
        public int ImageId { get; set; }

        // Thuộc tính: ProductId
        public int ProductId { get; set; }

        // Thuộc tính: VariantId
        public int? VariantId { get; set; }

        [Required]
        [StringLength(300)]
        // Thuộc tính: ImageUrl
        public string ImageUrl { get; set; } = string.Empty;

        // Thuộc tính: SortOrder
        public int SortOrder { get; set; } = 0;

        // Thuộc tính: IsPrimary
        public bool IsPrimary { get; set; } = false;

        // Navigations
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("VariantId")]
        public virtual ProductVariant? Variant { get; set; }
    }
}

