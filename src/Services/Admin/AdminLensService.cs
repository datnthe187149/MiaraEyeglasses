using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    public interface IAdminLensService
    {
        Task<IEnumerable<LensOption>> GetAllLensesAsync();
        Task<(bool Success, string? ErrorMessage)> CreateLensAsync(string lensName, decimal extraPrice, string? description, string? rxLimit);
        Task ToggleLensActiveAsync(int lensOptionId);
        Task<(bool Success, string? ErrorMessage)> UpdateLensAsync(int lensOptionId, string lensName, decimal refractiveIndex, decimal extraPrice, string? description, string? rxLimit);
    }

    public class AdminLensService : IAdminLensService
    {
        private readonly IAdminLensRepository _lensRepository;

        public AdminLensService(IAdminLensRepository lensRepository)
        {
            _lensRepository = lensRepository;
        }

        public async Task<IEnumerable<LensOption>> GetAllLensesAsync()
        {
            return await _lensRepository.GetAllLensesAsync();
        }

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

        public async Task ToggleLensActiveAsync(int lensOptionId)
        {
            await _lensRepository.ToggleActiveAsync(lensOptionId);
        }

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

