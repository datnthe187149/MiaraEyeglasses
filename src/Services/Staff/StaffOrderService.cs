using MiaraEyeglasses.Repositories.Staff;

namespace MiaraEyeglasses.Services.Staff
{
    // Interface dịch vụ xử lý đơn hàng cho nhân viên
    public interface IStaffOrderService
    {
        Task<IEnumerable<Models.Entities.Order>> GetAssignedOrdersAsync(byte? status = null);
        // Lấy dữ liệu: OrderDetails
        Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId);
        // Xử lý tiến trình: ProcessLensCutting
        Task<(bool Success, string? ErrorMessage)> ProcessLensCuttingAsync(int orderId, int staffId);
        // Thực thi tác vụ: ShipOrder
        Task<(bool Success, string? ErrorMessage)> ShipOrderAsync(int orderId, int staffId);
        // Thực thi tác vụ: CompleteOrder
        Task<(bool Success, string? ErrorMessage)> CompleteOrderAsync(int orderId, int staffId);
        // Cập nhật dữ liệu: UpdateOrderStatus
        Task<(bool Success, string? ErrorMessage)> UpdateOrderStatusAsync(int orderId, byte newStatus, int staffId);
    }

    // Service điều phối cắt tròng, đóng gói và giao hàng
    public class StaffOrderService : IStaffOrderService
    {
        private readonly IStaffOrderRepository _staffOrderRepository;

        // Khởi tạo đối tượng StaffOrderService kèm nạp các phụ thuộc cần thiết
        public StaffOrderService(IStaffOrderRepository staffOrderRepository)
        {
            _staffOrderRepository = staffOrderRepository;
        }

        public async Task<IEnumerable<Models.Entities.Order>> GetAssignedOrdersAsync(byte? status = null)
        {
            return await _staffOrderRepository.GetAssignedOrdersAsync(status);
        }

        // Lấy dữ liệu: OrderDetails
        public async Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId)
        {
            return await _staffOrderRepository.GetOrderDetailsAsync(orderId);
        }

        // Xử lý tiến trình: ProcessLensCutting
        public async Task<(bool Success, string? ErrorMessage)> ProcessLensCuttingAsync(int orderId, int staffId)
        {
            return await UpdateOrderStatusAsync(orderId, 2, staffId);
        }

        // Thực thi tác vụ: ShipOrder
        public async Task<(bool Success, string? ErrorMessage)> ShipOrderAsync(int orderId, int staffId)
        {
            return await UpdateOrderStatusAsync(orderId, 3, staffId);
        }

        // Thực thi tác vụ: CompleteOrder
        public async Task<(bool Success, string? ErrorMessage)> CompleteOrderAsync(int orderId, int staffId)
        {
            return await UpdateOrderStatusAsync(orderId, 4, staffId);
        }

        // Cập nhật dữ liệu: UpdateOrderStatus
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

