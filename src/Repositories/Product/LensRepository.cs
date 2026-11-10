using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Product
{
    public interface ILensRepository : IBaseRepository<LensOption>
    {
        Task<IEnumerable<LensOption>> GetActiveLensOptionsAsync();
    }

    public class LensRepository : BaseRepository<LensOption>, ILensRepository
    {
        public LensRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<LensOption>> GetActiveLensOptionsAsync()
        {
            return await _dbSet.Where(l => l.IsActive).OrderBy(l => l.ExtraPrice).ToListAsync();
        }
    }
}

