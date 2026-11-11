using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng Banners: Lưu trữ các hình ảnh trình chiếu quảng cáo và sự kiện
    [Table("Banners")]
    public class Banner
    {
        // Khóa chính mã định danh banner
        [Key]
        public int BannerId { get; set; }

        // Tiêu đề của banner quảng cáo
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        // Đường dẫn liên kết khi người dùng bấm vào banner
        [Required]
        [StringLength(300)]
        public string LinkUrl { get; set; } = string.Empty;

        // Đường dẫn hình ảnh banner
        [Required]
        [StringLength(300)]
        public string ImageUrl { get; set; } = string.Empty;

        // Thứ tự ưu tiên sắp xếp hiển thị
        public int SortOrder { get; set; } = 0;

        // Trạng thái kích hoạt hiển thị
        public bool IsActive { get; set; } = true;

        // Thời điểm tạo bản ghi banner
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
