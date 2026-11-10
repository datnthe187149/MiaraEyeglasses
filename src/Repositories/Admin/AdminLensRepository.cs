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
        Task<bool> UpdateLensAsync(int lensOptionId, string lensName, decimal refractiveIndex, decimal extraPrice, string? description, string? rxLimit);
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

        public async Task<bool> UpdateLensAsync(int lensOptionId, string lensName, decimal refractiveIndex, decimal extraPrice, string? description, string? rxLimit)
        {
            var l = await _dbSet.FindAsync(lensOptionId);
            if (l == null)
            {
                return false;
            }

            l.LensName = lensName.Trim();
            if (refractiveIndex > 0)
            {
                l.RefractiveIndex = refractiveIndex;
            }
            l.ExtraPrice = extraPrice;
            l.Description = description?.Trim();
            l.RxLimit = rxLimit?.Trim();

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

