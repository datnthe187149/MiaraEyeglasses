using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Staff;

namespace MiaraEyeglasses.Services.Staff
{
    // Interface dịch vụ quản lý tồn kho
    public interface IStaffInventoryService
    {
        Task<IEnumerable<ProductVariant>> GetInventoryVariantsAsync();
        Task<(IEnumerable<ProductVariant> Variants, int TotalCount, int TotalPages, int LowStockCount, int OutOfStockCount, int AllCount)> GetInventoryPagedAsync(
            int page, int pageSize, string? search = null, string? filter = null);
        // Cập nhật dữ liệu: UpdateVariantStock
        Task<(bool Success, string? ErrorMessage)> UpdateVariantStockAsync(int variantId, int newStock);
    }

    // Service kiểm kê tồn kho và cập nhật số lượng theo biến thể
    public class StaffInventoryService : IStaffInventoryService
    {
        private readonly IStaffInventoryRepository _inventoryRepository;

        // Khởi tạo đối tượng StaffInventoryService kèm nạp các phụ thuộc cần thiết
        public StaffInventoryService(IStaffInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<IEnumerable<ProductVariant>> GetInventoryVariantsAsync()
        {
            return await _inventoryRepository.GetAllVariantsWithProductsAsync();
        }

        public async Task<(IEnumerable<ProductVariant> Variants, int TotalCount, int TotalPages, int LowStockCount, int OutOfStockCount, int AllCount)> GetInventoryPagedAsync(
            int page, int pageSize, string? search = null, string? filter = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (variants, totalCount, lowCount, outCount, allCount) = await _inventoryRepository.GetInventoryPagedAsync(page, pageSize, search, filter);
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            if (totalPages < 1) totalPages = 1;

            return (variants, totalCount, totalPages, lowCount, outCount, allCount);
        }

        // Cập nhật dữ liệu: UpdateVariantStock
        public async Task<(bool Success, string? ErrorMessage)> UpdateVariantStockAsync(int variantId, int newStock)
        {
            if (newStock < 0) return (false, "Số lượng tồn kho không được âm.");

            var success = await _inventoryRepository.UpdateStockAsync(variantId, newStock);
            if (!success) return (false, "Không tìm thấy biến thể sản phẩm.");

            return (true, null);
        }
    }
}

