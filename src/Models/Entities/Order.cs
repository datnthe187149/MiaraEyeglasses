using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng Orders: Quản lý thông tin đơn hàng, snapshot địa chỉ, mã giảm giá và thanh toán
    /// </summary>
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        /// <summary>
        /// Mã đơn hàng tự sinh (VD: MR-000123)
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string OrderCode { get; set; } = string.Empty;

        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(300)]
        public string? OrderNote { get; set; }

        public int? CouponId { get; set; }

        [StringLength(50)]
        public string? CouponCode { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,0)")]
        public decimal ShippingFee { get; set; } = 0;

        [Column(TypeName = "decimal(18,0)")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 0: Chờ thanh toán | 1: Đã thanh toán | 2: Đang xử lý (cắt tròng) | 3: Đang giao | 4: Hoàn thành | 5: Đã hủy | 6: Trả hàng
        /// </summary>
        public byte Status { get; set; } = 0;

        public DateTime? PaymentExpireAt { get; set; }

        public int? HandledBy { get; set; } // Nhân viên xử lý

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        [StringLength(300)]
        public string? CancelReason { get; set; }

        // Hoàn tiền VNPay
        public DateTime? RefundedAt { get; set; }

        [StringLength(100)]
        public string? RefundTransactionNo { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal? RefundAmount { get; set; }

        [StringLength(300)]
        public string? RefundReason { get; set; }

        // Navigations
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("HandledBy")]
        public virtual User? HandledByUser { get; set; }

        [ForeignKey("CouponId")]
        public virtual Coupon? Coupon { get; set; }

        [ForeignKey("Status")]
        public virtual OrderStatus OrderStatus { get; set; } = null!;

        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();
    }
}

