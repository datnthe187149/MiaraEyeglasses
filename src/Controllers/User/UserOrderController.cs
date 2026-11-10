using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // Controller quản lý Đơn hàng cá nhân của Khách hàng
    // Cho phép khách xem lịch sử các đơn kính đã đặt và tự hủy đơn khi chưa giao
    [Authorize]
    [CustomerOnly]
    public class UserOrderController : Controller
    {
        private readonly IUserOrderService _userOrderService;

        public UserOrderController(IUserOrderService userOrderService)
        {
            _userOrderService = userOrderService;
        }

        // Lấy ID người dùng hiện tại đang đăng nhập (từ Session hoặc Cookie Claims)
        private int GetCurrentUserId()
        {
            // Kiểm tra UserId lưu trong Session
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            // Kiểm tra UserId từ Cookie Authentication
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) 
                return userId;

            return 1;
        }

        // [GET] Xem danh sách đơn hàng đã đặt của khách hàng
        [HttpGet]
        [Route("UserOrder")]
        [Route("User/Orders")]
        [Route("Orders")]
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();

            // Lấy danh sách đơn hàng từ database qua Service
            var orders = await _userOrderService.GetOrdersByUserAsync(userId);

            return View("~/Views/User/Orders.cshtml", orders);
        }

        // [POST] Khách hàng tự hủy đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/CancelOrder")]
        [Route("CancelOrder")]
        public async Task<IActionResult> CancelOrder(int orderId, string? reason)
        {
            int userId = GetCurrentUserId();

            // Gọi Service hủy đơn và tự động hoàn lại số lượng tồn kho
            var (success, error) = await _userOrderService.CancelOrderAsync(
                orderId, 
                userId, 
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
