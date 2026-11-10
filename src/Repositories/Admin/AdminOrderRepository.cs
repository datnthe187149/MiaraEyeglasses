using MiaraEyeglasses.Repositories.Order;

namespace MiaraEyeglasses.Repositories.Admin
{
    public interface IAdminOrderRepository
    {
        Task<IEnumerable<Models.Entities.Order>> GetAllOrdersAsync(byte? status = null);
        Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId);
    }

    public class AdminOrderRepository : IAdminOrderRepository
    {
        private readonly IOrderRepository _orderRepository;

        public AdminOrderRepository(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // Lấy toàn bộ danh sách đơn hàng cho Admin theo bộ lọc trạng thái
        public async Task<IEnumerable<Models.Entities.Order>> GetAllOrdersAsync(byte? status = null)
        {
            return await _orderRepository.GetAllOrdersForAdminAsync(status);
        }

        // Lấy thông tin chi tiết một đơn hàng kèm sản phẩm, tròng kính và nhật ký giao dịch
        public async Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId)
        {
            return await _orderRepository.GetOrderWithDetailsAsync(orderId);
        }
    }
}

