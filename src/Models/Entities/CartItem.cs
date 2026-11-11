using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng CartItems: Mục trong giỏ hàng kèm thông số đơn kính (OD/OS: Sph, Cyl, Axis, PD, ảnh đơn)
    [Table("CartItems")]
    // Class định nghĩa: CartItem
    public class CartItem
    {
        [Key]
        // Thuộc tính: CartItemId
        public int CartItemId { get; set; }

        // Thuộc tính: UserId
        public int UserId { get; set; }

        // Thuộc tính: VariantId
        public int VariantId { get; set; }

        // Thuộc tính: LensOptionId
        public int LensOptionId { get; set; } = 1; // 1: Mặc định "Chỉ mua gọng"

        [Range(1, 20)]
        // Thuộc tính: Quantity
        public int Quantity { get; set; } = 1;

        // Thuộc tính: IsSelected
        public bool IsSelected { get; set; } = true;

        // Thông số đơn kính (Mắt phải - OD)
        [Column(TypeName = "decimal(4,2)")]
        // Thuộc tính: RightSph
        public decimal? RightSph { get; set; } // Độ cầu (-15 đến +10)

        [Column(TypeName = "decimal(4,2)")]
        // Thuộc tính: RightCyl
        public decimal? RightCyl { get; set; } // Độ loạn (-6 đến +6)

        // Thuộc tính: RightAxis
        public short? RightAxis { get; set; } // Trục (0 đến 180)

        // Thông số đơn kính (Mắt trái - OS)
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
        public decimal? PD { get; set; } // Khoảng cách đồng tử (50 đến 78 mm)

        [StringLength(300)]
        // Thuộc tính: PrescriptionImg
        public string? PrescriptionImg { get; set; } // Đường dẫn ảnh đơn kính

        [StringLength(300)]
        // Thuộc tính: Note
        public string? Note { get; set; }

        // Thuộc tính: CreatedAt
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

