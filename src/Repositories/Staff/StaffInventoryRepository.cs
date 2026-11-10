using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Staff
{
    public interface IStaffInventoryRepository : IBaseRepository<ProductVariant>
    {
        Task<IEnumerable<ProductVariant>> GetAllVariantsWithProductsAsync();
        Task<bool> UpdateStockAsync(int variantId, int newStock);
    }

    public class StaffInventoryRepository : BaseRepository<ProductVariant>, IStaffInventoryRepository
    {
        public StaffInventoryRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<ProductVariant>> GetAllVariantsWithProductsAsync()
        {
            return await _dbSet
                .Include(v => v.Product)
                    .ThenInclude(p => p.Images)
                .OrderBy(v => v.Product.ProductName)
                .ThenBy(v => v.ColorName)
                .ToListAsync();
        }

        public async Task<bool> UpdateStockAsync(int variantId, int newStock)
        {
            var variant = await _dbSet.FindAsync(variantId);
            if (variant == null) return false;

            variant.StockQty = newStock;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

