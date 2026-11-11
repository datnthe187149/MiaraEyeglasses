using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    // Interface định nghĩa: IAdminBrandService
    public interface IAdminBrandService
    {
        Task<IEnumerable<Brand>> GetAllBrandsAsync();
        // Thêm mới dữ liệu: CreateBrand
        Task<(bool Success, string? ErrorMessage)> CreateBrandAsync(string brandName, string? originCountry, string? description);
        // Cập nhật dữ liệu: UpdateBrand
        Task<(bool Success, string? ErrorMessage)> UpdateBrandAsync(int brandId, string brandName, string? originCountry);
        // Đổi trạng thái kích hoạt: ToggleBrandActive
        Task ToggleBrandActiveAsync(int brandId);
    }

    // Class định nghĩa: AdminBrandService
    public class AdminBrandService : IAdminBrandService
    {
        private readonly IAdminBrandRepository _brandRepository;

        // Khởi tạo đối tượng AdminBrandService kèm nạp các phụ thuộc cần thiết
        public AdminBrandService(IAdminBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        // Lấy tất cả thương hiệu
        public async Task<IEnumerable<Brand>> GetAllBrandsAsync()
        {
            return await _brandRepository.GetAllBrandsWithProductsAsync();
        }

        // Thêm mới thương hiệu
        public async Task<(bool Success, string? ErrorMessage)> CreateBrandAsync(string brandName, string? originCountry, string? description)
        {
            if (string.IsNullOrWhiteSpace(brandName))
            {
                return (false, "Tên thương hiệu không được để trống.");
            }

            string country = "Quốc tế";
            if (!string.IsNullOrWhiteSpace(originCountry))
            {
                country = originCountry.Trim();
            }

            var brand = new Brand
            {
                BrandName = brandName.Trim(),
                Country = country,
                Description = description,
                IsActive = true
            };

            await _brandRepository.AddAsync(brand);
            await _brandRepository.SaveChangesAsync();
            return (true, null);
        }

        // Cập nhật thông tin thương hiệu gọng kính
        public async Task<(bool Success, string? ErrorMessage)> UpdateBrandAsync(int brandId, string brandName, string? originCountry)
        {
            if (brandId <= 0)
            {
                return (false, "Mã thương hiệu không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(brandName))
            {
                return (false, "Tên thương hiệu không được để trống.");
            }

            var brand = await _brandRepository.GetBrandByIdAsync(brandId);
            if (brand == null)
            {
                return (false, "Không tìm thấy thương hiệu cần cập nhật.");
            }

            string country = "Quốc tế";
            if (!string.IsNullOrWhiteSpace(originCountry))
            {
                country = originCountry.Trim();
            }

            brand.BrandName = brandName.Trim();
            brand.Country = country;

            await _brandRepository.UpdateBrandAsync(brand);
            return (true, null);
        }

        // Bật / tắt trạng thái phân phối
        public async Task ToggleBrandActiveAsync(int brandId)
        {
            await _brandRepository.ToggleActiveAsync(brandId);
        }
    }
}

