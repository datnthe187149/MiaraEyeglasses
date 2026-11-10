using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng Banners: Banner hình ảnh trình chiếu trang chủ
    /// </summary>
    [Table("Banners")]
    public class Banner
    {
        [Key]
        public int BannerId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string LinkUrl { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string ImageUrl { get; set; } = string.Empty;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

