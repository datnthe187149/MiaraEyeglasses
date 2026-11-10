using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.Order;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.Order
{
    // Controller quản lý Đặt hàng & Thanh toán (Checkout & Order Placement)
    [CustomerOnly]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IVnPayService _vnPayService;
        private readonly IProfileService _profileService;

        public OrderController(IOrderService orderService, ICartService cartService, IVnPayService vnPayService, IProfileService profileService)
        {
            _orderService = orderService;
            _cartService = cartService;
            _vnPayService = vnPayService;
            _profileService = profileService;
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

        // [GET] /Checkout hoặc /Order/Checkout: Hiển thị form thanh toán và danh sách món được chọn trong giỏ
        [HttpGet]
        [Route("Checkout")]
        [Route("Order/Checkout")]
        public async Task<IActionResult> Checkout()
        {
            int userId = GetCurrentUserId();
            var cart = await _cartService.GetUserCartAsync(userId);
            if (!cart.Items.Any(i => i.IsSelected))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất 1 sản phẩm trong giỏ hàng để thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            var checkoutModel = new CheckoutViewModel 
            { 
                Cart = cart,
                PaymentMethod = "COD" // Mặc định là COD (Thanh toán khi nhận hàng)
            };

            // TUYỆT ĐỐI KHÔNG tự động điền thông tin của Quản trị viên (Admin) hoặc Nhân viên (Staff)
            // Chỉ prefill thông tin nếu người dùng đăng nhập THẬT SỰ là Khách hàng (Customer)
            bool isActuallyLoggedIn = User.Identity?.IsAuthenticated == true || HttpContext.Session.GetInt32("UserId").HasValue;
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? HttpContext.Session.GetString("Role") ?? "";

            bool isAdminOrStaff = userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) 
                               || userRole.Equals("Staff", StringComparison.OrdinalIgnoreCase)
                               || userRole.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase)
                               || userRole.Equals("Nhân viên", StringComparison.OrdinalIgnoreCase);

            if (isActuallyLoggedIn && !isAdminOrStaff && userId > 1)
            {
                var user = await _profileService.GetUserProfileAsync(userId);
                if (user != null && user.RoleId == 1) // RoleId = 1 là Khách hàng
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

        // [POST] Tiếp nhận đặt hàng (Có validate thông tin người nhận trước khi tạo đơn)
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

            // 3. Validate: Địa chỉ giao hàng bắt buộc
            if (string.IsNullOrWhiteSpace(model.ShippingAddress))
            {
                ModelState.AddModelError("ShippingAddress", "Vui lòng nhập địa chỉ chi tiết để giao kính.");
            }

            // 4. Validate: Giỏ hàng phải có món hàng được chọn
            if (!model.Cart.Items.Any(i => i.IsSelected))
            {
                ModelState.AddModelError(string.Empty, "Giỏ hàng của bạn đang trống hoặc chưa có sản phẩm nào được chọn.");
            }

            if (!ModelState.IsValid)
                return View("~/Views/Order/Checkout.cshtml", model);

            // Bước 5: Gọi tầng Service tạo đơn hàng qua Stored Procedure
            var (success, errorMessage, orderId) = await _orderService.CreateOrderAsync(userId, model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Không thể tạo đơn hàng.");
                return View("~/Views/Order/Checkout.cshtml", model);
            }

            var order = await _orderService.GetOrderDetailsAsync(orderId, userId);
            if (order == null)
                return RedirectToAction("Success", new { id = orderId });

            // Lựa chọn 1: Cổng thanh toán trực tuyến VNPAY (Sandbox)
            if (model.PaymentMethod == "VNPAY")
            {
                var paymentUrl = _vnPayService.CreatePaymentUrl(order, HttpContext);
                return Redirect(paymentUrl);
            }

            // Lựa chọn 2: Chuyển khoản trực tiếp / Quét mã QR (DIRECT)
            if (model.PaymentMethod == "DIRECT")
            {
                await _orderService.AddPaymentRecordAsync(orderId, "DIRECT", order.TotalAmount, 1, order.OrderCode);
                await _orderService.UpdateOrderStatusAsync(orderId, 1, 1); // 1: Đã thanh toán
                TempData["SuccessMessage"] = "Đặt hàng thành công! Đơn hàng chuyển khoản trực tiếp đã được xác nhận thanh toán.";
                return RedirectToAction("Success", new { id = orderId });
            }

            // Lựa chọn 3 (Mặc định): Thanh toán khi nhận hàng (COD / Tiền mặt)
            await _orderService.AddPaymentRecordAsync(orderId, "COD", order.TotalAmount, 0, order.OrderCode);
            TempData["SuccessMessage"] = "Đặt hàng thành công! MIARA đã tiếp nhận đơn hàng. Quý khách thanh toán tiền mặt khi nhận kính.";
            return RedirectToAction("Success", new { id = orderId });
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

        // [GET] Trang xem chi tiết đơn hàng của khách hàng
        public async Task<IActionResult> Detail(int id)
        {
            int userId = GetCurrentUserId();
            var order = await _orderService.GetOrderDetailsAsync(id, userId);
            if (order == null)
                return NotFound();

            return View("~/Views/Order/Detail.cshtml", order);
        }

        // [POST] Hủy đơn hàng khi chưa chuyển sang giao
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
