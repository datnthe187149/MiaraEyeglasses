using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng OrderStatuses: Danh mục các trạng thái xử lý của đơn hàng trong hệ thống
    [Table("OrderStatuses")]
    // Class định nghĩa: OrderStatus
    public class OrderStatus
    {
        // Mã định danh trạng thái đơn hàng (0: Chờ thanh toán, 1: Đã xác nhận, 5: Đã hủy)
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        // Thuộc tính: StatusId
        public byte StatusId { get; set; }

        // Tên trạng thái đơn hàng hiển thị trên giao diện
        [Required]
        [StringLength(50)]
        // Thuộc tính: StatusName
        public string StatusName { get; set; } = string.Empty;
    }
}
