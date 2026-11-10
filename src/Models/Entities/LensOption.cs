using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng LensOptions: Các gói tròng kính (Chỉ mua gọng, BlueCut, Đổi màu, Siêu mỏng...)
    /// </summary>
    [Table("LensOptions")]
    public class LensOption
    {
        [Key]
        public int LensOptionId { get; set; }

        [Required]
        [StringLength(150)]
        public string LensName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(3,2)")]
        public decimal RefractiveIndex { get; set; } // Chiết suất tròng: 1.50, 1.56, 1.60, 1.67

        [Column(TypeName = "decimal(18,0)")]
        public decimal ExtraPrice { get; set; } = 0; // Giá phụ thu

        [StringLength(100)]
        public string? RxLimit { get; set; } // Độ cận khuyến nghị

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

