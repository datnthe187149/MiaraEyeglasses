using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng OrderStatuses: Danh mục trạng thái đơn hàng (0: Chờ thanh toán ... 6: Trả hàng)
    /// </summary>
    [Table("OrderStatuses")]
    public class OrderStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public byte StatusId { get; set; }

        [Required]
        [StringLength(50)]
        public string StatusName { get; set; } = string.Empty;
    }
}

