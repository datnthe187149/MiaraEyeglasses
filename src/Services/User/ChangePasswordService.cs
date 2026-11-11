using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Repositories.User;

namespace MiaraEyeglasses.Services.User
{
    // Interface dịch vụ đổi mật khẩu tài khoản
    public interface IChangePasswordService
    {
        // Thực thi tác vụ: ChangePassword
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(int userId, ChangePasswordViewModel model);
    }

    // Service kiểm tra mật khẩu hiện tại và lưu mật khẩu mới
    public class ChangePasswordService : IChangePasswordService
    {
        private readonly IChangePasswordRepository _changePasswordRepository;

        // Khởi tạo đối tượng ChangePasswordService kèm nạp các phụ thuộc cần thiết
        public ChangePasswordService(IChangePasswordRepository changePasswordRepository)
        {
            _changePasswordRepository = changePasswordRepository;
        }

        // Thực thi tác vụ: ChangePassword
        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(int userId, ChangePasswordViewModel model)
        {
            var user = await _changePasswordRepository.GetByIdAsync(userId);
            if (user == null)
                return (false, "Người dùng không tồn tại.");

            bool isOldValid = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash);
            if (!isOldValid)
                return (false, "Mật khẩu hiện tại không chính xác.");

            string newHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _changePasswordRepository.UpdatePasswordAsync(user, newHash);

            return (true, null);
        }
    }
}
