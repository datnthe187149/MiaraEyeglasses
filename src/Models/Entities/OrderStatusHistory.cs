using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng OrderStatusHistory: Lịch sử thay đổi trạng thái đơn (được ghi tự động qua Trigger trg_Orders_StatusHistory)
    [Table("OrderStatusHistory")]
    // Class định nghĩa: OrderStatusHistory
    public class OrderStatusHistory
    {
        [Key]
        // Thuộc tính: HistoryId
        public int HistoryId { get; set; }

        // Thuộc tính: OrderId
        public int OrderId { get; set; }

        // Thuộc tính: OldStatus
        public byte? OldStatus { get; set; }

        // Thuộc tính: NewStatus
        public byte NewStatus { get; set; }

        // Thuộc tính: ChangedBy
        public int? ChangedBy { get; set; } // NULL = hệ thống/VNPay tự động

        [StringLength(300)]
        // Thuộc tính: Note
        public string? Note { get; set; }

        // Thuộc tính: ChangedAt
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

