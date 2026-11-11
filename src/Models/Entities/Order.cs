using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng Orders: Quản lý thông tin đơn hàng, snapshot địa chỉ, mã giảm giá và thanh toán
    [Table("Orders")]
    // Class định nghĩa: Order
    public class Order
    {
        [Key]
        // Thuộc tính: OrderId
        public int OrderId { get; set; }

        // Mã đơn hàng tự sinh (VD: MR-000123)
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        // Thuộc tính: OrderCode
        public string OrderCode { get; set; } = string.Empty;

        // Thuộc tính: UserId
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        // Thuộc tính: RecipientName
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        // Thuộc tính: Phone
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        // Thuộc tính: ShippingAddress
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(300)]
        // Thuộc tính: OrderNote
        public string? OrderNote { get; set; }

        // Thuộc tính: CouponId
        public int? CouponId { get; set; }

        [StringLength(50)]
        // Thuộc tính: CouponCode
        public string? CouponCode { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        // Thuộc tính: Subtotal
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        // Thuộc tính: DiscountAmount
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,0)")]
        // Thuộc tính: ShippingFee
        public decimal ShippingFee { get; set; } = 0;

        [Column(TypeName = "decimal(18,0)")]
        // Thuộc tính: TotalAmount
        public decimal TotalAmount { get; set; }

        // 0: Chờ thanh toán | 1: Đã thanh toán | 2: Đang xử lý (cắt tròng) | 3: Đang giao | 4: Hoàn thành | 5: Đã hủy | 6: Trả hàng
        public byte Status { get; set; } = 0;

        // Thuộc tính: PaymentExpireAt
        public DateTime? PaymentExpireAt { get; set; }

        // Thuộc tính: HandledBy
        public int? HandledBy { get; set; } // Nhân viên xử lý

        // Thuộc tính: CreatedAt
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Thuộc tính: PaidAt
        public DateTime? PaidAt { get; set; }
        // Thuộc tính: CompletedAt
        public DateTime? CompletedAt { get; set; }
        // Thuộc tính: CancelledAt
        public DateTime? CancelledAt { get; set; }

        [StringLength(300)]
        // Thuộc tính: CancelReason
        public string? CancelReason { get; set; }

        // Hoàn tiền VNPay
        public DateTime? RefundedAt { get; set; }

        [StringLength(100)]
        // Thuộc tính: RefundTransactionNo
        public string? RefundTransactionNo { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        // Thuộc tính: RefundAmount
        public decimal? RefundAmount { get; set; }

        [StringLength(300)]
        // Thuộc tính: RefundReason
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

