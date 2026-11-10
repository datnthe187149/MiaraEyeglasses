using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // Controller quản lý Hồ sơ cá nhân của khách hàng (Thông tin, Sổ địa chỉ, Mật khẩu)
    [Authorize]
    [CustomerOnly]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
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

        // [GET] Xem trang hồ sơ cá nhân và các tab thông tin
        [HttpGet]
        [Route("User/Account")]
        [Route("User/Profile")]
        [Route("Profile")]
        public async Task<IActionResult> Index(string? tab = null)
        {
            int userId = GetCurrentUserId();

            // Lấy thông tin user kèm danh bạ địa chỉ và đơn kính cận
            var user = await _profileService.GetUserProfileAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.ActiveTab = tab ?? "profile";
            return View("~/Views/User/Profile.cshtml", user);
        }

        // [POST] Cập nhật thông tin lý lịch cá nhân (Họ tên, SĐT, Giới tính, Ngày sinh)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/UpdateProfile")]
        [Route("Profile/Update")]
        public async Task<IActionResult> Update(UpdateProfileViewModel model)
        {
            int userId = GetCurrentUserId();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu cập nhật hồ sơ không hợp lệ.";
                return RedirectToAction("Index", new { tab = "profile" });
            }

            // Lưu thông tin mới vào database
            var (success, error) = await _profileService.UpdateProfileAsync(userId, model);
            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "Cập nhật thông tin thất bại.";
                return RedirectToAction("Index", new { tab = "profile" });
            }

            // Đồng bộ lại tên mới vào Session
            HttpContext.Session.SetString("UserName", model.FullName);

            TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
            return RedirectToAction("Index", new { tab = "profile" });
        }
    }
}
