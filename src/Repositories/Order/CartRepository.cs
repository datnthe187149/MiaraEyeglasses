using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Order
{
    // Interface kho dữ liệu giỏ hàng người dùng
    public interface ICartRepository : IBaseRepository<CartItem>
    {
        Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(int userId);
        // Lấy dữ liệu: CartItem
        Task<CartItem?> GetCartItemAsync(int userId, int variantId, int lensOptionId);
        // Xóa dữ liệu: ClearCart
        Task ClearCartAsync(int userId);
    }

    // Lớp thực thi truy xuất và lưu trữ giỏ hàng trong cơ sở dữ liệu
    public class CartRepository : BaseRepository<CartItem>, ICartRepository
    {
        // Khởi tạo đối tượng CartRepository kèm nạp các phụ thuộc cần thiết
        public CartRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(int userId)
        {
            return await _dbSet.Include(c => c.Variant)
                                   .ThenInclude(v => v.Product)
                                       .ThenInclude(p => p.Images)
                               .Include(c => c.LensOption)
                               .Where(c => c.UserId == userId)
                               .OrderByDescending(c => c.CreatedAt)
                               .ToListAsync();
        }

        // Lấy dữ liệu: CartItem
        public async Task<CartItem?> GetCartItemAsync(int userId, int variantId, int lensOptionId)
        {
            return await _dbSet.FirstOrDefaultAsync(c =>
                c.UserId == userId && c.VariantId == variantId && c.LensOptionId == lensOptionId);
        }

        // Xóa dữ liệu: ClearCart
        public async Task ClearCartAsync(int userId)
        {
            var items = await _dbSet.Where(c => c.UserId == userId).ToListAsync();
            _dbSet.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}

