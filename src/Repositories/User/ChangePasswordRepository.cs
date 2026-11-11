using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using UserEntity = MiaraEyeglasses.Models.Entities.User;

namespace MiaraEyeglasses.Repositories.User
{
    // Interface kho dữ liệu đổi mật khẩu người dùng
    public interface IChangePasswordRepository
    {
        // Lấy dữ liệu: ById
        Task<UserEntity?> GetByIdAsync(int userId);
        // Cập nhật dữ liệu: UpdatePassword
        Task UpdatePasswordAsync(UserEntity user, string newPasswordHash);
    }

    // Lớp thực thi cập nhật mật khẩu băm mới
    public class ChangePasswordRepository : IChangePasswordRepository
    {
        private readonly EyeglassesDbContext _context;

        // Khởi tạo đối tượng ChangePasswordRepository kèm nạp các phụ thuộc cần thiết
        public ChangePasswordRepository(EyeglassesDbContext context)
        {
            _context = context;
        }

        // Lấy dữ liệu: ById
        public async Task<UserEntity?> GetByIdAsync(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
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
