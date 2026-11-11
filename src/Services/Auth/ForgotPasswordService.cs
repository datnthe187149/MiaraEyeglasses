using MiaraEyeglasses.Helpers;
using MiaraEyeglasses.Repositories.Auth;

namespace MiaraEyeglasses.Services.Auth
{
    // Interface dịch vụ quên mật khẩu và đặt lại mật khẩu bằng Session + OTP (chuẩn CatBaBooking)
    public interface IForgotPasswordService
    {
        // Sinh mã OTP và gửi qua email cho người dùng
        Task<string?> SendOTPAsync(string email);

        // Alias tương thích đồng bộ chuẩn CatBaBooking
        string? SendOTP(string email);

        // Đặt lại mật khẩu mới cho tài khoản
        Task<bool> ResetPasswordAsync(string email, string newPassword);

        // Alias tương thích đồng bộ chuẩn CatBaBooking
        bool ResetPassword(string email, string newPassword);
    }

    // Triển khai dịch vụ Quên mật khẩu theo cơ chế Session + OTP
    public class ForgotPasswordService : IForgotPasswordService
    {
        private readonly IForgotPasswordRepository _forgotPasswordRepository;
        private readonly IEmailService _emailService;

        public ForgotPasswordService(
            IForgotPasswordRepository forgotPasswordRepository,
            IEmailService emailService)
        {
            _forgotPasswordRepository = forgotPasswordRepository;
            _emailService = emailService;
        }

        // Sinh mã OTP 6 số và gửi đến email của tài khoản
        public async Task<string?> SendOTPAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var user = await _forgotPasswordRepository.GetActiveUserByEmailAsync(email);
            if (user == null) return null;

            // Sinh mã OTP ngẫu nhiên 6 chữ số
            string otp = new Random().Next(100000, 999999).ToString();

            // Gửi email chứa mã OTP
            await _emailService.SendOtpEmailAsync(user.Email, otp);

            return otp;
        }

        public string? SendOTP(string email)
        {
            return SendOTPAsync(email).GetAwaiter().GetResult();
        }

        // Cập nhật mật khẩu mới bằng cách băm qua PasswordHelper (BCrypt)
        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPassword))
                return false;

            var user = await _forgotPasswordRepository.GetByEmailAsync(email);
            if (user == null) return false;

            // Băm mật khẩu mới bằng BCrypt và lưu vào CSDL
            user.PasswordHash = PasswordHelper.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _forgotPasswordRepository.UpdateUserAsync(user);
            return true;
        }

        public bool ResetPassword(string email, string newPassword)
        {
            return ResetPasswordAsync(email, newPassword).GetAwaiter().GetResult();
        }
    }
}
