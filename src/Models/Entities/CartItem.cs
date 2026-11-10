using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng CartItems: Mục trong giỏ hàng kèm thông số đơn kính (OD/OS: Sph, Cyl, Axis, PD, ảnh đơn)
    /// </summary>
    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        public int UserId { get; set; }

        public int VariantId { get; set; }

        public int LensOptionId { get; set; } = 1; // 1: Mặc định "Chỉ mua gọng"

        [Range(1, 20)]
        public int Quantity { get; set; } = 1;

        public bool IsSelected { get; set; } = true;

        // Thông số đơn kính (Mắt phải - OD)
        [Column(TypeName = "decimal(4,2)")]
        public decimal? RightSph { get; set; } // Độ cầu (-15 đến +10)

        [Column(TypeName = "decimal(4,2)")]
        public decimal? RightCyl { get; set; } // Độ loạn (-6 đến +6)

        public short? RightAxis { get; set; } // Trục (0 đến 180)

        // Thông số đơn kính (Mắt trái - OS)
        [Column(TypeName = "decimal(4,2)")]
        public decimal? LeftSph { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? LeftCyl { get; set; }

        public short? LeftAxis { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        public decimal? PD { get; set; } // Khoảng cách đồng tử (50 đến 78 mm)

        [StringLength(300)]
        public string? PrescriptionImg { get; set; } // Đường dẫn ảnh đơn kính

        [StringLength(300)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigations
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("VariantId")]
        public virtual ProductVariant Variant { get; set; } = null!;

        [ForeignKey("LensOptionId")]
        public virtual LensOption LensOption { get; set; } = null!;
    }
}

