using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Order
{
    public interface ICartRepository : IBaseRepository<CartItem>
    {
        Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(int userId);
        Task<CartItem?> GetCartItemAsync(int userId, int variantId, int lensOptionId);
        Task ClearCartAsync(int userId);
    }

    public class CartRepository : BaseRepository<CartItem>, ICartRepository
    {
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

        public async Task<CartItem?> GetCartItemAsync(int userId, int variantId, int lensOptionId)
        {
            return await _dbSet.FirstOrDefaultAsync(c =>
                c.UserId == userId && c.VariantId == variantId && c.LensOptionId == lensOptionId);
        }

        public async Task ClearCartAsync(int userId)
        {
            var items = await _dbSet.Where(c => c.UserId == userId).ToListAsync();
            _dbSet.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}

