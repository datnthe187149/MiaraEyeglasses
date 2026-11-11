using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    // Interface dịch vụ quản lý tròng kính cho Admin
    public interface IAdminLensService
    {
        Task<IEnumerable<LensOption>> GetAllLensesAsync();
        // Thêm mới dữ liệu: CreateLens
        Task<(bool Success, string? ErrorMessage)> CreateLensAsync(string lensName, decimal extraPrice, string? description, string? rxLimit);
        // Đổi trạng thái kích hoạt: ToggleLensActive
        Task ToggleLensActiveAsync(int lensOptionId);
        // Cập nhật dữ liệu: UpdateLens
        Task<(bool Success, string? ErrorMessage)> UpdateLensAsync(int lensOptionId, string lensName, decimal refractiveIndex, decimal extraPrice, string? description, string? rxLimit);
    }

    // Service quản lý bảng giá và thông số các loại tròng kính
    public class AdminLensService : IAdminLensService
    {
        private readonly IAdminLensRepository _lensRepository;

        // Khởi tạo đối tượng AdminLensService kèm nạp các phụ thuộc cần thiết
        public AdminLensService(IAdminLensRepository lensRepository)
        {
            _lensRepository = lensRepository;
        }

        public async Task<IEnumerable<LensOption>> GetAllLensesAsync()
        {
            return await _lensRepository.GetAllLensesAsync();
        }

        // Thêm mới dữ liệu: CreateLens
        public async Task<(bool Success, string? ErrorMessage)> CreateLensAsync(string lensName, decimal extraPrice, string? description, string? rxLimit)
        {
            var lens = new LensOption
            {
                LensName = lensName,
                RefractiveIndex = 1.56m,
                ExtraPrice = extraPrice,
                Description = description,
                RxLimit = rxLimit,
                IsActive = true
            };

            await _lensRepository.AddAsync(lens);
            await _lensRepository.SaveChangesAsync();
            return (true, null);
        }

        // Đổi trạng thái kích hoạt: ToggleLensActive
        public async Task ToggleLensActiveAsync(int lensOptionId)
        {
            await _lensRepository.ToggleActiveAsync(lensOptionId);
        }

        // Cập nhật dữ liệu: UpdateLens
        public async Task<(bool Success, string? ErrorMessage)> UpdateLensAsync(
            int lensOptionId, string lensName, decimal refractiveIndex, decimal extraPrice, string? description, string? rxLimit)
        {
            if (string.IsNullOrWhiteSpace(lensName))
            {
                return (false, "Tên gói tròng kính không được để trống.");
            }

            if (extraPrice < 0)
            {
                return (false, "Giá phụ thu tròng kính không được âm.");
            }

            var updated = await _lensRepository.UpdateLensAsync(lensOptionId, lensName.Trim(), refractiveIndex, extraPrice, description, rxLimit);
            if (!updated)
            {
                return (false, "Không tìm thấy gói tròng kính cần cập nhật.");
            }

            return (true, null);
        }
    }
}

