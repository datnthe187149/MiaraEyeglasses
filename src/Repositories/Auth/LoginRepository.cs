using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using UserEntity = MiaraEyeglasses.Models.Entities.User;

namespace MiaraEyeglasses.Repositories.Auth
{
    // Giao diện truy xuất dữ liệu cho chức năng Đăng nhập (Data Access Layer)
    public interface ILoginRepository
    {
        // Tìm kiếm người dùng theo Email (kèm thông tin Role)
        Task<UserEntity?> GetByEmailAsync(string email);
    }

    // Triển khai truy xuất CSDL bằng Entity Framework Core
    public class LoginRepository : ILoginRepository
    {
        private readonly EyeglassesDbContext _context;

        // Tiêm DbContext thông qua DI Container
        public LoginRepository(EyeglassesDbContext context)
        {
            _context = context;
        }

        // Thực thi câu truy vấn LINQ: SELECT TOP(1) kèm JOIN bảng Roles
        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role) // Eager loading: nạp luôn dữ liệu bảng Roles liên quan
                .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
