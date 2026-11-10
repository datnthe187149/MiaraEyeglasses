using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Repositories.User;

namespace MiaraEyeglasses.Services.User
{
    public interface IAddressService
    {
        Task<IEnumerable<UserAddress>> GetAddressesAsync(int userId);
        Task<bool> AddAddressAsync(int userId, AddAddressViewModel model);
        Task<bool> DeleteAddressAsync(int userId, int addressId);
        Task<bool> SetDefaultAddressAsync(int userId, int addressId);
    }

    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;

        public AddressService(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<IEnumerable<UserAddress>> GetAddressesAsync(int userId)
        {
            return await _addressRepository.GetAddressesByUserIdAsync(userId);
        }

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

        public async Task<bool> DeleteAddressAsync(int userId, int addressId)
        {
            var addr = await _addressRepository.GetUserAddressByIdAsync(userId, addressId);
            if (addr == null) return false;

            _addressRepository.Remove(addr);
            await _addressRepository.SaveChangesAsync();
            return true;
        }

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

