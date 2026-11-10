using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Staff;

namespace MiaraEyeglasses.Controllers.Staff
{
    [Authorize(Roles = "Admin,Staff")]
    public class StaffReviewController : Controller
    {
        private readonly IStaffReviewService _reviewService;

        public StaffReviewController(IStaffReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private int GetCurrentStaffId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) return userId;
            return 2;
        }

        [HttpGet]
        [Route("Staff/Reviews")]
        [Route("StaffReview")]
        public async Task<IActionResult> Index(byte? status = null)
        {
            if (User.IsInRole("Admin") || HttpContext.Session.GetString("UserRole") == "Admin")
            {
                return RedirectToAction("Index", "AdminReview");
            }

            var reviews = await _reviewService.GetReviewsAsync(status);
            ViewBag.SelectedStatus = status;
            return View("~/Views/Staff/Reviews.cshtml", reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Staff/ModerateReview")]
        [Route("StaffReview/ModerateReview")]
        public async Task<IActionResult> ModerateReview(int reviewId, byte status)
        {
            int staffId = GetCurrentStaffId();
            var (success, error) = await _reviewService.ModerateReviewAsync(reviewId, status, staffId);
            if (success)
                TempData["SuccessMessage"] = "Cập nhật trạng thái đánh giá thành công!";
            else
                TempData["ErrorMessage"] = error ?? "Cập nhật thất bại.";

            return RedirectToAction("Index");
        }
    }
}

