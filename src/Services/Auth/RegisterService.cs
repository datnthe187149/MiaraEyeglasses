using UserEntity = MiaraEyeglasses.Models.Entities.User;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Repositories.Auth;

namespace MiaraEyeglasses.Services.Auth
{
    // Interface dịch vụ đăng ký tài khoản mới
    public interface IRegisterService
    {
        // Thực thi tác vụ: Register
        Task<(bool Success, string? ErrorMessage, UserEntity? User)> RegisterAsync(RegisterViewModel model);
    }

    // Service kiểm tra hợp lệ, mã hóa mật khẩu và tạo tài khoản khách hàng
    public class RegisterService : IRegisterService
    {
        private readonly IRegisterRepository _registerRepository;

        // Khởi tạo đối tượng RegisterService kèm nạp các phụ thuộc cần thiết
        public RegisterService(IRegisterRepository registerRepository)
        {
            _registerRepository = registerRepository;
        }

        // Thực thi tác vụ: Register
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
