using System.Text.RegularExpressions;

namespace MiaraEyeglasses.Helpers
{
    // Tiện ích xử lý băm mật khẩu và kiểm tra độ mạnh mật khẩu (chuẩn CatBaBooking)
    public static class PasswordHelper
    {
        // Băm mật khẩu bằng thuật toán an toàn BCrypt
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // So khớp mật khẩu người dùng nhập với chuỗi hash trong CSDL
        public static bool VerifyPassword(string password, string hashPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashPassword))
                return false;
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashPassword);
            }
            catch
            {
                return false;
            }
        }

        // Kiểm tra mật khẩu có đáp ứng tiêu chuẩn an toàn không
        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
                return false;

            // Kiểm tra tối thiểu 6 ký tự, có thể nới lỏng cho môi trường học tập hoặc áp dụng regex
            return password.Length >= 6;
        }
    }
}

