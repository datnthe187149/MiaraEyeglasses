using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.User
{
    // Interface kho dữ liệu người dùng chung trong hệ thống
    public interface IUserRepository : IBaseRepository<Models.Entities.User>
    {
        // Lấy dữ liệu: ByEmail
        Task<Models.Entities.User?> GetByEmailAsync(string email);
        // Lấy dữ liệu: ByPhone
        Task<Models.Entities.User?> GetByPhoneAsync(string phone);
        // Lấy dữ liệu: UserWithDetails
        Task<Models.Entities.User?> GetUserWithDetailsAsync(int userId);
    }

    // Lớp thực thi truy vấn tài khoản người dùng theo email hoặc số điện thoại
    public class UserRepository : BaseRepository<Models.Entities.User>, IUserRepository
    {
        // Khởi tạo đối tượng UserRepository kèm nạp các phụ thuộc cần thiết
        public UserRepository(EyeglassesDbContext context) : base(context) { }

        // Lấy dữ liệu: ByEmail
        public async Task<Models.Entities.User?> GetByEmailAsync(string email)
        {
            return await _dbSet.Include(u => u.Role)
                               .FirstOrDefaultAsync(u => u.Email == email);
        }

        // Lấy dữ liệu: ByPhone
        public async Task<Models.Entities.User?> GetByPhoneAsync(string phone)
        {
            return await _dbSet.Include(u => u.Role)
                               .FirstOrDefaultAsync(u => u.Phone == phone);
        }

        // Lấy dữ liệu: UserWithDetails
        public async Task<Models.Entities.User?> GetUserWithDetailsAsync(int userId)
        {
            return await _dbSet.Include(u => u.Role)
                               .Include(u => u.Addresses)
                               .Include(u => u.Prescriptions)
                               .FirstOrDefaultAsync(u => u.UserId == userId);
        }
    }
}

