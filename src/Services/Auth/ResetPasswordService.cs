using MiaraEyeglasses.Helpers;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Repositories.Auth;

namespace MiaraEyeglasses.Services.Auth
{
    // Giao diện xử lý đặt lại mật khẩu mới
    public interface IResetPasswordService
    {
        Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordViewModel model);
    }

    // Nghiệp vụ đặt lại mật khẩu mới
    public class ResetPasswordService : IResetPasswordService
    {
        private readonly IResetPasswordRepository _resetPasswordRepository;

        public ResetPasswordService(IResetPasswordRepository resetPasswordRepository)
        {
            _resetPasswordRepository = resetPasswordRepository;
        }

        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return (false, "Email không hợp lệ.");
            }

            var user = await _resetPasswordRepository.GetByEmailAsync(model.Email);
            if (user == null)
            {
                return (false, "Tài khoản không tồn tại trong hệ thống.");
            }

            string newPasswordHash = PasswordHelper.HashPassword(model.NewPassword);
            await _resetPasswordRepository.UpdatePasswordAsync(user, newPasswordHash);

            return (true, null);
        }
    }
}
