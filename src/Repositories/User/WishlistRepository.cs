using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.User
{
    // Interface kho dữ liệu sản phẩm yêu thích (Wishlist)
    public interface IWishlistRepository : IBaseRepository<Wishlist>
    {
        Task<IEnumerable<Wishlist>> GetWishlistByUserIdAsync(int userId);
        // Lấy dữ liệu: WishlistItem
        Task<Wishlist?> GetWishlistItemAsync(int userId, int productId);
        // Kiểm tra điều kiện: IsWishlisted
        Task<bool> IsWishlistedAsync(int userId, int productId);
    }

    // Lớp thực thi lưu trữ và truy vấn danh sách mẫu kính yêu thích
    public class WishlistRepository : BaseRepository<Wishlist>, IWishlistRepository
    {
        // Khởi tạo đối tượng WishlistRepository kèm nạp các phụ thuộc cần thiết
        public WishlistRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<Wishlist>> GetWishlistByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(w => w.Product)
                    .ThenInclude(p => p.Images)
                .Include(w => w.Product)
                    .ThenInclude(p => p.Brand)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        // Lấy dữ liệu: WishlistItem
        public async Task<Wishlist?> GetWishlistItemAsync(int userId, int productId)
        {
            return await _dbSet.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        }

        // Kiểm tra điều kiện: IsWishlisted
        public async Task<bool> IsWishlistedAsync(int userId, int productId)
        {
            return await _dbSet.AnyAsync(w => w.UserId == userId && w.ProductId == productId);
        }
    }
}

