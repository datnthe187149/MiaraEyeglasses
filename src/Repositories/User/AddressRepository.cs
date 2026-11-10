using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.User
{
    public interface IAddressRepository : IBaseRepository<UserAddress>
    {
        Task<IEnumerable<UserAddress>> GetAddressesByUserIdAsync(int userId);
        Task<UserAddress?> GetUserAddressByIdAsync(int userId, int addressId);
        Task ClearDefaultAddressesAsync(int userId);
        Task<bool> HasAnyAddressAsync(int userId);
    }

    public class AddressRepository : BaseRepository<UserAddress>, IAddressRepository
    {
        public AddressRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<UserAddress>> GetAddressesByUserIdAsync(int userId)
        {
            return await _dbSet.Where(a => a.UserId == userId).OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt).ToListAsync();
        }

        public async Task<UserAddress?> GetUserAddressByIdAsync(int userId, int addressId)
        {
            return await _dbSet.FirstOrDefaultAsync(a => a.AddressId == addressId && a.UserId == userId);
        }

        public async Task ClearDefaultAddressesAsync(int userId)
        {
            var addresses = await _dbSet.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
            foreach (var addr in addresses) addr.IsDefault = false;
        }

        public async Task<bool> HasAnyAddressAsync(int userId)
        {
            return await _dbSet.AnyAsync(a => a.UserId == userId);
        }
    }
}

