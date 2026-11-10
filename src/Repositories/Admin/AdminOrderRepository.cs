using MiaraEyeglasses.Repositories.Order;

namespace MiaraEyeglasses.Repositories.Admin
{
    public interface IAdminOrderRepository
    {
        Task<IEnumerable<Models.Entities.Order>> GetAllOrdersAsync(byte? status = null);
        Task RefundOrderAsync(int orderId, int adminId, string refundTxnNo, string reason, decimal? refundAmount = null);
    }

    public class AdminOrderRepository : IAdminOrderRepository
    {
        private readonly IOrderRepository _orderRepository;

        public AdminOrderRepository(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<Models.Entities.Order>> GetAllOrdersAsync(byte? status = null)
        {
            return await _orderRepository.GetAllOrdersForAdminAsync(status);
        }

        public async Task RefundOrderAsync(int orderId, int adminId, string refundTxnNo, string reason, decimal? refundAmount = null)
        {
            await _orderRepository.RefundOrderViaProcedureAsync(orderId, adminId, refundTxnNo, reason, refundAmount);
        }
    }
}

