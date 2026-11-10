using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using UserEntity = MiaraEyeglasses.Models.Entities.User;

namespace MiaraEyeglasses.Repositories.User
{
    public interface IChangePasswordRepository
    {
        Task<UserEntity?> GetByIdAsync(int userId);
        Task UpdatePasswordAsync(UserEntity user, string newPasswordHash);
    }

    public class ChangePasswordRepository : IChangePasswordRepository
    {
        private readonly EyeglassesDbContext _context;

        public ChangePasswordRepository(EyeglassesDbContext context)
        {
            _context = context;
        }

        public async Task<UserEntity?> GetByIdAsync(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task UpdatePasswordAsync(UserEntity user, string newPasswordHash)
        {
            user.PasswordHash = newPasswordHash;
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
