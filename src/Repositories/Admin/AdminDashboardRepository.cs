using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;

namespace MiaraEyeglasses.Repositories.Admin
{
    // Interface kho dữ liệu thống kê số liệu tổng quan hệ thống
    public interface IAdminDashboardRepository
    {
        // Lấy dữ liệu: StatsSummary
        Task<(int TotalOrders, int TotalUsers, int TotalProducts, decimal TotalRevenue)> GetStatsSummaryAsync();
        Task<IEnumerable<DailyRevenueReport>> GetDailyRevenueAsync(int days = 30);
        Task<IEnumerable<BestSellerReport>> GetBestSellersAsync(int top = 10);
        Task<IEnumerable<LowStockReport>> GetLowStockVariantsAsync();
    }

    // Lớp thực thi tổng hợp các chỉ số doanh thu và đơn hàng cho Dashboard
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly EyeglassesDbContext _context;

        // Khởi tạo đối tượng AdminDashboardRepository kèm nạp các phụ thuộc cần thiết
        public AdminDashboardRepository(EyeglassesDbContext context)
        {
            _context = context;
        }

        // Lấy dữ liệu: StatsSummary
        public async Task<(int TotalOrders, int TotalUsers, int TotalProducts, decimal TotalRevenue)> GetStatsSummaryAsync()
        {
            int totalOrders = await _context.Orders.CountAsync();
            int totalUsers = await _context.Users.CountAsync();
            int totalProducts = await _context.Products.CountAsync();
            decimal totalRevenue = await _context.Orders
                .Where(o => o.Status >= 1 && o.Status <= 4 && o.PaidAt != null)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            return (totalOrders, totalUsers, totalProducts, totalRevenue);
        }

        public async Task<IEnumerable<DailyRevenueReport>> GetDailyRevenueAsync(int days = 30)
        {
            return await _context.DailyRevenues
                .OrderByDescending(r => r.RevenueDate)
                .Take(days)
                .ToListAsync();
        }

        public async Task<IEnumerable<BestSellerReport>> GetBestSellersAsync(int top = 10)
        {
            return await _context.BestSellers
                .OrderByDescending(b => b.SoldQty)
                .Take(top)
                .ToListAsync();
        }

        public async Task<IEnumerable<LowStockReport>> GetLowStockVariantsAsync()
        {
            return await _context.LowStocks
                .OrderBy(l => l.StockQty)
                .ToListAsync();
        }
    }
}

