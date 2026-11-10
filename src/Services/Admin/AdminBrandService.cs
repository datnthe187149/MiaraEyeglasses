using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    public interface IAdminBrandService
    {
        Task<IEnumerable<Brand>> GetAllBrandsAsync();
        Task<(bool Success, string? ErrorMessage)> CreateBrandAsync(string brandName, string? originCountry, string? description);
        Task ToggleBrandActiveAsync(int brandId);
    }

    public class AdminBrandService : IAdminBrandService
    {
        private readonly IAdminBrandRepository _brandRepository;

        public AdminBrandService(IAdminBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public async Task<IEnumerable<Brand>> GetAllBrandsAsync()
        {
            return await _brandRepository.GetAllBrandsWithProductsAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateBrandAsync(string brandName, string? originCountry, string? description)
        {
            var brand = new Brand
            {
                BrandName = brandName,
                Country = originCountry,
                Description = description,
                IsActive = true
            };

            await _brandRepository.AddAsync(brand);
            await _brandRepository.SaveChangesAsync();
            return (true, null);
        }

        public async Task ToggleBrandActiveAsync(int brandId)
        {
            await _brandRepository.ToggleActiveAsync(brandId);
        }
    }
}

