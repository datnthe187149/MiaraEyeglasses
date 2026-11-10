using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;
using ProductEntity = MiaraEyeglasses.Models.Entities.Product;

namespace MiaraEyeglasses.Repositories.Admin
{
    public interface IAdminProductRepository : IBaseRepository<ProductEntity>
    {
        Task<IEnumerable<ProductEntity>> GetAllProductsWithDetailsAsync();
        Task<(IEnumerable<ProductEntity> Products, int TotalCount)> GetProductsPagedAsync(int page, int pageSize, string? search = null);
        Task AddProductWithVariantAndImageAsync(ProductEntity product, ProductVariant variant, ProductImage image);
        Task AddProductWithVariantsAndImagesAsync(ProductEntity product, IEnumerable<ProductVariant> variants, IEnumerable<ProductImage> images);
        Task ToggleProductActiveAsync(int productId);
        Task<bool> ProductCodeExistsAsync(string productCode);
        Task<Brand> GetOrCreateBrandAsync(string brandName);
        Task<Category> GetOrCreateCategoryAsync(string categoryName, string slug);
    }

    public class AdminProductRepository : BaseRepository<ProductEntity>, IAdminProductRepository
    {
        public AdminProductRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<ProductEntity>> GetAllProductsWithDetailsAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<ProductEntity> Products, int TotalCount)> GetProductsPagedAsync(int page, int pageSize, string? search = null)
        {
            var query = _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(term) || p.ProductCode.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync();
            var products = await query.OrderByDescending(p => p.CreatedAt)
                                      .Skip((page - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToListAsync();

            return (products, totalCount);
        }

        public async Task AddProductWithVariantAndImageAsync(ProductEntity product, ProductVariant variant, ProductImage image)
        {
            await AddProductWithVariantsAndImagesAsync(product, new[] { variant }, new[] { image });
        }

        public async Task AddProductWithVariantsAndImagesAsync(ProductEntity product, IEnumerable<ProductVariant> variants, IEnumerable<ProductImage> images)
        {
            await _dbSet.AddAsync(product);
            await _context.SaveChangesAsync();

            foreach (var v in variants)
            {
                v.ProductId = product.ProductId;
                await _context.ProductVariants.AddAsync(v);
            }

            foreach (var img in images)
            {
                img.ProductId = product.ProductId;
                await _context.ProductImages.AddAsync(img);
            }

            await _context.SaveChangesAsync();
        }

        public async Task ToggleProductActiveAsync(int productId)
        {
            var p = await _dbSet.FindAsync(productId);
            if (p != null)
            {
                p.IsActive = !p.IsActive;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ProductCodeExistsAsync(string productCode)
        {
            return await _dbSet.AnyAsync(p => p.ProductCode == productCode);
        }

        public async Task<Brand> GetOrCreateBrandAsync(string brandName)
        {
            var brand = await _context.Brands.FirstOrDefaultAsync(b => b.BrandName.ToLower() == brandName.ToLower());
            if (brand == null)
            {
                brand = new Brand { BrandName = brandName, IsActive = true };
                await _context.Brands.AddAsync(brand);
                await _context.SaveChangesAsync();
            }
            return brand;
        }

        public async Task<Category> GetOrCreateCategoryAsync(string categoryName, string slug)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName.ToLower() == categoryName.ToLower() || c.Slug.ToLower() == slug.ToLower());
            if (category == null)
            {
                category = new Category { CategoryName = categoryName, Slug = slug, IsActive = true };
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();
            }
            return category;
        }
    }
}
