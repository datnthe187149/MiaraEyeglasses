using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;
using MiaraEyeglasses.Repositories.Order;

namespace MiaraEyeglasses.Repositories.Staff
{
    // Interface kho dữ liệu đơn hàng dành cho nhân viên
    public interface IStaffOrderRepository : IBaseRepository<Models.Entities.Order>
    {
        Task<IEnumerable<Models.Entities.Order>> GetAssignedOrdersAsync(byte? status = null);
        // Lấy dữ liệu: OrderDetails
        Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId);
        // Cập nhật dữ liệu: UpdateOrderStatus
        Task UpdateOrderStatusAsync(int orderId, byte newStatus, int staffId);
    }

    // Lớp thực thi truy xuất đơn hàng cho nhân viên kỹ thuật và đóng gói
    public class StaffOrderRepository : BaseRepository<Models.Entities.Order>, IStaffOrderRepository
    {
        private readonly IOrderRepository _orderRepository;

        // Khởi tạo đối tượng StaffOrderRepository kèm nạp các phụ thuộc cần thiết
        public StaffOrderRepository(EyeglassesDbContext context, IOrderRepository orderRepository) : base(context)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<Models.Entities.Order>> GetAssignedOrdersAsync(byte? status = null)
        {
            return await _orderRepository.GetAllOrdersForAdminAsync(status);
        }

        // Lấy dữ liệu: OrderDetails
        public async Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId)
        {
            return await _orderRepository.GetOrderWithDetailsAsync(orderId);
        }

        // Cập nhật dữ liệu: UpdateOrderStatus
        public async Task UpdateOrderStatusAsync(int orderId, byte newStatus, int staffId)
        {
            await _orderRepository.UpdateOrderStatusViaProcedureAsync(orderId, newStatus, staffId);
        }
    }
}

