using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    [Authorize]
    [CustomerOnly]
    public class UserOrderController : Controller
    {
        private readonly IUserOrderService _userOrderService;

        public UserOrderController(IUserOrderService userOrderService)
        {
            _userOrderService = userOrderService;
        }

        private int GetCurrentUserId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) return userId;
            return 1;
        }

        [HttpGet]
        [Route("User/Orders")]
        [Route("Orders")]
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();
            var orders = await _userOrderService.GetOrdersByUserAsync(userId);
            return View("~/Views/User/Orders.cshtml", orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/CancelOrder")]
        [Route("CancelOrder")]
        public async Task<IActionResult> CancelOrder(int orderId, string? reason)
        {
            int userId = GetCurrentUserId();
            var (success, error) = await _userOrderService.CancelOrderAsync(orderId, userId, reason ?? "Khách hàng tự hủy đơn");
            if (success)
            {
                TempData["SuccessMessage"] = "Đã hủy đơn hàng thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = error ?? "Hủy đơn hàng thất bại.";
            }

            return RedirectToAction("Index");
        }
    }
}

