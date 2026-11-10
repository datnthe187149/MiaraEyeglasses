using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng ProductVariants: Biến thể theo màu sắc, kích cỡ, SKU và tồn kho thực tế
    /// </summary>
    [Table("ProductVariants")]
    public class ProductVariant
    {
        [Key]
        public int VariantId { get; set; }

        public int ProductId { get; set; }

        [Required]
        [StringLength(60)]
        public string Sku { get; set; } = string.Empty;

        [Required]
        [StringLength(60)]
        public string ColorName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ColorGroup { get; set; } = string.Empty; // 'Đen', 'Nâu', 'Đồi mồi', 'Trong suốt', 'Hồng / Đỏ', 'Xám / Bạc', 'Vàng'

        [Required]
        [StringLength(10)]
        public string SizeLabel { get; set; } = "M"; // S / M / L

        public short? LensWidth { get; set; } // Chiều rộng tròng (mm)
        public short? BridgeWidth { get; set; } // Cầu kính (mm)
        public short? TempleLength { get; set; } // Càng kính (mm)

        public int StockQty { get; set; } = 0; // Tồn kho

        public bool IsActive { get; set; } = true;

        // Navigations
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}

