using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.User
{
    // Interface kho dữ liệu địa chỉ giao hàng của người dùng
    public interface IAddressRepository : IBaseRepository<UserAddress>
    {
        Task<IEnumerable<UserAddress>> GetAddressesByUserIdAsync(int userId);
        // Lấy dữ liệu: UserAddressById
        Task<UserAddress?> GetUserAddressByIdAsync(int userId, int addressId);
        // Xóa dữ liệu: ClearDefaultAddresses
        Task ClearDefaultAddressesAsync(int userId);
        // Kiểm tra điều kiện: HasAnyAddress
        Task<bool> HasAnyAddressAsync(int userId);
    }

    // Lớp thực thi truy xuất và quản lý sổ địa chỉ giao hàng
    public class AddressRepository : BaseRepository<UserAddress>, IAddressRepository
    {
        // Khởi tạo đối tượng AddressRepository kèm nạp các phụ thuộc cần thiết
        public AddressRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<UserAddress>> GetAddressesByUserIdAsync(int userId)
        {
            return await _dbSet.Where(a => a.UserId == userId).OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt).ToListAsync();
        }

        // Lấy dữ liệu: UserAddressById
        public async Task<UserAddress?> GetUserAddressByIdAsync(int userId, int addressId)
        {
            return await _dbSet.FirstOrDefaultAsync(a => a.AddressId == addressId && a.UserId == userId);
        }

        // Xóa dữ liệu: ClearDefaultAddresses
        public async Task ClearDefaultAddressesAsync(int userId)
        {
            var addresses = await _dbSet.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
            foreach (var addr in addresses) addr.IsDefault = false;
        }

        // Kiểm tra điều kiện: HasAnyAddress
        public async Task<bool> HasAnyAddressAsync(int userId)
        {
            return await _dbSet.AnyAsync(a => a.UserId == userId);
        }
    }
}

