using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.User
{
    public interface IProfileRepository : IBaseRepository<Models.Entities.User>
    {
        Task<Models.Entities.User?> GetUserProfileWithDetailsAsync(int userId);
        Task<bool> IsPhoneUsedByOtherUserAsync(string phone, int userId);
    }

    public class ProfileRepository : BaseRepository<Models.Entities.User>, IProfileRepository
    {
        public ProfileRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<Models.Entities.User?> GetUserProfileWithDetailsAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.Role)
                .Include(u => u.Addresses)
                .Include(u => u.Prescriptions)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderStatus)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> IsPhoneUsedByOtherUserAsync(string phone, int userId)
        {
            return await _dbSet.AnyAsync(u => u.Phone == phone && u.UserId != userId);
        }
    }
}

