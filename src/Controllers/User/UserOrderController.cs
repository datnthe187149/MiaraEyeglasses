using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // Controller xem danh sách đơn hàng của người dùng qua Session (chuẩn CatBaBooking)
    public class UserOrderController : Controller
    {
        private readonly IUserOrderService _userOrderService;

        public UserOrderController(IUserOrderService userOrderService)
        {
            _userOrderService = userOrderService;
        }

        private int? GetCurrentUserId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (int.TryParse(userIdStr, out var id))
                return id;

            return null;
        }

        // [GET] Xem danh sách đơn hàng đã đặt
        [HttpGet]
        [Route("UserOrder")]
        [Route("User/Orders")]
        [Route("Orders")]
        public async Task<IActionResult> Index()
        {
            int? userId = GetCurrentUserId();
            if (!userId.HasValue) return Redirect("/login");

            var orders = await _userOrderService.GetOrdersByUserAsync(userId.Value);
            return View("~/Views/User/Orders.cshtml", orders);
        }

        // [POST] Khách hàng tự hủy đơn hàng
        [HttpPost]
        [Route("User/CancelOrder")]
        [Route("CancelOrder")]
        public async Task<IActionResult> CancelOrder(int orderId, string? reason)
        {
            int? userId = GetCurrentUserId();
            if (!userId.HasValue) return Redirect("/login");

            var (success, error) = await _userOrderService.CancelOrderAsync(
                orderId, 
                userId.Value, 
                reason ?? "Khách hàng tự hủy đơn"
            );

            if (success)
            {
                TempData["SuccessMessage"] = "Đã hủy đơn hàng thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = error ?? "Không thể hủy đơn hàng này.";
            }

            return RedirectToAction("Index");
        }
    }
}
