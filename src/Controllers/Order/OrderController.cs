using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.Order;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.Order
{
    // Controller quản lý Đặt hàng & Thanh toán (Checkout & Order Placement)
    // Tự động chuyển tiếp sang cổng thanh toán trực tuyến VNPAY Sandbox
    [CustomerOnly]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IVnPayService _vnPayService;
        private readonly IProfileService _profileService;

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

        // Lấy ID người dùng hiện tại đang đăng nhập
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

        // [GET] /Checkout: Màn hình nhập thông tin nhận hàng và tóm tắt đơn hàng
        [HttpGet]
        [Route("Checkout")]
        [Route("Order/Checkout")]
        public async Task<IActionResult> Checkout()
        {
            int userId = GetCurrentUserId();
            var cart = await _cartService.GetUserCartAsync(userId);

            // Bắt buộc phải có ít nhất 1 sản phẩm được chọn để thanh toán
            if (!cart.Items.Any(i => i.IsSelected))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất 1 sản phẩm trong giỏ hàng để thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            var checkoutModel = new CheckoutViewModel 
            { 
                Cart = cart,
                PaymentMethod = "VNPAY" // Tự động chọn VNPAY Sandbox
            };

            // Tuyệt đối không tự động điền thông tin của Admin/Staff
            bool isActuallyLoggedIn = User.Identity?.IsAuthenticated == true || HttpContext.Session.GetInt32("UserId").HasValue;
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? HttpContext.Session.GetString("Role") ?? "";

            bool isAdminOrStaff = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) 
                               || userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase)
                               || userRole.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase)
                               || userRole.Equals("Nhân viên", StringComparison.OrdinalIgnoreCase);

            // Chỉ điền sẵn nếu là khách hàng thật sự
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

            return View("~/Views/Order/Checkout.cshtml", checkoutModel);
        }

        // [POST] Tạo đơn hàng mới và chuyển hướng sang cổng thanh toán VNPAY Sandbox
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            int userId = GetCurrentUserId();
            model.Cart = await _cartService.GetUserCartAsync(userId);

            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrWhiteSpace(model.RecipientName))
            {
                ModelState.AddModelError("RecipientName", "Vui lòng nhập họ và tên người nhận hàng.");
            }

            if (string.IsNullOrWhiteSpace(model.Phone))
            {
                ModelState.AddModelError("Phone", "Vui lòng nhập số điện thoại liên hệ giao hàng.");
            }

            if (string.IsNullOrWhiteSpace(model.ShippingAddress))
            {
                ModelState.AddModelError("ShippingAddress", "Vui lòng nhập địa chỉ chi tiết để giao kính.");
            }

            if (!model.Cart.Items.Any(i => i.IsSelected))
            {
                ModelState.AddModelError(string.Empty, "Giỏ hàng của bạn đang trống hoặc chưa có sản phẩm nào được chọn.");
            }

            if (!ModelState.IsValid)
                return View("~/Views/Order/Checkout.cshtml", model);

            // Tạo đơn hàng qua Stored Procedure (Transaction an toàn trừ kho và xóa giỏ)
            var (success, errorMessage, orderId) = await _orderService.CreateOrderAsync(userId, model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Không thể tạo đơn hàng.");
                return View("~/Views/Order/Checkout.cshtml", model);
            }

            var order = await _orderService.GetOrderDetailsAsync(orderId, userId);
            if (order == null)
                return RedirectToAction("Success", new { id = orderId });

            // Tự động chuyển hướng sang VNPAY Sandbox để thanh toán
            var paymentUrl = _vnPayService.CreatePaymentUrl(order, HttpContext);
            return Redirect(paymentUrl);
        }

        // [GET] Trang thông báo đặt hàng thành công
        public async Task<IActionResult> Success(int id)
        {
            int userId = GetCurrentUserId();
            var order = await _orderService.GetOrderDetailsAsync(id, userId);
            if (order == null)
                return NotFound();

            return View("~/Views/Order/Success.cshtml", order);
        }

        // [GET] Xem chi tiết đơn hàng của khách
        public async Task<IActionResult> Detail(int id)
        {
            int userId = GetCurrentUserId();
            var order = await _orderService.GetOrderDetailsAsync(id, userId);
            if (order == null)
                return NotFound();

            return View("~/Views/Order/Detail.cshtml", order);
        }

        // [POST] API hủy đơn hàng từ phía khách hàng (AJAX)
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
