using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Admin;
using MiaraEyeglasses.Services.Order;

namespace MiaraEyeglasses.Controllers.Admin
{
    // Controller Bảng điều khiển tổng quan dành riêng cho Quản trị viên (Admin Dashboard)
    // Tổng hợp số liệu thống kê doanh thu, đơn hàng, người dùng, sản phẩm bán chạy và cảnh báo tồn kho
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly IAdminDashboardService _dashboardService;
        private readonly IOrderService _orderService;

        public AdminDashboardController(IAdminDashboardService dashboardService, IOrderService orderService)
        {
            _dashboardService = dashboardService;
            _orderService = orderService;
        }

        // [GET] /Admin/Dashboard: Hiển thị các chỉ số kinh doanh và danh sách đơn hàng mới nhất
        [HttpGet]
        [Route("Admin")]
        [Route("Admin/Index")]
        [Route("Admin/Dashboard")]
        [Route("AdminDashboard")]
        public async Task<IActionResult> Index()
        {
            // 1. Lấy dữ liệu tổng quan: tổng đơn, người dùng, số mẫu kính và tổng doanh thu
            var summary = await _dashboardService.GetDashboardSummaryAsync();
            ViewBag.TotalOrders = summary.TotalOrders;
            ViewBag.TotalUsers = summary.TotalUsers;
            ViewBag.TotalProducts = summary.TotalProducts;
            ViewBag.TotalRevenue = summary.TotalRevenue;

            // 2. Lấy dữ liệu biểu đồ doanh thu 30 ngày gần nhất
            ViewBag.DailyRevenues = await _dashboardService.GetDailyRevenueAsync(30);

            // 3. Lấy top 5 sản phẩm bán chạy nhất
            ViewBag.BestSellers = await _dashboardService.GetBestSellersAsync(5);

            // 4. Lấy danh sách các biến thể sắp hết hàng để cảnh báo nhập kho
            ViewBag.LowStocks = await _dashboardService.GetLowStockVariantsAsync();

            // 5. Lấy 10 đơn hàng mới nhất hiển thị trên bảng điều khiển
            var recentOrders = await _orderService.GetAllOrdersForStaffAsync();
            return View("~/Views/Admin/Index.cshtml", recentOrders.Take(10));
        }
    }
}
