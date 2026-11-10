using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Repositories.User;

namespace MiaraEyeglasses.Services.User
{
    public interface IChangePasswordService
    {
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(int userId, ChangePasswordViewModel model);
    }

    public class ChangePasswordService : IChangePasswordService
    {
        private readonly IChangePasswordRepository _changePasswordRepository;

        public ChangePasswordService(IChangePasswordRepository changePasswordRepository)
        {
            _changePasswordRepository = changePasswordRepository;
        }

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
