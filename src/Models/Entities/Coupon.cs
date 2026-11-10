using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng Coupons: Mã giảm giá khuyến mãi (AMOUNT theo tiền mặt hoặc PERCENT theo phần trăm)
    /// </summary>
    [Table("Coupons")]
    public class Coupon
    {
        [Key]
        public int CouponId { get; set; }

        [Required]
        [StringLength(50)]
        public string CouponCode { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string DiscountType { get; set; } = "AMOUNT"; // 'AMOUNT' hoặc 'PERCENT'

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal? MaxDiscount { get; set; } // Trần giảm giá khi DiscountType = PERCENT

        [Column(TypeName = "decimal(18,0)")]
        public decimal MinOrderAmount { get; set; } = 0; // Giá trị đơn hàng tối thiểu

        public DateOnly? StartDate { get; set; }

        public DateOnly ExpireDate { get; set; }

        public int? UsageLimit { get; set; } // Giới hạn lượt dùng (NULL = không giới hạn)

        public int UsedCount { get; set; } = 0; // Số lượt đã sử dụng

        public bool IsActive { get; set; } = true;
    }
}

