using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using UserEntity = MiaraEyeglasses.Models.Entities.User;

namespace MiaraEyeglasses.Repositories.Auth
{
    // Interface kho dữ liệu đặt lại mật khẩu mới
    public interface IResetPasswordRepository
    {
        // Lấy dữ liệu: ByEmail
        Task<UserEntity?> GetByEmailAsync(string email);
        // Cập nhật dữ liệu: UpdatePassword
        Task UpdatePasswordAsync(UserEntity user, string newPasswordHash);
    }

    // Lớp thực thi cập nhật mật khẩu mới (chuẩn CatBaBooking)
    public class ResetPasswordRepository : IResetPasswordRepository
    {
        private readonly EyeglassesDbContext _context;

        // Khởi tạo đối tượng ResetPasswordRepository kèm nạp các phụ thuộc cần thiết
        public ResetPasswordRepository(EyeglassesDbContext context)
        {
            _context = context;
        }

        // Lấy dữ liệu: ByEmail
        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        // Cập nhật dữ liệu: UpdatePassword
        public async Task UpdatePasswordAsync(UserEntity user, string newPasswordHash)
        {
            user.PasswordHash = newPasswordHash;
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
