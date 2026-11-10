using MiaraEyeglasses.Repositories.Staff;

namespace MiaraEyeglasses.Services.Staff
{
    public interface IStaffOrderService
    {
        Task<IEnumerable<Models.Entities.Order>> GetAssignedOrdersAsync(byte? status = null);
        Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId);
        Task<(bool Success, string? ErrorMessage)> ProcessLensCuttingAsync(int orderId, int staffId);
        Task<(bool Success, string? ErrorMessage)> ShipOrderAsync(int orderId, int staffId);
        Task<(bool Success, string? ErrorMessage)> CompleteOrderAsync(int orderId, int staffId);
        Task<(bool Success, string? ErrorMessage)> UpdateOrderStatusAsync(int orderId, byte newStatus, int staffId);
    }

    public class StaffOrderService : IStaffOrderService
    {
        private readonly IStaffOrderRepository _staffOrderRepository;

        public StaffOrderService(IStaffOrderRepository staffOrderRepository)
        {
            _staffOrderRepository = staffOrderRepository;
        }

        public async Task<IEnumerable<Models.Entities.Order>> GetAssignedOrdersAsync(byte? status = null)
        {
            return await _staffOrderRepository.GetAssignedOrdersAsync(status);
        }

        public async Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId)
        {
            return await _staffOrderRepository.GetOrderDetailsAsync(orderId);
        }

        public async Task<(bool Success, string? ErrorMessage)> ProcessLensCuttingAsync(int orderId, int staffId)
        {
            return await UpdateOrderStatusAsync(orderId, 2, staffId);
        }

        public async Task<(bool Success, string? ErrorMessage)> ShipOrderAsync(int orderId, int staffId)
        {
            return await UpdateOrderStatusAsync(orderId, 3, staffId);
        }

        public async Task<(bool Success, string? ErrorMessage)> CompleteOrderAsync(int orderId, int staffId)
        {
            return await UpdateOrderStatusAsync(orderId, 4, staffId);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateOrderStatusAsync(int orderId, byte newStatus, int staffId)
        {
            try
            {
                await _staffOrderRepository.UpdateOrderStatusAsync(orderId, newStatus, staffId);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}

