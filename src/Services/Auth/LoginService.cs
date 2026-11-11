using UserEntity = MiaraEyeglasses.Models.Entities.User;
using MiaraEyeglasses.Repositories.Auth;
using MiaraEyeglasses.Helpers;

namespace MiaraEyeglasses.Services.Auth
{
    public interface ILoginService
    {
        Task<UserEntity?> LoginAsync(string email, string password);
        UserEntity? Login(string email, string password);
        Task<(bool Success, string? ErrorMessage, UserEntity? User)> AuthenticateAsync(string email, string password);
    }

    public class LoginService : ILoginService
    {
        private readonly ILoginRepository _loginRepository;

        public LoginService(ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }

        public async Task<UserEntity?> LoginAsync(string email, string password)
        {
            var user = await _loginRepository.GetByEmailAsync(email);
            if (user == null || !user.IsActive) return null;

            bool isPasswordValid = PasswordHelper.VerifyPassword(password, user.PasswordHash);
            if (!isPasswordValid) return null;

            return user;
        }

        public UserEntity? Login(string email, string password)
            => LoginAsync(email, password).GetAwaiter().GetResult();

        public async Task<(bool Success, string? ErrorMessage, UserEntity? User)> AuthenticateAsync(string email, string password)
        {
            var user = await LoginAsync(email, password);
            if (user == null) return (false, "Email hoặc mật khẩu không chính xác hoặc tài khoản đang bị khóa.", null);
            return (true, null, user);
        }
    }
}
