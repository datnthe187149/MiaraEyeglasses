using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // Controller quản lý Danh sách yêu thích (Wishlist) qua Session (chuẩn CatBaBooking)
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
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

        // [GET] /Wishlist: Hiển thị danh sách các mẫu kính đã lưu yêu thích
        [HttpGet]
        [Route("User/Wishlist")]
        [Route("Wishlist")]
        public async Task<IActionResult> Index()
        {
            int? userId = GetCurrentUserId();
            if (!userId.HasValue) return Redirect("/login");

            var wishlist = await _wishlistService.GetUserWishlistAsync(userId.Value);
            return View("~/Views/User/Wishlist.cshtml", wishlist);
        }

        // [POST] Thêm hoặc Xóa một mẫu kính khỏi danh sách yêu thích
        [HttpPost]
        [Route("User/ToggleWishlist")]
        [Route("Wishlist/Toggle")]
        public async Task<IActionResult> Toggle(int productId, string? returnUrl = null)
        {
            int? userId = GetCurrentUserId();
            if (!userId.HasValue) return Redirect("/login");

            var isWishlisted = await _wishlistService.ToggleWishlistAsync(userId.Value, productId);
            if (isWishlisted)
                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào danh sách yêu thích!";
            else
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi danh sách yêu thích.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }
    }
}
