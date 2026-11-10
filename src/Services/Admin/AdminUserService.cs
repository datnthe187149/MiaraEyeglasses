using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    public interface IAdminUserService
    {
        Task<IEnumerable<Models.Entities.User>> GetUsersAsync(int? roleId = null, string? search = null);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<(bool Success, string? ErrorMessage)> CreateStaffUserAsync(string fullName, string email, string phone, string password);
        Task ToggleUserActiveAsync(int userId);
    }

    public class AdminUserService : IAdminUserService
    {
        private readonly IAdminUserRepository _userRepository;

        public AdminUserService(IAdminUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<Models.Entities.User>> GetUsersAsync(int? roleId = null, string? search = null)
        {
            return await _userRepository.GetUsersAsync(roleId, search);
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _userRepository.GetAllRolesAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateStaffUserAsync(string fullName, string email, string phone, string password)
        {
            var exists = await _userRepository.EmailExistsAsync(email);
            if (exists)
                return (false, "Email này đã được sử dụng.");

            var staffUser = new Models.Entities.User
            {
                FullName = fullName,
                Email = email,
                Phone = phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(staffUser);
            await _userRepository.SaveChangesAsync();
            return (true, null);
        }

        public async Task ToggleUserActiveAsync(int userId)
        {
            await _userRepository.ToggleActiveAsync(userId);
        }
    }
}

