using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    public interface IAdminOrderService
    {
        Task<IEnumerable<Models.Entities.Order>> GetAllOrdersAsync(byte? status = null);
        Task<(IEnumerable<Models.Entities.Order> Orders, int TotalCount, int TotalPages)> GetOrdersPagedAsync(
            int page, int pageSize, byte? status = null, string? search = null);
        Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId);
    }

    public class AdminOrderService : IAdminOrderService
    {
        private readonly IAdminOrderRepository _adminOrderRepository;

        public AdminOrderService(IAdminOrderRepository adminOrderRepository)
        {
            _adminOrderRepository = adminOrderRepository;
        }

        // Lấy danh sách tất cả các đơn hàng cho Quản trị viên
        public async Task<IEnumerable<Models.Entities.Order>> GetAllOrdersAsync(byte? status = null)
        {
            return await _adminOrderRepository.GetAllOrdersAsync(status);
        }

        public async Task<(IEnumerable<Models.Entities.Order> Orders, int TotalCount, int TotalPages)> GetOrdersPagedAsync(
            int page, int pageSize, byte? status = null, string? search = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (orders, totalCount) = await _adminOrderRepository.GetOrdersPagedForAdminAsync(page, pageSize, status, search);
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            if (totalPages < 1) totalPages = 1;

            return (orders, totalCount, totalPages);
        }

        // Lấy chi tiết đơn hàng phục vụ màn hình xem chi tiết
        public async Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId)
        {
            if (orderId <= 0)
            {
                return null;
            }
            return await _adminOrderRepository.GetOrderDetailsAsync(orderId);
        }
    }
}

