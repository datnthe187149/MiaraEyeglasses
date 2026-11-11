using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng Payments: Nhật ký giao dịch cổng thanh toán VNPay (hỗ trợ thanh toán lại và đối soát IPN)
    [Table("Payments")]
    // Class định nghĩa: Payment
    public class Payment
    {
        [Key]
        // Thuộc tính: PaymentId
        public int PaymentId { get; set; }

        // Thuộc tính: OrderId
        public int OrderId { get; set; }

        [Required]
        [StringLength(20)]
        // Thuộc tính: Method
        public string Method { get; set; } = "VNPAY";

        [Column(TypeName = "decimal(18,0)")]
        // Thuộc tính: Amount
        public decimal Amount { get; set; }

        // 0: Đang chờ | 1: Thành công | 2: Thất bại | 3: Đã hoàn tiền
        public byte Status { get; set; } = 0;

        [Required]
        [StringLength(100)]
        // Thuộc tính: TransactionRef
        public string TransactionRef { get; set; } = string.Empty; // vnp_TxnRef

        [StringLength(100)]
        // Thuộc tính: TransactionNo
        public string? TransactionNo { get; set; } // vnp_TransactionNo

        [StringLength(50)]
        // Thuộc tính: BankCode
        public string? BankCode { get; set; }

        [StringLength(10)]
        // Thuộc tính: ResponseCode
        public string? ResponseCode { get; set; } // '00' = thành công

        // Thuộc tính: PayDate
        public DateTime? PayDate { get; set; }

        // Thuộc tính: RawResponse
        public string? RawResponse { get; set; } // Query IPN/return thô

        // Thuộc tính: CreatedAt
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
    }
}

