using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng OrderItems: Chi tiết sản phẩm trong đơn hàng (Lưu snapshot tên, giá, đơn kính tại thời điểm đặt)
    /// </summary>
    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }

        public int? ProductId { get; set; }
        public int? VariantId { get; set; }

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ProductCode { get; set; } = string.Empty;

        [Required]
        [StringLength(60)]
        public string Sku { get; set; } = string.Empty;

        [Required]
        [StringLength(60)]
        public string ColorName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string SizeLabel { get; set; } = string.Empty;

        public int? LensOptionId { get; set; }

        [Required]
        [StringLength(150)]
        public string LensName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,0)")]
        public decimal FramePrice { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal LensPrice { get; set; } = 0;

        public int Quantity { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(18,0)")]
        public decimal LineTotal { get; set; }

        // Đơn kính để nhân viên kỹ thuật cắt mài tròng
        [Column(TypeName = "decimal(4,2)")]
        public decimal? RightSph { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? RightCyl { get; set; }

        public short? RightAxis { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? LeftSph { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? LeftCyl { get; set; }

        public short? LeftAxis { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        public decimal? PD { get; set; }

        [StringLength(300)]
        public string? PrescriptionImg { get; set; }

        [StringLength(300)]
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

