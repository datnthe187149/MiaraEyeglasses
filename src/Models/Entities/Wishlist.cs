using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng Wishlists: Danh sách sản phẩm kính yêu thích của khách hàng
    [Table("Wishlists")]
    // Class định nghĩa: Wishlist
    public class Wishlist
    {
        // Thuộc tính: UserId
        public int UserId { get; set; }
        // Thuộc tính: ProductId
        public int ProductId { get; set; }

        // Thuộc tính: CreatedAt
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigations
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}

