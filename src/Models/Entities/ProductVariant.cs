using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng ProductVariants: Biến thể theo màu sắc, kích cỡ, SKU và tồn kho thực tế
    [Table("ProductVariants")]
    // Class định nghĩa: ProductVariant
    public class ProductVariant
    {
        [Key]
        // Thuộc tính: VariantId
        public int VariantId { get; set; }

        // Thuộc tính: ProductId
        public int ProductId { get; set; }

        [Required]
        [StringLength(60)]
        // Thuộc tính: Sku
        public string Sku { get; set; } = string.Empty;

        [Required]
        [StringLength(60)]
        // Thuộc tính: ColorName
        public string ColorName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        // Thuộc tính: ColorGroup
        public string ColorGroup { get; set; } = string.Empty; // 'Đen', 'Nâu', 'Đồi mồi', 'Trong suốt', 'Hồng / Đỏ', 'Xám / Bạc', 'Vàng'

        [Required]
        [StringLength(10)]
        // Thuộc tính: SizeLabel
        public string SizeLabel { get; set; } = "M"; // S / M / L

        // Thuộc tính: LensWidth
        public short? LensWidth { get; set; } // Chiều rộng tròng (mm)
        // Thuộc tính: BridgeWidth
        public short? BridgeWidth { get; set; } // Cầu kính (mm)
        // Thuộc tính: TempleLength
        public short? TempleLength { get; set; } // Càng kính (mm)

        // Thuộc tính: StockQty
        public int StockQty { get; set; } = 0; // Tồn kho

        // Thuộc tính: IsActive
        public bool IsActive { get; set; } = true;

        // Navigations
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}

