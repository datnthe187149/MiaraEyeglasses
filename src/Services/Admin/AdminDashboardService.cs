using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    public interface IAdminDashboardService
    {
        Task<(int TotalOrders, int TotalUsers, int TotalProducts, decimal TotalRevenue)> GetDashboardSummaryAsync();
        Task<IEnumerable<DailyRevenueReport>> GetDailyRevenueAsync(int days = 30);
        Task<IEnumerable<BestSellerReport>> GetBestSellersAsync(int top = 10);
        Task<IEnumerable<LowStockReport>> GetLowStockVariantsAsync();
    }

    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IAdminDashboardRepository _dashboardRepository;

        public AdminDashboardService(IAdminDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<(int TotalOrders, int TotalUsers, int TotalProducts, decimal TotalRevenue)> GetDashboardSummaryAsync()
        {
            return await _dashboardRepository.GetStatsSummaryAsync();
        }

        public async Task<IEnumerable<DailyRevenueReport>> GetDailyRevenueAsync(int days = 30)
        {
            return await _dashboardRepository.GetDailyRevenueAsync(days);
        }

        public async Task<IEnumerable<BestSellerReport>> GetBestSellersAsync(int top = 10)
        {
            return await _dashboardRepository.GetBestSellersAsync(top);
        }

        public async Task<IEnumerable<LowStockReport>> GetLowStockVariantsAsync()
        {
            return await _dashboardRepository.GetLowStockVariantsAsync();
        }
    }
}

