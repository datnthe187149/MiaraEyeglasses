using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Product;

namespace MiaraEyeglasses.Services.Product
{
    public interface ILensService
    {
        Task<IEnumerable<LensOption>> GetActiveLensOptionsAsync();
    }

    public class LensService : ILensService
    {
        private readonly ILensRepository _lensRepository;

        public LensService(ILensRepository lensRepository)
        {
            _lensRepository = lensRepository;
        }

        public async Task<IEnumerable<LensOption>> GetActiveLensOptionsAsync()
        {
            return await _lensRepository.GetActiveLensOptionsAsync();
        }
    }
}

