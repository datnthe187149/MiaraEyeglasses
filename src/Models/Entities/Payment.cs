using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng Payments: Nhật ký giao dịch cổng thanh toán VNPay (hỗ trợ thanh toán lại và đối soát IPN)
    /// </summary>
    [Table("Payments")]
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int OrderId { get; set; }

        [Required]
        [StringLength(20)]
        public string Method { get; set; } = "VNPAY";

        [Column(TypeName = "decimal(18,0)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 0: Đang chờ | 1: Thành công | 2: Thất bại | 3: Đã hoàn tiền
        /// </summary>
        public byte Status { get; set; } = 0;

        [Required]
        [StringLength(100)]
        public string TransactionRef { get; set; } = string.Empty; // vnp_TxnRef

        [StringLength(100)]
        public string? TransactionNo { get; set; } // vnp_TransactionNo

        [StringLength(50)]
        public string? BankCode { get; set; }

        [StringLength(10)]
        public string? ResponseCode { get; set; } // '00' = thành công

        public DateTime? PayDate { get; set; }

        public string? RawResponse { get; set; } // Query IPN/return thô

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
    }
}

