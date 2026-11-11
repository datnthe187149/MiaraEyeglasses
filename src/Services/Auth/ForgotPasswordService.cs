using System.Security.Cryptography;
using System.Text;
using MiaraEyeglasses.Repositories.Auth;

namespace MiaraEyeglasses.Services.Auth
{
    // Interface dịch vụ quên mật khẩu
    public interface IForgotPasswordService
    {
        // Thực thi tác vụ: GenerateResetToken
        Task<(bool Success, string? ErrorMessage, string? ResetToken)> GenerateResetTokenAsync(string email);
    }

    // Service sinh token an toàn và gửi liên kết đặt lại mật khẩu
    public class ForgotPasswordService : IForgotPasswordService
    {
        private readonly IForgotPasswordRepository _forgotPasswordRepository;

        // Khởi tạo đối tượng ForgotPasswordService kèm nạp các phụ thuộc cần thiết
        public ForgotPasswordService(IForgotPasswordRepository forgotPasswordRepository)
        {
            _forgotPasswordRepository = forgotPasswordRepository;
        }

        // Thực thi tác vụ: GenerateResetToken
        public async Task<(bool Success, string? ErrorMessage, string? ResetToken)> GenerateResetTokenAsync(string email)
        {
            var user = await _forgotPasswordRepository.GetByEmailAsync(email);
            if (user == null)
                return (false, "Không tìm thấy tài khoản với email này.", null);

            var rawToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var tokenHash = ComputeSha256Hash(rawToken);

            await _forgotPasswordRepository.UpdateResetTokenAsync(user, tokenHash, DateTime.UtcNow.AddMinutes(30));
            return (true, null, rawToken);
        }

        // Thực thi tác vụ: ComputeSha256Hash
        private static string ComputeSha256Hash(string rawData)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            foreach (var b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}

