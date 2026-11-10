using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Repositories.User;

namespace MiaraEyeglasses.Services.User
{
    public interface IProfileService
    {
        Task<Models.Entities.User?> GetUserProfileAsync(int userId);
        Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(int userId, UpdateProfileViewModel model);
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(int userId, ChangePasswordViewModel model);
    }

    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;

        public ProfileService(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<Models.Entities.User?> GetUserProfileAsync(int userId)
        {
            return await _profileRepository.GetUserProfileWithDetailsAsync(userId);
        }

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

