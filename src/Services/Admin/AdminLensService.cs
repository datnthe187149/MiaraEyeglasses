using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    public interface IAdminLensService
    {
        Task<IEnumerable<LensOption>> GetAllLensesAsync();
        Task<(bool Success, string? ErrorMessage)> CreateLensAsync(string lensName, decimal extraPrice, string? description, string? rxLimit);
        Task ToggleLensActiveAsync(int lensOptionId);
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
    }
}

