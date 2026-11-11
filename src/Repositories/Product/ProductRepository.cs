using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Product
{
    // Interface kho dữ liệu sản phẩm kính mắt
    public interface IProductRepository : IBaseRepository<Models.Entities.Product>
    {
        Task<IEnumerable<Models.Entities.Product>> GetFeaturedProductsAsync(int count = 8);
        // Lấy dữ liệu: ProductWithVariantsAndImages
        Task<Models.Entities.Product?> GetProductWithVariantsAndImagesAsync(int productId);
        Task<(IEnumerable<Models.Entities.Product> Products, int TotalCount)> SearchProductsAsync(
            string? searchTerm, int? categoryId, int? brandId, string? gender, string? shape, string? material,
            decimal? minPrice, decimal? maxPrice, string? sort = null, int page = 1, int pageSize = 12);
        Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(int productId);
        Task<IEnumerable<Models.Entities.Product>> GetSimilarProductsAsync(int currentProductId, int? categoryId, string? shape, int count = 4);
        Task<IEnumerable<Models.Entities.Product>> GetBestsellerProductsAsync(int count = 4);
    }

    // Lớp thực thi tìm kiếm, lọc động và phân trang sản phẩm
    public class ProductRepository : BaseRepository<Models.Entities.Product>, IProductRepository
    {
        // Khởi tạo đối tượng ProductRepository kèm nạp các phụ thuộc cần thiết
        public ProductRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<Models.Entities.Product>> GetFeaturedProductsAsync(int count = 8)
        {
            return await _dbSet.Include(p => p.Brand)
                               .Include(p => p.Category)
                               .Include(p => p.Images)
                               .Include(p => p.Variants)
                               .Where(p => p.IsActive)
                               .OrderByDescending(p => p.CreatedAt)
                               .Take(count)
                               .AsNoTracking()
                               .ToListAsync();
        }

        // Lấy dữ liệu: ProductWithVariantsAndImages
        public async Task<Models.Entities.Product?> GetProductWithVariantsAndImagesAsync(int productId)
        {
            return await _dbSet.Include(p => p.Brand)
                               .Include(p => p.Category)
                               .Include(p => p.Variants.Where(v => v.IsActive))
                               .Include(p => p.Images)
                               .Include(p => p.Reviews.Where(r => r.Status == 1))
                                   .ThenInclude(r => r.User)
                               .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsActive);
        }

        public async Task<(IEnumerable<Models.Entities.Product> Products, int TotalCount)> SearchProductsAsync(
            string? searchTerm, int? categoryId, int? brandId, string? gender, string? shape, string? material,
            decimal? minPrice, decimal? maxPrice, string? sort = null, int page = 1, int pageSize = 12)
        {
            var query = _dbSet.Include(p => p.Brand)
                              .Include(p => p.Category)
                              .Include(p => p.Images)
                              .Include(p => p.Variants)
                              .Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(term) || p.ProductCode.ToLower().Contains(term));
            }

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            if (!string.IsNullOrWhiteSpace(gender))
                query = query.Where(p => p.Gender == gender);

            if (!string.IsNullOrWhiteSpace(shape))
                query = query.Where(p => p.FrameShape == shape);

            if (!string.IsNullOrWhiteSpace(material))
                query = query.Where(p => p.Material == material);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            var totalCount = await query.CountAsync();

            // Áp dụng quy tắc sắp xếp
            IQueryable<Models.Entities.Product> orderedQuery = sort switch
            {
                "price-asc" => query.OrderBy(p => p.Price),
                "price-desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.ProductName),
                "bestseller" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var products = await orderedQuery.Skip((page - 1) * pageSize)
                                             .Take(pageSize)
                                             .AsNoTracking()
                                             .ToListAsync();

            return (products, totalCount);
        }

        public async Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(int productId)
        {
            return await _context.ProductVariants
                                 .Where(v => v.ProductId == productId && v.IsActive)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Models.Entities.Product>> GetSimilarProductsAsync(int currentProductId, int? categoryId, string? shape, int count = 4)
        {
            var query = _dbSet.Include(p => p.Brand)
                              .Include(p => p.Category)
                              .Include(p => p.Images)
                              .Include(p => p.Variants)
                              .Where(p => p.IsActive && p.ProductId != currentProductId);

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(shape))
            {
                query = query.Where(p => p.FrameShape == shape);
            }

            var results = await query.OrderByDescending(p => p.CreatedAt).Take(count).AsNoTracking().ToListAsync();
            // Nếu danh sách cùng loại ít hơn count, lấy thêm các sản phẩm khác để đủ số lượng gợi ý
            if (results.Count < count)
            {
                var existingIds = results.Select(r => r.ProductId).Append(currentProductId).ToList();
                var fallback = await _dbSet.Include(p => p.Brand)
                                           .Include(p => p.Category)
                                           .Include(p => p.Images)
                                           .Include(p => p.Variants)
                                           .Where(p => p.IsActive && !existingIds.Contains(p.ProductId))
                                           .Take(count - results.Count)
                                           .AsNoTracking()
                                           .ToListAsync();
                results.AddRange(fallback);
            }

            return results;
        }

        public async Task<IEnumerable<Models.Entities.Product>> GetBestsellerProductsAsync(int count = 4)
        {
            return await _dbSet.Include(p => p.Brand)
                              .Include(p => p.Category)
                              .Include(p => p.Images)
                              .Include(p => p.Variants)
                              .Where(p => p.IsActive)
                              .OrderByDescending(p => p.Price) // Hoặc sản phẩm bán chạy nổi bật
                              .Take(count)
                              .AsNoTracking()
                              .ToListAsync();
        }
    }
}

