using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using UserEntity = MiaraEyeglasses.Models.Entities.User;

namespace MiaraEyeglasses.Repositories.Auth
{
    public interface IRegisterRepository
    {
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> IsPhoneExistsAsync(string phone);
        Task<UserEntity> CreateUserAsync(UserEntity user);
    }

    public class RegisterRepository : IRegisterRepository
    {
        private readonly EyeglassesDbContext _context;

        public RegisterRepository(EyeglassesDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> IsPhoneExistsAsync(string phone)
        {
            return await _context.Users.AnyAsync(u => u.Phone == phone);
        }

        public async Task<UserEntity> CreateUserAsync(UserEntity user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
