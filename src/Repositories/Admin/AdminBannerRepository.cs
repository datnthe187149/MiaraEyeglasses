using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Admin
{
    public interface IAdminBannerRepository : IBaseRepository<Banner>
    {
        Task<IEnumerable<Banner>> GetAllBannersAsync();
        Task ToggleActiveAsync(int bannerId);
    }

    public class AdminBannerRepository : BaseRepository<Banner>, IAdminBannerRepository
    {
        public AdminBannerRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<Banner>> GetAllBannersAsync()
        {
            return await _dbSet.OrderBy(b => b.SortOrder).ToListAsync();
        }

        public async Task ToggleActiveAsync(int bannerId)
        {
            var b = await _dbSet.FindAsync(bannerId);
            if (b != null)
            {
                b.IsActive = !b.IsActive;
                await _context.SaveChangesAsync();
            }
        }
    }
}

