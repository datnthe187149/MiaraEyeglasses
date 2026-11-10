using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.Order;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.Order
{
    // =========================================================================================
    // CONTROLLER: Quản lý Đặt hàng & Thanh toán trực tuyến (Order & Checkout Controller)
    // Phân hệ: Bán hàng trực tuyến & Cổng thanh toán điện tử
    // Chức năng:
    //   - Hiển thị trang thanh toán đơn hàng (Checkout) kèm danh sách sản phẩm được chọn từ giỏ hàng.
    //   - Hỗ trợ bộ điều khiển số lượng sản phẩm ngay tại màn hình thanh toán, tự động xóa khi về 0.
    //   - Tự động chuyển hướng khách sang cổng thanh toán trực tuyến VNPAY (Sandbox).
    //   - Hiển thị màn hình xác nhận đặt hàng thành công (Success).
    // =========================================================================================
    [CustomerOnly]
    public class OrderController : Controller
    {
        // Khai báo các dịch vụ nghiệp vụ cần thiết (Dependency Injection)
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IVnPayService _vnPayService;
        private readonly IProfileService _profileService;

        /// <summary>
        /// Hàm khởi tạo tiêm phụ thuộc các Service nghiệp vụ Đơn hàng, Giỏ hàng, VNPAY và Hồ sơ khách
        /// </summary>
        public OrderController(
            IOrderService orderService, 
            ICartService cartService, 
            IVnPayService vnPayService, 
            IProfileService profileService)
        {
            _orderService = orderService;
            _cartService = cartService;
            _vnPayService = vnPayService;
            _profileService = profileService;
        }

        /// <summary>
        /// Hàm bổ trợ lấy mã định danh người dùng (UserId) hiện tại đang đăng nhập
        /// </summary>
        /// <returns>Mã ID của người dùng trong hệ thống</returns>
        private int GetCurrentUserId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) 
                return userId;

            return 1;
        }

        /// <summary>
        /// [GET] Hiển thị màn hình Thanh toán đơn hàng (Checkout)
        /// Tuyệt đối KHÔNG tự động điền thông tin của Admin/Staff; chỉ prefill nếu là Khách hàng thật sự.
        /// Route hỗ trợ: /Checkout, /Order/Checkout
        /// </summary>
        [HttpGet]
        [Route("Checkout")]
        [Route("Order/Checkout")]
        public async Task<IActionResult> Checkout()
        {
            // Bước 1: Lấy thông tin giỏ hàng của người dùng hiện tại
            int userId = GetCurrentUserId();
            var cart = await _cartService.GetUserCartAsync(userId);

            // Kiểm tra: Bắt buộc giỏ hàng phải có ít nhất 1 sản phẩm được tick chọn thanh toán
            if (!cart.Items.Any(i => i.IsSelected))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất 1 sản phẩm trong giỏ hàng để thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            // Khởi tạo ViewModel thanh toán với phương thức mặc định là VNPAY Sandbox
            var checkoutModel = new CheckoutViewModel 
            { 
                Cart = cart,
                PaymentMethod = "VNPAY" // Tự động thanh toán qua VNPAY Sandbox theo yêu cầu
            };

            // Bước 2: Bảo mật dữ liệu - Tuyệt đối không tự động điền thông tin của Quản trị viên hoặc Nhân viên
            bool isActuallyLoggedIn = User.Identity?.IsAuthenticated == true || HttpContext.Session.GetInt32("UserId").HasValue;
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? HttpContext.Session.GetString("Role") ?? "";

            bool isAdminOrStaff = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) 
                               || userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase)
                               || userRole.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase)
                               || userRole.Equals("Nhân viên", StringComparison.OrdinalIgnoreCase);

            // Chỉ tự động điền nếu đúng là tài khoản Khách hàng mua sắm (RoleId = 1)
            if (isActuallyLoggedIn && !isAdminOrStaff && userId > 1)
            {
                var user = await _profileService.GetUserProfileAsync(userId);
                if (user != null && user.RoleId == 1)
                {
                    checkoutModel.RecipientName = user.FullName;
                    checkoutModel.Phone = user.Phone;

                    var defaultAddr = user.Addresses.FirstOrDefault(a => a.IsDefault) ?? user.Addresses.FirstOrDefault();
                    if (defaultAddr != null)
                    {
                        checkoutModel.RecipientName = defaultAddr.RecipientName;
                        checkoutModel.Phone = defaultAddr.Phone;
                        checkoutModel.ShippingAddress = $"{defaultAddr.StreetAddress}, {defaultAddr.DistrictWard}, {defaultAddr.Province}";
                    }
                }
            }

            // Bước 3: Trả về giao diện thanh toán
            return View("~/Views/Order/Checkout.cshtml", checkoutModel);
        }

        /// <summary>
        /// [POST] Tiếp nhận thông tin nhận hàng và khởi tạo đơn hàng qua Stored Procedure
        /// Sau khi tạo đơn thành công, tự động chuyển hướng khách sang cổng VNPAY Sandbox
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            int userId = GetCurrentUserId();
            model.Cart = await _cartService.GetUserCartAsync(userId);

            // 1. Validate: Họ tên người nhận bắt buộc
            if (string.IsNullOrWhiteSpace(model.RecipientName))
            {
                ModelState.AddModelError("RecipientName", "Vui lòng nhập họ và tên người nhận hàng.");
            }

            // 2. Validate: Số điện thoại nhận hàng bắt buộc
            if (string.IsNullOrWhiteSpace(model.Phone))
            {
                ModelState.AddModelError("Phone", "Vui lòng nhập số điện thoại liên hệ giao hàng.");
            }

            // 3. Validate: Địa chỉ giao hàng chi tiết bắt buộc
            if (string.IsNullOrWhiteSpace(model.ShippingAddress))
            {
                ModelState.AddModelError("ShippingAddress", "Vui lòng nhập địa chỉ chi tiết để giao kính.");
            }

            // 4. Validate: Giỏ hàng phải có ít nhất 1 sản phẩm được chọn
            if (!model.Cart.Items.Any(i => i.IsSelected))
            {
                ModelState.AddModelError(string.Empty, "Giỏ hàng của bạn đang trống hoặc chưa có sản phẩm nào được chọn.");
            }

            // Nếu dữ liệu không hợp lệ, trả về form báo lỗi cho người dùng
            if (!ModelState.IsValid)
                return View("~/Views/Order/Checkout.cshtml", model);

            // Bước 5: Gọi Service tạo đơn hàng qua Stored Procedure (Transaction trừ kho và xóa giỏ an toàn)
            var (success, errorMessage, orderId) = await _orderService.CreateOrderAsync(userId, model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Không thể tạo đơn hàng.");
                return View("~/Views/Order/Checkout.cshtml", model);
            }

            // Lấy chi tiết đơn hàng vừa tạo để chuẩn bị thông tin thanh toán
            var order = await _orderService.GetOrderDetailsAsync(orderId, userId);
            if (order == null)
                return RedirectToAction("Success", new { id = orderId });

            // Bước 6: Tự động chuyển hướng sang Cổng thanh toán trực tuyến VNPAY (Sandbox)
            var paymentUrl = _vnPayService.CreatePaymentUrl(order, HttpContext);
            return Redirect(paymentUrl);
        }

        /// <summary>
        /// [GET] Trang thông báo đặt hàng thành công sau khi hoàn tất thanh toán
        /// </summary>
        /// <param name="id">Mã định danh đơn hàng</param>
        public async Task<IActionResult> Success(int id)
        {
            int userId = GetCurrentUserId();
            var order = await _orderService.GetOrderDetailsAsync(id, userId);
            if (order == null)
                return NotFound();

            return View("~/Views/Order/Success.cshtml", order);
        }

        /// <summary>
        /// [GET] Trang xem chi tiết đơn hàng của khách hàng
        /// </summary>
        /// <param name="id">Mã định danh đơn hàng</param>
        public async Task<IActionResult> Detail(int id)
        {
            int userId = GetCurrentUserId();
            var order = await _orderService.GetOrderDetailsAsync(id, userId);
            if (order == null)
                return NotFound();

            return View("~/Views/Order/Detail.cshtml", order);
        }

        /// <summary>
        /// [POST] API hủy đơn hàng từ phía khách hàng (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Cancel(int orderId, string? reason)
        {
            if (orderId <= 0)
                return Json(new { success = false, message = "Mã đơn hàng không hợp lệ." });

            int userId = GetCurrentUserId();
            var (success, error) = await _orderService.CancelOrderAsync(orderId, userId, reason ?? "Khách hàng tự hủy đơn");
            if (!success)
                return Json(new { success = false, message = error });

            return Json(new { success = true, message = "Đã hủy đơn hàng thành công." });
        }
    }
}
