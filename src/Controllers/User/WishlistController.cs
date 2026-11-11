using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // Controller quản lý Danh sách yêu thích (Wishlist) của khách hàng
    [Authorize]
    [CustomerOnly]
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        // Lấy ID người dùng hiện tại
        private int GetCurrentUserId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) return userId;
            return 1;
        }

        // [GET] /Wishlist: Hiển thị danh sách các mẫu kính đã lưu yêu thích
        [HttpGet]
        [Route("User/Wishlist")]
        [Route("Wishlist")]
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();
            var wishlist = await _wishlistService.GetUserWishlistAsync(userId);
            return View("~/Views/User/Wishlist.cshtml", wishlist);
        }

        // [POST] Thêm hoặc Xóa một mẫu kính khỏi danh sách yêu thích (Toggle)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/ToggleWishlist")]
        [Route("Wishlist/Toggle")]
        public async Task<IActionResult> Toggle(int productId, string? returnUrl = null)
        {
            int userId = GetCurrentUserId();

            // Đảo trạng thái yêu thích của sản phẩm (nếu có rồi thì xóa, chưa có thì thêm)
            var isWishlisted = await _wishlistService.ToggleWishlistAsync(userId, productId);
            if (isWishlisted)
                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào danh sách yêu thích!";
            else
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi danh sách yêu thích.";

            // Quay lại trang trước đó nếu hợp lệ
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }
    }
}
