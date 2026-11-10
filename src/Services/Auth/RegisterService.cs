using UserEntity = MiaraEyeglasses.Models.Entities.User;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Repositories.Auth;

namespace MiaraEyeglasses.Services.Auth
{
    public interface IRegisterService
    {
        Task<(bool Success, string? ErrorMessage, UserEntity? User)> RegisterAsync(RegisterViewModel model);
    }

    public class RegisterService : IRegisterService
    {
        private readonly IRegisterRepository _registerRepository;

        public RegisterService(IRegisterRepository registerRepository)
        {
            _registerRepository = registerRepository;
        }

        public async Task<(bool Success, string? ErrorMessage, UserEntity? User)> RegisterAsync(RegisterViewModel model)
        {
            if (await _registerRepository.IsEmailExistsAsync(model.Email))
                return (false, "Email này đã được đăng ký trong hệ thống.", null);

            if (await _registerRepository.IsPhoneExistsAsync(model.Phone))
                return (false, "Số điện thoại này đã được sử dụng.", null);

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new UserEntity
            {
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                PasswordHash = passwordHash,
                Gender = model.Gender,
                BirthDate = model.BirthDate,
                RoleId = 1, // Customer
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _registerRepository.CreateUserAsync(user);
            return (true, null, createdUser);
        }
    }
}
