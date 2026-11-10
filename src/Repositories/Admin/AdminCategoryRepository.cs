using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Admin
{
    public interface IAdminCategoryRepository : IBaseRepository<Category>
    {
        Task<IEnumerable<Category>> GetAllCategoriesWithProductsAsync();
        Task ToggleActiveAsync(int categoryId);
        Task<bool> UpdateCategoryAsync(int categoryId, string categoryName, string? slug);
    }

    public class AdminCategoryRepository : BaseRepository<Category>, IAdminCategoryRepository
    {
        public AdminCategoryRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<Category>> GetAllCategoriesWithProductsAsync()
        {
            return await _dbSet.Include(c => c.Products).ToListAsync();
        }

        public async Task ToggleActiveAsync(int categoryId)
        {
            var cat = await _dbSet.FindAsync(categoryId);
            if (cat != null)
            {
                cat.IsActive = !cat.IsActive;
                await _context.SaveChangesAsync();
            }
        }

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

