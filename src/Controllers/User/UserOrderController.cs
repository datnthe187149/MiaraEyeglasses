using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // =========================================================================================
    // CONTROLLER: Quản lý Đơn hàng cá nhân của Khách hàng (User Order Controller)
    // Phân hệ: Khách hàng & Quản lý lịch sử giao dịch mua sắm
    // Chức năng:
    //   - Xem danh sách đơn hàng đã đặt kèm trạng thái, tổng tiền và chi tiết kính thuốc.
    //   - Cho phép khách hàng tự hủy đơn hàng khi đơn chưa chuyển sang trạng thái xử lý/giao hàng.
    // =========================================================================================
    [Authorize]
    [CustomerOnly]
    public class UserOrderController : Controller
    {
        // Khai báo tầng Service xử lý nghiệp vụ đơn hàng của người dùng
        private readonly IUserOrderService _userOrderService;

        /// <summary>
        /// Hàm khởi tạo tiêm phụ thuộc IUserOrderService (Dependency Injection)
        /// </summary>
        /// <param name="userOrderService">Dịch vụ quản lý đơn hàng người dùng</param>
        public UserOrderController(IUserOrderService userOrderService)
        {
            _userOrderService = userOrderService;
        }

        /// <summary>
        /// Hàm bổ trợ lấy mã định danh người dùng (UserId) hiện tại đang đăng nhập
        /// Ưu tiên lấy từ Session bộ nhớ, nếu không có sẽ trích xuất từ ClaimsPrincipal
        /// </summary>
        /// <returns>Mã ID của người dùng trong cơ sở dữ liệu</returns>
        private int GetCurrentUserId()
        {
            // 1. Kiểm tra UserId đã lưu trong Session khi đăng nhập thành công
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            // 2. Trích xuất UserId từ Claims (NameIdentifier) do Cookie Authentication quản lý
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) 
                return userId;

            // 3. Giá trị fallback an toàn (UserId = 1)
            return 1;
        }

        /// <summary>
        /// [GET] Hiển thị danh sách lịch sử đơn hàng của khách hàng đang đăng nhập
        /// Route hỗ trợ: /UserOrder, /User/Orders, /Orders
        /// </summary>
        /// <returns>Razor View kèm danh sách các đơn hàng</returns>
        [HttpGet]
        [Route("UserOrder")]
        [Route("User/Orders")]
        [Route("Orders")]
        public async Task<IActionResult> Index()
        {
            // Bước 1: Lấy mã UserId của khách hàng hiện tại
            int userId = GetCurrentUserId();

            // Bước 2: Gọi Service truy vấn toàn bộ đơn hàng của khách từ cơ sở dữ liệu
            var orders = await _userOrderService.GetOrdersByUserAsync(userId);

            // Bước 3: Trả về giao diện danh sách đơn hàng
            return View("~/Views/User/Orders.cshtml", orders);
        }

        /// <summary>
        /// [POST] Xử lý yêu cầu tự hủy đơn hàng từ phía khách hàng
        /// </summary>
        /// <param name="orderId">Mã định danh đơn hàng cần hủy</param>
        /// <param name="reason">Lý do hủy đơn do khách cung cấp</param>
        /// <returns>Chuyển hướng về trang danh sách đơn hàng kèm thông báo kết quả</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/CancelOrder")]
        [Route("CancelOrder")]
        public async Task<IActionResult> CancelOrder(int orderId, string? reason)
        {
            // Bước 1: Xác định UserId của người gửi yêu cầu hủy đơn
            int userId = GetCurrentUserId();

            // Bước 2: Gọi Service hủy đơn qua Stored Procedure (hoàn lại số lượng tồn kho và áp dụng transaction an toàn)
            var (success, error) = await _userOrderService.CancelOrderAsync(
                orderId, 
                userId, 
                reason ?? "Khách hàng tự hủy đơn"
            );

            // Bước 3: Phản hồi thông báo kết quả lên màn hình qua TempData
            if (success)
            {
                TempData["SuccessMessage"] = "Đã hủy đơn hàng thành công và hoàn trả số lượng tồn kho.";
            }
            else
            {
                TempData["ErrorMessage"] = error ?? "Không thể hủy đơn hàng (Đơn đã thanh toán hoặc đã được xử lý).";
            }

            // Bước 4: Tải lại trang danh sách đơn hàng
            return RedirectToAction("Index");
        }
    }
}
