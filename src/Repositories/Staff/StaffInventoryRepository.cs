using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Staff
{
    public interface IStaffInventoryRepository : IBaseRepository<ProductVariant>
    {
        Task<IEnumerable<ProductVariant>> GetAllVariantsWithProductsAsync();
        Task<(IEnumerable<ProductVariant> Variants, int TotalCount, int LowStockCount, int OutOfStockCount, int AllCount)> GetInventoryPagedAsync(
            int page, int pageSize, string? search = null, string? filter = null);
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

        public async Task<(IEnumerable<ProductVariant> Variants, int TotalCount, int LowStockCount, int OutOfStockCount, int AllCount)> GetInventoryPagedAsync(
            int page, int pageSize, string? search = null, string? filter = null)
        {
            var baseQuery = _dbSet
                .Include(v => v.Product)
                    .ThenInclude(p => p.Images)
                .Include(v => v.Product)
                    .ThenInclude(p => p.Brand)
                .AsQueryable();

            // Đếm thống kê tổng
            int allCount = await baseQuery.CountAsync();
            int lowStockCount = await baseQuery.CountAsync(v => v.StockQty > 0 && v.StockQty <= 3);
            int outOfStockCount = await baseQuery.CountAsync(v => v.StockQty == 0);

            var query = baseQuery;

            // 1. Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(v => v.Sku.ToLower().Contains(term)
                                      || v.ColorName.ToLower().Contains(term)
                                      || v.Product.ProductName.ToLower().Contains(term)
                                      || v.Product.ProductCode.ToLower().Contains(term));
            }

            // 2. Lọc theo tình trạng tồn kho
            if (filter == "low")
            {
                query = query.Where(v => v.StockQty > 0 && v.StockQty <= 3);
            }
            else if (filter == "out")
            {
                query = query.Where(v => v.StockQty == 0);
            }
            else if (filter == "in")
            {
                query = query.Where(v => v.StockQty > 3);
            }

            int totalCount = await query.CountAsync();
            var variants = await query.OrderBy(v => v.StockQty)
                                      .ThenBy(v => v.Product.ProductName)
                                      .Skip((page - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToListAsync();

            return (variants, totalCount, lowStockCount, outOfStockCount, allCount);
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

