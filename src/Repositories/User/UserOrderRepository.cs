using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;

namespace MiaraEyeglasses.Repositories.User
{
    public interface IUserOrderRepository
    {
        Task<IEnumerable<MiaraEyeglasses.Models.Entities.Order>> GetOrdersByUserAsync(int userId);
        Task<MiaraEyeglasses.Models.Entities.Order?> GetOrderByIdAsync(int orderId, int userId);
        Task UpdateOrderAsync(MiaraEyeglasses.Models.Entities.Order order);
    }

    public class UserOrderRepository : IUserOrderRepository
    {
        private readonly EyeglassesDbContext _context;

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

        public async Task<MiaraEyeglasses.Models.Entities.Order?> GetOrderByIdAsync(int orderId, int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.Items)
                .ThenInclude(i => i.Variant)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);
        }

        public async Task UpdateOrderAsync(MiaraEyeglasses.Models.Entities.Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}
