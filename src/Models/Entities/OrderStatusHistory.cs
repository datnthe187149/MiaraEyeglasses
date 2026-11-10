using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng OrderStatusHistory: Lịch sử thay đổi trạng thái đơn (được ghi tự động qua Trigger trg_Orders_StatusHistory)
    /// </summary>
    [Table("OrderStatusHistory")]
    public class OrderStatusHistory
    {
        [Key]
        public int HistoryId { get; set; }

        public int OrderId { get; set; }

        public byte? OldStatus { get; set; }

        public byte NewStatus { get; set; }

        public int? ChangedBy { get; set; } // NULL = hệ thống/VNPay tự động

        [StringLength(300)]
        public string? Note { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        // Navigations
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("OldStatus")]
        public virtual OrderStatus? OldOrderStatus { get; set; }

        [ForeignKey("NewStatus")]
        public virtual OrderStatus NewOrderStatus { get; set; } = null!;

        [ForeignKey("ChangedBy")]
        public virtual User? ChangedByUser { get; set; }
    }
}

