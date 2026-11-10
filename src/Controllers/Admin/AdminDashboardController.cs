using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Admin;
using MiaraEyeglasses.Services.Order;

namespace MiaraEyeglasses.Controllers.Admin
{
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

        [HttpGet]
        [Route("Admin")]
        [Route("Admin/Index")]
        [Route("Admin/Dashboard")]
        [Route("AdminDashboard")]
        public async Task<IActionResult> Index()
        {
            var summary = await _dashboardService.GetDashboardSummaryAsync();
            ViewBag.TotalOrders = summary.TotalOrders;
            ViewBag.TotalUsers = summary.TotalUsers;
            ViewBag.TotalProducts = summary.TotalProducts;
            ViewBag.TotalRevenue = summary.TotalRevenue;

            ViewBag.DailyRevenues = await _dashboardService.GetDailyRevenueAsync(30);
            ViewBag.BestSellers = await _dashboardService.GetBestSellersAsync(5);
            ViewBag.LowStocks = await _dashboardService.GetLowStockVariantsAsync();

            var recentOrders = await _orderService.GetAllOrdersForStaffAsync();
            return View("~/Views/Admin/Index.cshtml", recentOrders.Take(10));
        }
    }
}
