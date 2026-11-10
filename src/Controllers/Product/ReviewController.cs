using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.Product;

namespace MiaraEyeglasses.Controllers.Product
{
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private int GetCurrentUserId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) return userId;
            return 0;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SubmitReviewViewModel model)
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để gửi đánh giá sản phẩm.";
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Detail", "Shop", new { id = model.ProductId }) });
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin đánh giá chưa hợp lệ.";
                return RedirectToAction("Detail", "Shop", new { id = model.ProductId });
            }

            var (success, error) = await _reviewService.SubmitReviewAsync(userId, model.ProductId, model.Rating, model.Comment);
            if (success)
                TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi đánh giá cho sản phẩm!";
            else
                TempData["ErrorMessage"] = error ?? "Gửi đánh giá thất bại.";

            return RedirectToAction("Detail", "Shop", new { id = model.ProductId });
        }
    }
}

