using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng Reviews: Đánh giá và bình luận sản phẩm của khách hàng
    /// </summary>
    [Table("Reviews")]
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        public int ProductId { get; set; }

        public int UserId { get; set; }

        [Range(1, 5)]
        public byte Rating { get; set; } // 1 đến 5 sao

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// 0: Chờ duyệt | 1: Đã duyệt hiển thị | 2: Ẩn
        /// </summary>
        public byte Status { get; set; } = 0;

        public bool IsRead { get; set; } = false;

        public int? ModeratedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigations
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ModeratedBy")]
        public virtual User? ModeratedByUser { get; set; }
    }
}

