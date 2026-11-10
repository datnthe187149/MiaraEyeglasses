using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    [Authorize]
    [CustomerOnly]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
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

        [HttpGet]
        [Route("User/Account")]
        [Route("User/Profile")]
        [Route("Profile")]
        public async Task<IActionResult> Index(string? tab = null)
        {
            int userId = GetCurrentUserId();
            var user = await _profileService.GetUserProfileAsync(userId);
            if (user == null)
                return RedirectToAction("Index", "Login");

            ViewBag.ActiveTab = tab ?? "profile";
            return View("~/Views/User/Account.cshtml", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/UpdateProfile")]
        [Route("Profile/Update")]
        public async Task<IActionResult> Update(UpdateProfileViewModel model)
        {
            int userId = GetCurrentUserId();
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin không hợp lệ, vui lòng kiểm tra lại.";
                return RedirectToAction("Index", new { tab = "profile" });
            }

            var (success, error) = await _profileService.UpdateProfileAsync(userId, model);
            if (success)
            {
                HttpContext.Session.SetString("UserName", model.FullName);
                TempData["SuccessMessage"] = "Cập nhật hồ sơ cá nhân thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = error ?? "Cập nhật thất bại.";
            }

            return RedirectToAction("Index", new { tab = "profile" });
        }
    }
}
