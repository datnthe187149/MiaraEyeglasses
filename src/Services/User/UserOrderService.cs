using MiaraEyeglasses.Repositories.User;

namespace MiaraEyeglasses.Services.User
{
    // Interface dịch vụ quản lý đơn hàng của khách hàng
    public interface IUserOrderService
    {
        Task<IEnumerable<MiaraEyeglasses.Models.Entities.Order>> GetOrdersByUserAsync(int userId);
        // Thực thi tác vụ: CancelOrder
        Task<(bool Success, string? ErrorMessage)> CancelOrderAsync(int orderId, int userId, string reason);
    }

    // Service xử lý xem lịch sử và yêu cầu hủy đơn hàng
    public class UserOrderService : IUserOrderService
    {
        private readonly IUserOrderRepository _userOrderRepository;

        // Khởi tạo đối tượng UserOrderService kèm nạp các phụ thuộc cần thiết
        public UserOrderService(IUserOrderRepository userOrderRepository)
        {
            _userOrderRepository = userOrderRepository;
        }

        public async Task<IEnumerable<MiaraEyeglasses.Models.Entities.Order>> GetOrdersByUserAsync(int userId)
        {
            return await _userOrderRepository.GetOrdersByUserAsync(userId);
        }

        // Thực thi tác vụ: CancelOrder
        public async Task<(bool Success, string? ErrorMessage)> CancelOrderAsync(int orderId, int userId, string reason)
        {
            var order = await _userOrderRepository.GetOrderByIdAsync(orderId, userId);
            if (order == null)
                return (false, "Không tìm thấy đơn hàng.");

            if (order.Status != 0)
                return (false, "Chỉ có thể hủy đơn hàng khi đang ở trạng thái 'Chờ thanh toán' (Status = 0). Đơn đã thanh toán không thể tự ý hủy.");

            order.Status = 5; // Cancelled
            order.CancelReason = reason;

            await _userOrderRepository.UpdateOrderAsync(order);
            return (true, null);
        }
    }
}
