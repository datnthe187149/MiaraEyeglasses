using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng LensOptions: Danh mục các gói tròng kính (Chỉ gọng, Chống ánh sáng xanh, Đổi màu, Siêu mỏng)
    [Table("LensOptions")]
    public class LensOption
    {
        // Khóa chính mã định danh loại tròng kính
        [Key]
        public int LensOptionId { get; set; }

        // Tên gói tròng kính hiển thị cho khách chọn
        [Required]
        [StringLength(150)]
        public string LensName { get; set; } = string.Empty;

        // Chiết suất tròng kính (VD: 1.56, 1.60, 1.67)
        [Column(TypeName = "decimal(3,2)")]
        public decimal RefractiveIndex { get; set; }

        // Mức giá phụ thu cộng thêm vào giá gọng
        [Column(TypeName = "decimal(18,0)")]
        public decimal ExtraPrice { get; set; } = 0;

        // Giới hạn độ cận thị khuyến nghị cho tròng kính
        [StringLength(100)]
        public string? RxLimit { get; set; }

        // Mô tả chi tiết tính năng của tròng kính
        [StringLength(500)]
        public string? Description { get; set; }

        // Trạng thái mở bán gói tròng kính
        public bool IsActive { get; set; } = true;
    }
}
