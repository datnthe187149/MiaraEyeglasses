using System.Security.Cryptography;
using System.Text;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Repositories.Auth;

namespace MiaraEyeglasses.Services.Auth
{
    // Giao diện xử lý đặt lại mật khẩu mới
    public interface IResetPasswordService
    {
        Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordViewModel model);
    }

    // Nghiệp vụ đặt lại mật khẩu khi người dùng bấm vào link xác thực trong email
    public class ResetPasswordService : IResetPasswordService
    {
        private readonly IResetPasswordRepository _resetPasswordRepository;

        public ResetPasswordService(IResetPasswordRepository resetPasswordRepository)
        {
            _resetPasswordRepository = resetPasswordRepository;
        }

        // Thực hiện đặt lại mật khẩu mới: kiểm tra token, kiểm tra hạn dùng, mã hóa mật khẩu mới
        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            // Bước 1: Tìm người dùng trong cơ sở dữ liệu dựa vào địa chỉ Email
            var user = await _resetPasswordRepository.GetByEmailAsync(model.Email);
            if (user == null)
            {
                return (false, "Tài khoản không tồn tại trong hệ thống.");
            }

            // Bước 2: Băm mã token gửi lên bằng thuật toán SHA-256 để so sánh với mã trong CSDL
            // Lý do: Trong CSDL chỉ lưu chuỗi băm của token chứ không lưu token gốc để bảo mật
            string tokenHash = ComputeSha256Hash(model.Token);

            // Bước 3: So sánh mã băm token và kiểm tra thời gian hết hạn của token
            if (user.ResetPasswordTokenHash != tokenHash)
            {
                return (false, "Mã xác thực đổi mật khẩu không hợp lệ.");
            }

            if (user.ResetPasswordExpire < DateTime.UtcNow)
            {
                return (false, "Mã xác thực đổi mật khẩu đã hết hạn (chỉ có hiệu lực trong thời gian quy định).");
            }

            // Bước 4: Mã hóa mật khẩu mới bằng thuật toán bảo mật BCrypt
            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            // Bước 5: Cập nhật mật khẩu mới vào CSDL và xóa token xác thực cũ để không dùng lại được nữa
            await _resetPasswordRepository.UpdatePasswordAsync(user, newPasswordHash);

            return (true, null);
        }

        // Hàm tiện ích băm chuỗi thành mã SHA-256 (64 ký tự hex)
        private static string ComputeSha256Hash(string rawData)
        {
            using (var sha256 = SHA256.Create())
            {
                // Chuyển chuỗi ban đầu thành mảng byte UTF-8
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Ghép từng byte thành chuỗi hệ thập lục phân (hexadecimal)
                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
