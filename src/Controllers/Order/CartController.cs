using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.Order;

namespace MiaraEyeglasses.Controllers.Order
{
    // Controller quản lý Giỏ hàng (Cart) của người dùng:
    // - Xem giỏ hàng, thêm sản phẩm kèm gói tròng và độ cận (Rx)
    // - Tăng/giảm số lượng, chọn/bỏ chọn thanh toán, xóa món, áp dụng voucher
    // [CustomerOnly]: Khóa với Admin/Staff để tránh nhầm lẫn dữ liệu mua hàng cá nhân
    [CustomerOnly]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        // Tiêm phụ thuộc ICartService qua Constructor
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // Trích xuất ID người dùng hiện tại từ Session hoặc Cookie Claims
        private int GetCurrentUserId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) return userId;
            return 1;
        }

        // [GET] /Cart: Hiển thị trang giỏ hàng với danh sách món hàng, giá gọng, phụ thu tròng và độ cận
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();
            var cart = await _cartService.GetUserCartAsync(userId);
            return View("~/Views/Cart/Index.cshtml", cart);
        }

        // [POST] Thêm một sản phẩm (gọng + tròng + thông số độ cận) vào giỏ hàng
        [HttpPost]
        public async Task<IActionResult> AddToCart(AddToCartRequest request)
        {
            if (request == null || !ModelState.IsValid)
            {
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                    return BadRequest(ModelState);

                TempData["ErrorMessage"] = "Dữ liệu thêm vào giỏ không hợp lệ.";
                return RedirectToAction("Index", "Shop");
            }

            int userId = GetCurrentUserId();

            // Gọi Service để thêm vào giỏ (nếu đã có biến thể tương tự thì tăng số lượng)
            var success = await _cartService.AddToCartAsync(userId, request);
            if (!success)
            {
                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                    return Json(new { success = false, message = "Sản phẩm không đủ tồn kho hoặc không hợp lệ." });

                TempData["ErrorMessage"] = "Sản phẩm không đủ tồn kho hoặc không hợp lệ.";
                return RedirectToAction("Index", "Cart");
            }

            // Áp dụng mã giảm giá nếu khách hàng đã nhập sẵn
            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                await _cartService.ApplyCouponAsync(userId, request.CouponCode.Trim());
            }

            // Phản hồi JSON nếu gọi qua AJAX
            if (Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                string targetUrl;
                if (request.BuyNow)
                {
                    targetUrl = Url.Action("Checkout", "Order") ?? "/Order/Checkout";
                }
                else
                {
                    targetUrl = Url.Action("Index", "Cart") ?? "/Cart";
                }

                return Json(new { 
                    success = true, 
                    message = "Đã thêm sản phẩm vào giỏ hàng.", 
                    buyNow = request.BuyNow, 
                    redirectUrl = targetUrl 
                });
            }

            // Nếu người dùng chọn nút "Mua ngay", điều hướng thẳng đến trang Checkout
            if (request.BuyNow)
            {
                return RedirectToAction("Checkout", "Order");
            }

            // Mặc định chuyển hướng về trang Giỏ hàng
            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng thành công!";
            return RedirectToAction("Index", "Cart");
        }

        // [POST] Cập nhật số lượng của một mục trong giỏ hàng (Gọi qua AJAX khi bấm +/-)
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            int userId = GetCurrentUserId();
            var success = await _cartService.UpdateQuantityAsync(userId, cartItemId, quantity);
            return Json(new { success });
        }

        // [POST] Chọn / Bỏ chọn một món hàng để tính tiền thanh toán (Checkbox trong giỏ)
        [HttpPost]
        public async Task<IActionResult> ToggleSelect(int cartItemId)
        {
            int userId = GetCurrentUserId();
            var success = await _cartService.ToggleSelectAsync(userId, cartItemId);
            return Json(new { success });
        }

        // [POST] Xóa một mục khỏi giỏ hàng
        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            int userId = GetCurrentUserId();
            var success = await _cartService.RemoveCartItemAsync(userId, cartItemId);
            return Json(new { success });
        }

        // [POST] Áp dụng mã giảm giá trong trang giỏ hàng
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon([FromBody] string couponCode)
        {
            int userId = GetCurrentUserId();
            var discount = await _cartService.ApplyCouponAsync(userId, couponCode);
            if (discount > 0)
                return Json(new { success = true, discount, message = $"Áp dụng mã thành công! Giảm {discount:N0} ₫" });

            return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc chưa đạt giá trị đơn tối thiểu." });
        }
    }
}
