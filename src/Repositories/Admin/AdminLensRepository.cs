using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Admin
{
    public interface IAdminLensRepository : IBaseRepository<LensOption>
    {
        Task<IEnumerable<LensOption>> GetAllLensesAsync();
        Task ToggleActiveAsync(int lensOptionId);
    }

    public class AdminLensRepository : BaseRepository<LensOption>, IAdminLensRepository
    {
        public AdminLensRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<LensOption>> GetAllLensesAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task ToggleActiveAsync(int lensOptionId)
        {
            var l = await _dbSet.FindAsync(lensOptionId);
            if (l != null)
            {
                l.IsActive = !l.IsActive;
                await _context.SaveChangesAsync();
            }
        }
    }
}

