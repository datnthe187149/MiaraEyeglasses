using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using UserEntity = MiaraEyeglasses.Models.Entities.User;

namespace MiaraEyeglasses.Repositories.Auth
{
    // Interface kho dữ liệu xử lý cấp mã OTP quên mật khẩu
    public interface IForgotPasswordRepository
    {
        // Lấy thông tin tài khoản theo Email
        Task<UserEntity?> GetByEmailAsync(string email);

        // Lấy thông tin tài khoản đang hoạt động (IsActive = true) theo Email (chuẩn CatBaBooking)
        Task<UserEntity?> GetActiveUserByEmailAsync(string email);

        // Cập nhật thông tin User
        Task UpdateUserAsync(UserEntity user);
    }

    // Lớp thực thi lưu trữ và cập nhật trạng thái User
    public class ForgotPasswordRepository : IForgotPasswordRepository
    {
        private readonly EyeglassesDbContext _context;

        public ForgotPasswordRepository(EyeglassesDbContext context)
        {
            _context = context;
        }

        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            string cleanEmail = email.Trim().ToLower();
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail);
        }

        public async Task<UserEntity?> GetActiveUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            string cleanEmail = email.Trim().ToLower();
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == cleanEmail && u.IsActive);
        }

        public async Task UpdateUserAsync(UserEntity user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
