using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    [Authorize]
    [CustomerOnly]
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
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
        [Route("User/Wishlist")]
        [Route("Wishlist")]
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();
            var wishlist = await _wishlistService.GetUserWishlistAsync(userId);
            return View("~/Views/User/Wishlist.cshtml", wishlist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/ToggleWishlist")]
        [Route("Wishlist/Toggle")]
        public async Task<IActionResult> Toggle(int productId, string? returnUrl = null)
        {
            int userId = GetCurrentUserId();
            var isWishlisted = await _wishlistService.ToggleWishlistAsync(userId, productId);
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
