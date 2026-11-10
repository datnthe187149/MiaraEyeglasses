using MiaraEyeglasses.Repositories.User;

namespace MiaraEyeglasses.Services.User
{
    public interface IUserOrderService
    {
        Task<IEnumerable<MiaraEyeglasses.Models.Entities.Order>> GetOrdersByUserAsync(int userId);
        Task<(bool Success, string? ErrorMessage)> CancelOrderAsync(int orderId, int userId, string reason);
    }

    public class UserOrderService : IUserOrderService
    {
        private readonly IUserOrderRepository _userOrderRepository;

        public UserOrderService(IUserOrderRepository userOrderRepository)
        {
            _userOrderRepository = userOrderRepository;
        }

        public async Task<IEnumerable<MiaraEyeglasses.Models.Entities.Order>> GetOrdersByUserAsync(int userId)
        {
            return await _userOrderRepository.GetOrdersByUserAsync(userId);
        }

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
