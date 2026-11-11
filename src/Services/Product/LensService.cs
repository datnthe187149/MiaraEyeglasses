using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Product;

namespace MiaraEyeglasses.Services.Product
{
    // Interface dịch vụ tròng kính khách hàng
    public interface ILensService
    {
        Task<IEnumerable<LensOption>> GetActiveLensOptionsAsync();
    }

    // Service cung cấp danh sách tròng kính mở bán
    public class LensService : ILensService
    {
        private readonly ILensRepository _lensRepository;

        // Khởi tạo đối tượng LensService kèm nạp các phụ thuộc cần thiết
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

