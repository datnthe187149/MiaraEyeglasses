using MiaraEyeglasses.Repositories.Order;

namespace MiaraEyeglasses.Repositories.Admin
{
    // Interface kho dữ liệu đơn hàng phân quyền Admin
    public interface IAdminOrderRepository
    {
        Task<IEnumerable<Models.Entities.Order>> GetAllOrdersAsync(byte? status = null);
        Task<(IEnumerable<Models.Entities.Order> Orders, int TotalCount)> GetOrdersPagedForAdminAsync(
            int page, int pageSize, byte? status = null, string? search = null);
        // Lấy dữ liệu: OrderDetails
        Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId);
    }

    // Lớp thực thi truy xuất và cập nhật đơn hàng cho Admin
    public class AdminOrderRepository : IAdminOrderRepository
    {
        private readonly IOrderRepository _orderRepository;

        // Khởi tạo đối tượng AdminOrderRepository kèm nạp các phụ thuộc cần thiết
        public AdminOrderRepository(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // Lấy toàn bộ danh sách đơn hàng cho Admin theo bộ lọc trạng thái
        public async Task<IEnumerable<Models.Entities.Order>> GetAllOrdersAsync(byte? status = null)
        {
            return await _orderRepository.GetAllOrdersForAdminAsync(status);
        }

        public async Task<(IEnumerable<Models.Entities.Order> Orders, int TotalCount)> GetOrdersPagedForAdminAsync(
            int page, int pageSize, byte? status = null, string? search = null)
        {
            return await _orderRepository.GetOrdersPagedForAdminAsync(page, pageSize, status, search);
        }

        // Lấy thông tin chi tiết một đơn hàng kèm sản phẩm, tròng kính và nhật ký giao dịch
        public async Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId)
        {
            return await _orderRepository.GetOrderWithDetailsAsync(orderId);
        }
    }
}

