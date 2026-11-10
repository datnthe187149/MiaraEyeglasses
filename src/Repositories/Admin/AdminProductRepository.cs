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
        Task<(IEnumerable<ProductEntity> Products, int TotalCount)> GetProductsPagedAsync(int page, int pageSize, string? search = null, int? categoryId = null, int? brandId = null, bool? isActive = null);
        Task<ProductEntity?> GetProductByIdWithDetailsAsync(int productId);
        Task AddProductWithVariantAndImageAsync(ProductEntity product, ProductVariant variant, ProductImage image);
        Task AddProductWithVariantsAndImagesAsync(ProductEntity product, IEnumerable<ProductVariant> variants, IEnumerable<ProductImage> images);
        Task UpdateProductWithVariantsAndImagesAsync(ProductEntity product, IEnumerable<ProductVariant>? updatedVariants, ProductImage? newPrimaryImage, IEnumerable<ProductImage>? newGalleryImages);
        Task ToggleProductActiveAsync(int productId);
        Task<bool> ProductCodeExistsAsync(string productCode);
        Task<bool> ProductCodeExistsAsync(string productCode, int excludeProductId = 0);
        Task<bool> DeleteProductImageAsync(int imageId);
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

        public async Task<(IEnumerable<ProductEntity> Products, int TotalCount)> GetProductsPagedAsync(
            int page, int pageSize, string? search = null, int? categoryId = null, int? brandId = null, bool? isActive = null)
        {
            var query = _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .AsQueryable();

            // 1. Lọc theo từ khóa tìm kiếm (Tên sản phẩm hoặc Mã SKU)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(term) || p.ProductCode.ToLower().Contains(term));
            }

            // 2. Lọc theo Danh mục
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // 3. Lọc theo Thương hiệu
            if (brandId.HasValue && brandId.Value > 0)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            // 4. Lọc theo Trạng thái hiển thị / ẩn
            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();
            var products = await query.OrderByDescending(p => p.CreatedAt)
                                      .Skip((page - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToListAsync();

            return (products, totalCount);
        }

        public async Task<ProductEntity?> GetProductByIdWithDetailsAsync(int productId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
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

        public async Task<bool> ProductCodeExistsAsync(string productCode, int excludeProductId = 0)
        {
            if (excludeProductId > 0)
            {
                return await _dbSet.AnyAsync(p => p.ProductCode == productCode && p.ProductId != excludeProductId);
            }
            return await _dbSet.AnyAsync(p => p.ProductCode == productCode);
        }

        public async Task UpdateProductWithVariantsAndImagesAsync(
            ProductEntity product,
            IEnumerable<ProductVariant>? updatedVariants,
            ProductImage? newPrimaryImage,
            IEnumerable<ProductImage>? newGalleryImages)
        {
            // 1. Nếu có ảnh đại diện mới được chọn/upload
            if (newPrimaryImage != null)
            {
                var oldPrimaries = await _context.ProductImages
                    .Where(i => i.ProductId == product.ProductId && i.IsPrimary)
                    .ToListAsync();
                foreach (var old in oldPrimaries)
                {
                    old.IsPrimary = false;
                }

                newPrimaryImage.ProductId = product.ProductId;
                await _context.ProductImages.AddAsync(newPrimaryImage);
            }

            // 2. Nếu có ảnh phụ mới
            if (newGalleryImages != null)
            {
                foreach (var gImg in newGalleryImages)
                {
                    gImg.ProductId = product.ProductId;
                    await _context.ProductImages.AddAsync(gImg);
                }
            }

            // Lưu toàn bộ thay đổi của Product và các Variant đã được tracking
            await _context.SaveChangesAsync();
        }

        // Xóa một ảnh của sản phẩm (ảnh phụ)
        public async Task<bool> DeleteProductImageAsync(int imageId)
        {
            var img = await _context.ProductImages.FindAsync(imageId);
            if (img == null)
            {
                return false;
            }

            _context.ProductImages.Remove(img);
            await _context.SaveChangesAsync();
            return true;
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
