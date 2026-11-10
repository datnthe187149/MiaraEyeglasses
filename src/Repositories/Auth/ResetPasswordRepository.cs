using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using UserEntity = MiaraEyeglasses.Models.Entities.User;

namespace MiaraEyeglasses.Repositories.Auth
{
    public interface IResetPasswordRepository
    {
        Task<UserEntity?> GetByEmailAsync(string email);
        Task UpdatePasswordAsync(UserEntity user, string newPasswordHash);
    }

    public class ResetPasswordRepository : IResetPasswordRepository
    {
        private readonly EyeglassesDbContext _context;

        public ResetPasswordRepository(EyeglassesDbContext context)
        {
            _context = context;
        }

        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task UpdatePasswordAsync(UserEntity user, string newPasswordHash)
        {
            user.PasswordHash = newPasswordHash;
            user.ResetPasswordTokenHash = null;
            user.ResetPasswordExpire = null;
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
