using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Staff;

namespace MiaraEyeglasses.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminReviewController : Controller
    {
        private readonly IStaffReviewService _reviewService;

        public AdminReviewController(IStaffReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private int GetCurrentAdminId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) return userId;
            return 1;
        }

        [HttpGet]
        [Route("Admin/Reviews")]
        [Route("AdminReview")]
        public async Task<IActionResult> Index(byte? status = null)
        {
            var reviews = await _reviewService.GetReviewsAsync(status);
            ViewBag.SelectedStatus = status;
            return View("~/Views/Admin/Reviews.cshtml", reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/ModerateReview")]
        [Route("AdminReview/ModerateReview")]
        public async Task<IActionResult> ModerateReview(int reviewId, byte status)
        {
            int adminId = GetCurrentAdminId();
            var (success, error) = await _reviewService.ModerateReviewAsync(reviewId, status, adminId);
            if (success)
                TempData["SuccessMessage"] = "Cập nhật trạng thái đánh giá thành công!";
            else
                TempData["ErrorMessage"] = error ?? "Cập nhật thất bại.";

            return RedirectToAction("Index");
        }
    }
}

