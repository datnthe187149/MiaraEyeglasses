using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;
using MiaraEyeglasses.Repositories.Order;

namespace MiaraEyeglasses.Repositories.Staff
{
    public interface IStaffOrderRepository : IBaseRepository<Models.Entities.Order>
    {
        Task<IEnumerable<Models.Entities.Order>> GetAssignedOrdersAsync(byte? status = null);
        Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId);
        Task UpdateOrderStatusAsync(int orderId, byte newStatus, int staffId);
    }

    public class StaffOrderRepository : BaseRepository<Models.Entities.Order>, IStaffOrderRepository
    {
        private readonly IOrderRepository _orderRepository;

        public StaffOrderRepository(EyeglassesDbContext context, IOrderRepository orderRepository) : base(context)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<Models.Entities.Order>> GetAssignedOrdersAsync(byte? status = null)
        {
            return await _orderRepository.GetAllOrdersForAdminAsync(status);
        }

        public async Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId)
        {
            return await _orderRepository.GetOrderWithDetailsAsync(orderId);
        }

        public async Task UpdateOrderStatusAsync(int orderId, byte newStatus, int staffId)
        {
            await _orderRepository.UpdateOrderStatusViaProcedureAsync(orderId, newStatus, staffId);
        }
    }
}

