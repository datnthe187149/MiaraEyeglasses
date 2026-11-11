using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using UserEntity = MiaraEyeglasses.Models.Entities.User;

namespace MiaraEyeglasses.Repositories.Auth
{
    // Interface kho dữ liệu xử lý cấp token quên mật khẩu
    public interface IForgotPasswordRepository
    {
        // Lấy dữ liệu: ByEmail
        Task<UserEntity?> GetByEmailAsync(string email);
        // Cập nhật dữ liệu: UpdateResetToken
        Task UpdateResetTokenAsync(UserEntity user, string tokenHash, DateTime expireAt);
    }

    // Lớp thực thi lưu trữ token đặt lại mật khẩu
    public class ForgotPasswordRepository : IForgotPasswordRepository
    {
        private readonly EyeglassesDbContext _context;

        // Khởi tạo đối tượng ForgotPasswordRepository kèm nạp các phụ thuộc cần thiết
        public ForgotPasswordRepository(EyeglassesDbContext context)
        {
            _context = context;
        }

        // Lấy dữ liệu: ByEmail
        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        // Cập nhật dữ liệu: UpdateResetToken
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
