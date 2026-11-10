using UserEntity = MiaraEyeglasses.Models.Entities.User;
using MiaraEyeglasses.Repositories.Auth;

namespace MiaraEyeglasses.Services.Auth
{
    // Giao diện dịch vụ xác thực đăng nhập
    public interface ILoginService
    {
        // Xác thực thông tin tài khoản bằng Email và Mật khẩu
        Task<(bool Success, string? ErrorMessage, UserEntity? User)> AuthenticateAsync(string email, string password);
    }

    // Triển khai nghiệp vụ xác thực đăng nhập (Business Logic Layer)
    public class LoginService : ILoginService
    {
        private readonly ILoginRepository _loginRepository;

        // Tiêm phụ thuộc ILoginRepository để truy vấn CSDL
        public LoginService(ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }

        // Xử lý logic đăng nhập: tìm User, check IsActive, so khớp hash BCrypt
        public async Task<(bool Success, string? ErrorMessage, UserEntity? User)> AuthenticateAsync(string email, string password)
        {
            // Bước 1: Gọi Repository để tìm người dùng trong CSDL theo Email
            var user = await _loginRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return (false, "Email hoặc mật khẩu không chính xác.", null);
            }

            // Bước 2: Kiểm tra cờ trạng thái tài khoản
            if (!user.IsActive)
            {
                return (false, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.", null);
            }

            // Bước 3: So khớp mật khẩu với chuỗi hash trong CSDL bằng thuật toán BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return (false, "Email hoặc mật khẩu không chính xác.", null);
            }

            // Bước 4: Đăng nhập thành công -> Trả về User
            return (true, null, user);
        }
    }
}
