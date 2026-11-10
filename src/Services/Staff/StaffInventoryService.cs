using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Staff;

namespace MiaraEyeglasses.Services.Staff
{
    public interface IStaffInventoryService
    {
        Task<IEnumerable<ProductVariant>> GetInventoryVariantsAsync();
        Task<(bool Success, string? ErrorMessage)> UpdateVariantStockAsync(int variantId, int newStock);
    }

    public class StaffInventoryService : IStaffInventoryService
    {
        private readonly IStaffInventoryRepository _inventoryRepository;

        public StaffInventoryService(IStaffInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<IEnumerable<ProductVariant>> GetInventoryVariantsAsync()
        {
            return await _inventoryRepository.GetAllVariantsWithProductsAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateVariantStockAsync(int variantId, int newStock)
        {
            if (newStock < 0) return (false, "Số lượng tồn kho không được âm.");

            var success = await _inventoryRepository.UpdateStockAsync(variantId, newStock);
            if (!success) return (false, "Không tìm thấy biến thể sản phẩm.");

            return (true, null);
        }
    }
}

