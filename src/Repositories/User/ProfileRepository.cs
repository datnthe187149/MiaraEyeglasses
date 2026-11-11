using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.User
{
    // Interface kho dữ liệu hồ sơ thông tin cá nhân khách hàng
    public interface IProfileRepository : IBaseRepository<Models.Entities.User>
    {
        // Lấy dữ liệu: UserProfileWithDetails
        Task<Models.Entities.User?> GetUserProfileWithDetailsAsync(int userId);
        // Kiểm tra điều kiện: IsPhoneUsedByOtherUser
        Task<bool> IsPhoneUsedByOtherUserAsync(string phone, int userId);
    }

    // Lớp thực thi truy xuất thông tin tài khoản và thông tin cá nhân
    public class ProfileRepository : BaseRepository<Models.Entities.User>, IProfileRepository
    {
        // Khởi tạo đối tượng ProfileRepository kèm nạp các phụ thuộc cần thiết
        public ProfileRepository(EyeglassesDbContext context) : base(context) { }

        // Lấy dữ liệu: UserProfileWithDetails
        public async Task<Models.Entities.User?> GetUserProfileWithDetailsAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.Role)
                .Include(u => u.Addresses)
                .Include(u => u.Prescriptions)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderStatus)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        // Kiểm tra điều kiện: IsPhoneUsedByOtherUser
        public async Task<bool> IsPhoneUsedByOtherUserAsync(string phone, int userId)
        {
            return await _dbSet.AnyAsync(u => u.Phone == phone && u.UserId != userId);
        }
    }
}

