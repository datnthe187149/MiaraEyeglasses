using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Repositories.User;

namespace MiaraEyeglasses.Services.User
{
    // Interface dịch vụ thông tin tài khoản người dùng
    public interface IProfileService
    {
        // Lấy dữ liệu: UserProfile
        Task<Models.Entities.User?> GetUserProfileAsync(int userId);
        // Cập nhật dữ liệu: UpdateProfile
        Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(int userId, UpdateProfileViewModel model);
        // Thực thi tác vụ: ChangePassword
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(int userId, ChangePasswordViewModel model);
    }

    // Service cập nhật thông tin cá nhân khách hàng
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;

        // Khởi tạo đối tượng ProfileService kèm nạp các phụ thuộc cần thiết
        public ProfileService(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        // Lấy dữ liệu: UserProfile
        public async Task<Models.Entities.User?> GetUserProfileAsync(int userId)
        {
            return await _profileRepository.GetUserProfileWithDetailsAsync(userId);
        }

        // Cập nhật dữ liệu: UpdateProfile
        public async Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(int userId, UpdateProfileViewModel model)
        {
            var user = await _profileRepository.GetByIdAsync(userId);
            if (user == null) return (false, "Người dùng không tồn tại.");

            var phoneUsed = await _profileRepository.IsPhoneUsedByOtherUserAsync(model.Phone, userId);
            if (phoneUsed) return (false, "Số điện thoại này đã được sử dụng bởi tài khoản khác.");

            user.FullName = model.FullName;
            user.Phone = model.Phone;
            user.Gender = model.Gender;
            user.BirthDate = model.BirthDate;
            user.UpdatedAt = DateTime.UtcNow;

            await _profileRepository.SaveChangesAsync();
            return (true, null);
        }

        // Thực thi tác vụ: ChangePassword
        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(int userId, ChangePasswordViewModel model)
        {
            var user = await _profileRepository.GetByIdAsync(userId);
            if (user == null) return (false, "Người dùng không tồn tại.");

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                return (false, "Mật khẩu hiện tại không chính xác.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _profileRepository.SaveChangesAsync();
            return (true, null);
        }
    }
}

