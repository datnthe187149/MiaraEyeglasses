using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using UserEntity = MiaraEyeglasses.Models.Entities.User;

namespace MiaraEyeglasses.Repositories.Auth
{
    // Interface kho dữ liệu đăng ký tài khoản khách hàng mới
    public interface IRegisterRepository
    {
        // Kiểm tra điều kiện: IsEmailExists
        Task<bool> IsEmailExistsAsync(string email);
        // Kiểm tra điều kiện: IsPhoneExists
        Task<bool> IsPhoneExistsAsync(string phone);
        // Thêm mới dữ liệu: CreateUser
        Task<UserEntity> CreateUserAsync(UserEntity user);
    }

    // Lớp thực thi kiểm tra trùng lặp và lưu người dùng mới
    public class RegisterRepository : IRegisterRepository
    {
        private readonly EyeglassesDbContext _context;

        // Khởi tạo đối tượng RegisterRepository kèm nạp các phụ thuộc cần thiết
        public RegisterRepository(EyeglassesDbContext context)
        {
            _context = context;
        }

        // Kiểm tra điều kiện: IsEmailExists
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        // Kiểm tra điều kiện: IsPhoneExists
        public async Task<bool> IsPhoneExistsAsync(string phone)
        {
            return await _context.Users.AnyAsync(u => u.Phone == phone);
        }

        // Thêm mới dữ liệu: CreateUser
        public async Task<UserEntity> CreateUserAsync(UserEntity user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
