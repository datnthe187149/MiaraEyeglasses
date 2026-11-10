using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    public interface IAdminUserService
    {
        Task<IEnumerable<Models.Entities.User>> GetUsersAsync(int? roleId = null, string? search = null);
        Task<(IEnumerable<Models.Entities.User> Users, int TotalCount, int TotalPages)> GetUsersPagedAsync(
            int page, int pageSize, int? roleId = null, string? search = null);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Models.Entities.User?> GetUserDetailsAsync(int userId);
        Task<(bool Success, string? ErrorMessage)> CreateUserAsync(string fullName, string email, string phone, string password, int roleId, string? gender, DateOnly? birthDate, bool isActive);
        Task<(bool Success, string? ErrorMessage)> CreateStaffUserAsync(string fullName, string email, string phone, string password);
        Task<(bool Success, string Message)> DeleteUserAsync(int userId);
        Task ToggleUserActiveAsync(int userId);
    }

    public class AdminUserService : IAdminUserService
    {
        private readonly IAdminUserRepository _userRepository;

        public AdminUserService(IAdminUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Lấy danh sách tài khoản theo vai trò hoặc từ khóa tìm kiếm
        public async Task<IEnumerable<Models.Entities.User>> GetUsersAsync(int? roleId = null, string? search = null)
        {
            return await _userRepository.GetUsersAsync(roleId, search);
        }

        public async Task<(IEnumerable<Models.Entities.User> Users, int TotalCount, int TotalPages)> GetUsersPagedAsync(
            int page, int pageSize, int? roleId = null, string? search = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (users, totalCount) = await _userRepository.GetUsersPagedAsync(page, pageSize, roleId, search);
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            if (totalPages < 1) totalPages = 1;

            return (users, totalCount, totalPages);
        }

        // Lấy danh sách tất cả các Role
        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _userRepository.GetAllRolesAsync();
        }

        // Lấy chi tiết tài khoản người dùng kèm theo đơn hàng, địa chỉ và đơn kính
        public async Task<Models.Entities.User?> GetUserDetailsAsync(int userId)
        {
            if (userId <= 0)
            {
                return null;
            }
            return await _userRepository.GetUserByIdWithFullDetailsAsync(userId);
        }

        // Thêm mới tài khoản người dùng (Khách hàng, Nhân viên hoặc Quản trị viên)
        public async Task<(bool Success, string? ErrorMessage)> CreateUserAsync(
            string fullName, string email, string phone, string password, int roleId, string? gender, DateOnly? birthDate, bool isActive)
        {
            // Kiểm tra email trùng
            bool exists = await _userRepository.EmailExistsAsync(email);
            if (exists)
            {
                return (false, "Địa chỉ email này đã được sử dụng trong hệ thống.");
            }

            if (roleId <= 0)
            {
                roleId = 1; // Mặc định là Khách hàng
            }

            string cleanGender = "Nam";
            if (!string.IsNullOrWhiteSpace(gender))
            {
                cleanGender = gender.Trim();
            }

            var newUser = new Models.Entities.User
            {
                FullName = fullName.Trim(),
                Email = email.Trim().ToLower(),
                Phone = phone.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = roleId,
                Gender = cleanGender,
                BirthDate = birthDate,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveChangesAsync();
            return (true, null);
        }

        // Tạo tài khoản Nhân viên nhanh (tương thích method cũ)
        public async Task<(bool Success, string? ErrorMessage)> CreateStaffUserAsync(string fullName, string email, string phone, string password)
        {
            return await CreateUserAsync(fullName, email, phone, password, 2, "Nam", null, true);
        }

        // Xóa hoặc khóa tài khoản an toàn
        public async Task<(bool Success, string Message)> DeleteUserAsync(int userId)
        {
            if (userId <= 0)
            {
                return (false, "Mã người dùng không hợp lệ.");
            }
            return await _userRepository.DeleteUserAsync(userId);
        }

        // Đổi trạng thái kích hoạt tài khoản
        public async Task ToggleUserActiveAsync(int userId)
        {
            await _userRepository.ToggleActiveAsync(userId);
        }
    }
}

