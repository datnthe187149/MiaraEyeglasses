using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng OrderItems: Chi tiết sản phẩm trong đơn hàng (Lưu snapshot tên, giá, đơn kính tại thời điểm đặt)
    [Table("OrderItems")]
    // Class định nghĩa: OrderItem
    public class OrderItem
    {
        [Key]
        // Thuộc tính: OrderItemId
        public int OrderItemId { get; set; }

        // Thuộc tính: OrderId
        public int OrderId { get; set; }

        // Thuộc tính: ProductId
        public int? ProductId { get; set; }
        // Thuộc tính: VariantId
        public int? VariantId { get; set; }

        [Required]
        [StringLength(200)]
        // Thuộc tính: ProductName
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        // Thuộc tính: ProductCode
        public string ProductCode { get; set; } = string.Empty;

        [Required]
        [StringLength(60)]
        // Thuộc tính: Sku
        public string Sku { get; set; } = string.Empty;

        [Required]
        [StringLength(60)]
        // Thuộc tính: ColorName
        public string ColorName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        // Thuộc tính: SizeLabel
        public string SizeLabel { get; set; } = string.Empty;

        // Thuộc tính: LensOptionId
        public int? LensOptionId { get; set; }

        [Required]
        [StringLength(150)]
        // Thuộc tính: LensName
        public string LensName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,0)")]
        // Thuộc tính: FramePrice
        public decimal FramePrice { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        // Thuộc tính: LensPrice
        public decimal LensPrice { get; set; } = 0;

        // Thuộc tính: Quantity
        public int Quantity { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(18,0)")]
        // Thuộc tính: LineTotal
        public decimal LineTotal { get; set; }

        // Đơn kính để nhân viên kỹ thuật cắt mài tròng
        [Column(TypeName = "decimal(4,2)")]
        // Thuộc tính: RightSph
        public decimal? RightSph { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        // Thuộc tính: RightCyl
        public decimal? RightCyl { get; set; }

        // Thuộc tính: RightAxis
        public short? RightAxis { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        // Thuộc tính: LeftSph
        public decimal? LeftSph { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        // Thuộc tính: LeftCyl
        public decimal? LeftCyl { get; set; }

        // Thuộc tính: LeftAxis
        public short? LeftAxis { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        // Thuộc tính: PD
        public decimal? PD { get; set; }

        [StringLength(300)]
        // Thuộc tính: PrescriptionImg
        public string? PrescriptionImg { get; set; }

        [StringLength(300)]
        // Thuộc tính: Note
        public string? Note { get; set; }

        // Navigations
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("VariantId")]
        public virtual ProductVariant? Variant { get; set; }

        [ForeignKey("LensOptionId")]
        public virtual LensOption? LensOption { get; set; }
    }
}

