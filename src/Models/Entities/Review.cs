using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng Reviews: Đánh giá và bình luận sản phẩm của khách hàng
    [Table("Reviews")]
    // Class định nghĩa: Review
    public class Review
    {
        [Key]
        // Thuộc tính: ReviewId
        public int ReviewId { get; set; }

        // Thuộc tính: ProductId
        public int ProductId { get; set; }

        // Thuộc tính: UserId
        public int UserId { get; set; }

        [Range(1, 5)]
        // Thuộc tính: Rating
        public byte Rating { get; set; } // 1 đến 5 sao

        [Required]
        [StringLength(1000)]
        // Thuộc tính: Comment
        public string Comment { get; set; } = string.Empty;

        // 0: Chờ duyệt | 1: Đã duyệt hiển thị | 2: Ẩn
        public byte Status { get; set; } = 0;

        // Thuộc tính: IsRead
        public bool IsRead { get; set; } = false;

        // Thuộc tính: ModeratedBy
        public int? ModeratedBy { get; set; }

        // Thuộc tính: CreatedAt
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

