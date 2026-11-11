using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng Coupons: Quản lý mã giảm giá khuyến mãi (chiết khấu tiền mặt hoặc phần trăm)
    [Table("Coupons")]
    public class Coupon
    {
        // Khóa chính mã định danh coupon
        [Key]
        public int CouponId { get; set; }

        // Mã code giảm giá người dùng nhập (VD: MIARA50K)
        [Required]
        [StringLength(50)]
        public string CouponCode { get; set; } = string.Empty;

        // Loại giảm giá: 'AMOUNT' (tiền mặt cố định) hoặc 'PERCENT' (theo phần trăm)
        [Required]
        [StringLength(10)]
        public string DiscountType { get; set; } = "AMOUNT";

        // Giá trị chiết khấu
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        // Mức giảm tối đa áp dụng khi chiết khấu theo phần trăm
        [Column(TypeName = "decimal(18,0)")]
        public decimal? MaxDiscount { get; set; }

        // Giá trị đơn hàng tối thiểu để được áp dụng mã
        [Column(TypeName = "decimal(18,0)")]
        public decimal MinOrderAmount { get; set; } = 0;

        // Ngày bắt đầu có hiệu lực
        public DateOnly? StartDate { get; set; }

        // Ngày hết hạn của mã giảm giá
        public DateOnly ExpireDate { get; set; }

        // Giới hạn tổng số lượt sử dụng
        public int? UsageLimit { get; set; }

        // Số lượt đã được khách hàng sử dụng thực tế
        public int UsedCount { get; set; } = 0;

        // Trạng thái kích hoạt của mã giảm giá
        public bool IsActive { get; set; } = true;
    }
}
