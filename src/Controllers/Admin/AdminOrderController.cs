using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Admin;

namespace MiaraEyeglasses.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminOrderController : Controller
    {
        private readonly IAdminOrderService _orderService;

        public AdminOrderController(IAdminOrderService orderService)
        {
            _orderService = orderService;
        }

        private int GetCurrentAdminId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) return userId;
            return 1;
        }

        [HttpGet]
        [Route("Admin/Orders")]
        [Route("AdminOrder")]
        public async Task<IActionResult> Index(byte? status = null)
        {
            var orders = await _orderService.GetAllOrdersAsync(status);
            ViewBag.SelectedStatus = status;
            return View("~/Views/Admin/Orders.cshtml", orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Refund")]
        [Route("AdminOrder/Refund")]
        public async Task<IActionResult> Refund(int orderId, string reason, decimal? refundAmount = null)
        {
            int adminId = GetCurrentAdminId();
            var (success, error) = await _orderService.RefundOrderAsync(orderId, adminId, reason ?? "Admin hoàn tiền đơn hàng", refundAmount);
            if (success)
                TempData["SuccessMessage"] = $"Đã hoàn tiền đơn hàng #{orderId} thành công!";
            else
                TempData["ErrorMessage"] = error ?? "Hoàn tiền đơn hàng thất bại.";

            return RedirectToAction("Index");
        }
    }
}

