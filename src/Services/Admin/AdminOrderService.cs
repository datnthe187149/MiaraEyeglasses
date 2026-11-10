using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    public interface IAdminOrderService
    {
        Task<IEnumerable<Models.Entities.Order>> GetAllOrdersAsync(byte? status = null);
        Task<(bool Success, string? ErrorMessage)> RefundOrderAsync(int orderId, int adminId, string reason, decimal? refundAmount = null);
    }

    public class AdminOrderService : IAdminOrderService
    {
        private readonly IAdminOrderRepository _adminOrderRepository;

        public AdminOrderService(IAdminOrderRepository adminOrderRepository)
        {
            _adminOrderRepository = adminOrderRepository;
        }

        public async Task<IEnumerable<Models.Entities.Order>> GetAllOrdersAsync(byte? status = null)
        {
            return await _adminOrderRepository.GetAllOrdersAsync(status);
        }

        public async Task<(bool Success, string? ErrorMessage)> RefundOrderAsync(int orderId, int adminId, string reason, decimal? refundAmount = null)
        {
            try
            {
                string refundTxnNo = $"REFUND_{DateTime.UtcNow:yyyyMMddHHmmss}";
                await _adminOrderRepository.RefundOrderAsync(orderId, adminId, refundTxnNo, reason, refundAmount);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}

