using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.User
{
    public interface IUserRepository : IBaseRepository<Models.Entities.User>
    {
        Task<Models.Entities.User?> GetByEmailAsync(string email);
        Task<Models.Entities.User?> GetByPhoneAsync(string phone);
        Task<Models.Entities.User?> GetUserWithDetailsAsync(int userId);
    }

    public class UserRepository : BaseRepository<Models.Entities.User>, IUserRepository
    {
        public UserRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<Models.Entities.User?> GetByEmailAsync(string email)
        {
            return await _dbSet.Include(u => u.Role)
                               .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Models.Entities.User?> GetByPhoneAsync(string phone)
        {
            return await _dbSet.Include(u => u.Role)
                               .FirstOrDefaultAsync(u => u.Phone == phone);
        }

        public async Task<Models.Entities.User?> GetUserWithDetailsAsync(int userId)
        {
            return await _dbSet.Include(u => u.Role)
                               .Include(u => u.Addresses)
                               .Include(u => u.Prescriptions)
                               .FirstOrDefaultAsync(u => u.UserId == userId);
        }
    }
}

