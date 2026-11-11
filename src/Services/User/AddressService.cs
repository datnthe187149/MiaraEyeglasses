using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Repositories.User;

namespace MiaraEyeglasses.Services.User
{
    // Interface dịch vụ quản lý sổ địa chỉ giao hàng
    public interface IAddressService
    {
        Task<IEnumerable<UserAddress>> GetAddressesAsync(int userId);
        // Thêm mới dữ liệu: AddAddress
        Task<bool> AddAddressAsync(int userId, AddAddressViewModel model);
        // Xóa dữ liệu: DeleteAddress
        Task<bool> DeleteAddressAsync(int userId, int addressId);
        // Thực thi tác vụ: SetDefaultAddress
        Task<bool> SetDefaultAddressAsync(int userId, int addressId);
    }

    // Service triển khai các nghiệp vụ thêm, sửa, xóa địa chỉ nhận hàng
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;

        // Khởi tạo đối tượng AddressService kèm nạp các phụ thuộc cần thiết
        public AddressService(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<IEnumerable<UserAddress>> GetAddressesAsync(int userId)
        {
            return await _addressRepository.GetAddressesByUserIdAsync(userId);
        }

        // Thêm mới dữ liệu: AddAddress
        public async Task<bool> AddAddressAsync(int userId, AddAddressViewModel model)
        {
            if (model.IsDefault)
            {
                await _addressRepository.ClearDefaultAddressesAsync(userId);
            }
            else
            {
                var hasAny = await _addressRepository.HasAnyAddressAsync(userId);
                if (!hasAny) model.IsDefault = true;
            }

            var addr = new UserAddress
            {
                UserId = userId,
                RecipientName = model.RecipientName,
                Phone = model.Phone,
                Province = model.Province,
                DistrictWard = model.DistrictWard,
                StreetAddress = model.StreetAddress,
                IsDefault = model.IsDefault,
                CreatedAt = DateTime.UtcNow
            };

            await _addressRepository.AddAsync(addr);
            await _addressRepository.SaveChangesAsync();
            return true;
        }

        // Xóa dữ liệu: DeleteAddress
        public async Task<bool> DeleteAddressAsync(int userId, int addressId)
        {
            var addr = await _addressRepository.GetUserAddressByIdAsync(userId, addressId);
            if (addr == null) return false;

            _addressRepository.Remove(addr);
            await _addressRepository.SaveChangesAsync();
            return true;
        }

        // Thực thi tác vụ: SetDefaultAddress
        public async Task<bool> SetDefaultAddressAsync(int userId, int addressId)
        {
            var addresses = await _addressRepository.GetAddressesByUserIdAsync(userId);
            var target = addresses.FirstOrDefault(a => a.AddressId == addressId);
            if (target == null) return false;

            await _addressRepository.ClearDefaultAddressesAsync(userId);
            target.IsDefault = true;
            _addressRepository.Update(target);
            await _addressRepository.SaveChangesAsync();
            return true;
        }
    }
}

