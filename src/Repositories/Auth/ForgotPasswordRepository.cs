using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using UserEntity = MiaraEyeglasses.Models.Entities.User;

namespace MiaraEyeglasses.Repositories.Auth
{
    public interface IForgotPasswordRepository
    {
        Task<UserEntity?> GetByEmailAsync(string email);
        Task UpdateResetTokenAsync(UserEntity user, string tokenHash, DateTime expireAt);
    }

    public class ForgotPasswordRepository : IForgotPasswordRepository
    {
        private readonly EyeglassesDbContext _context;

        public ForgotPasswordRepository(EyeglassesDbContext context)
        {
            _context = context;
        }

        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task UpdateResetTokenAsync(UserEntity user, string tokenHash, DateTime expireAt)
        {
            user.ResetPasswordTokenHash = tokenHash;
            user.ResetPasswordExpire = expireAt;
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
