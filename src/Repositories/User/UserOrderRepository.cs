using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;

namespace MiaraEyeglasses.Repositories.User
{
    // Interface kho dữ liệu lịch sử đơn hàng của người dùng
    public interface IUserOrderRepository
    {
        Task<IEnumerable<MiaraEyeglasses.Models.Entities.Order>> GetOrdersByUserAsync(int userId);
        // Lấy dữ liệu: OrderById
        Task<MiaraEyeglasses.Models.Entities.Order?> GetOrderByIdAsync(int orderId, int userId);
        // Cập nhật dữ liệu: UpdateOrder
        Task UpdateOrderAsync(MiaraEyeglasses.Models.Entities.Order order);
    }

    // Lớp thực thi truy xuất danh sách đơn hàng của khách hàng
    public class UserOrderRepository : IUserOrderRepository
    {
        private readonly EyeglassesDbContext _context;

        // Khởi tạo đối tượng UserOrderRepository kèm nạp các phụ thuộc cần thiết
        public UserOrderRepository(EyeglassesDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MiaraEyeglasses.Models.Entities.Order>> GetOrdersByUserAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.Items)
                .ThenInclude(i => i.Variant)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // Lấy dữ liệu: OrderById
        public async Task<MiaraEyeglasses.Models.Entities.Order?> GetOrderByIdAsync(int orderId, int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.Items)
                .ThenInclude(i => i.Variant)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);
        }

        // Cập nhật dữ liệu: UpdateOrder
        public async Task UpdateOrderAsync(MiaraEyeglasses.Models.Entities.Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}
