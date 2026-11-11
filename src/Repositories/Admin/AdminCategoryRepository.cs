using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Admin
{
    // Interface kho dữ liệu quản lý danh mục gọng kính
    public interface IAdminCategoryRepository : IBaseRepository<Category>
    {
        Task<IEnumerable<Category>> GetAllCategoriesWithProductsAsync();
        // Đổi trạng thái kích hoạt: ToggleActive
        Task ToggleActiveAsync(int categoryId);
        // Cập nhật dữ liệu: UpdateCategory
        Task<bool> UpdateCategoryAsync(int categoryId, string categoryName, string? slug);
    }

    // Lớp thực thi quản lý danh mục sản phẩm cho Admin
    public class AdminCategoryRepository : BaseRepository<Category>, IAdminCategoryRepository
    {
        // Khởi tạo đối tượng AdminCategoryRepository kèm nạp các phụ thuộc cần thiết
        public AdminCategoryRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<Category>> GetAllCategoriesWithProductsAsync()
        {
            return await _dbSet.Include(c => c.Products).ToListAsync();
        }

        // Đổi trạng thái kích hoạt: ToggleActive
        public async Task ToggleActiveAsync(int categoryId)
        {
            var cat = await _dbSet.FindAsync(categoryId);
            if (cat != null)
            {
                cat.IsActive = !cat.IsActive;
                await _context.SaveChangesAsync();
            }
        }

        // Cập nhật dữ liệu: UpdateCategory
        public async Task<bool> UpdateCategoryAsync(int categoryId, string categoryName, string? slug)
        {
            var cat = await _dbSet.FindAsync(categoryId);
            if (cat == null)
            {
                return false;
            }

            cat.CategoryName = categoryName.Trim();
            if (!string.IsNullOrWhiteSpace(slug))
            {
                cat.Slug = slug.Trim().ToLower();
            }
            else
            {
                cat.Slug = categoryName.Trim().ToLower().Replace(" ", "-");
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

