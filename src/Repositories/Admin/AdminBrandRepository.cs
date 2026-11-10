using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Admin
{
    public interface IAdminBrandRepository : IBaseRepository<Brand>
    {
        Task<IEnumerable<Brand>> GetAllBrandsWithProductsAsync();
        Task ToggleActiveAsync(int brandId);
    }

    public class AdminBrandRepository : BaseRepository<Brand>, IAdminBrandRepository
    {
        public AdminBrandRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<Brand>> GetAllBrandsWithProductsAsync()
        {
            return await _dbSet.Include(b => b.Products).ToListAsync();
        }

        public async Task ToggleActiveAsync(int brandId)
        {
            var brand = await _dbSet.FindAsync(brandId);
            if (brand != null)
            {
                brand.IsActive = !brand.IsActive;
                await _context.SaveChangesAsync();
            }
        }
    }
}

