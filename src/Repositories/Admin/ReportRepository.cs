using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;

namespace MiaraEyeglasses.Repositories.Admin
{
    // Interface kho dữ liệu báo cáo kinh doanh
    public interface IReportRepository
    {
        Task<IEnumerable<DailyRevenueReport>> GetDailyRevenueAsync(int days = 30);
        Task<IEnumerable<BestSellerReport>> GetBestSellersAsync(int top = 10);
        Task<IEnumerable<LowStockReport>> GetLowStockVariantsAsync();
    }

    // Lớp thực thi trích xuất báo cáo doanh thu và top bán chạy
    public class ReportRepository : IReportRepository
    {
        private readonly EyeglassesDbContext _context;

        // Khởi tạo đối tượng ReportRepository kèm nạp các phụ thuộc cần thiết
        public ReportRepository(EyeglassesDbContext context)
        {
            _context = context;
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

