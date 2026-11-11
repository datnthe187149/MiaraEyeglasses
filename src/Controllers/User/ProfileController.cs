using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // Controller quản lý Hồ sơ tài khoản khách hàng bằng Session (chuẩn CatBaBooking)
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        // Lấy ID người dùng từ Session
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

        // [GET] Xem trang hồ sơ tài khoản và các tab thông tin
        [HttpGet]
        [Route("User/Account")]
        [Route("User/Profile")]
        [Route("Profile")]
        public async Task<IActionResult> Index(string? tab = null)
        {
            int? userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Redirect("/login");
            }

            // Lấy thông tin user kèm danh bạ địa chỉ và đơn kính cận
            var user = await _profileService.GetUserProfileAsync(userId.Value);
            if (user == null)
            {
                HttpContext.Session.Clear();
                return Redirect("/login");
            }

            ViewBag.ActiveTab = tab ?? "profile";
            return View("~/Views/User/Account.cshtml", user);
        }

        // [POST] Cập nhật thông tin lý lịch cá nhân (Họ tên, SĐT, Giới tính, Ngày sinh)
        [HttpPost]
        [Route("User/UpdateProfile")]
        [Route("Profile/Update")]
        public async Task<IActionResult> Update(UpdateProfileViewModel model)
        {
            int? userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Redirect("/login");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu cập nhật hồ sơ không hợp lệ.";
                return RedirectToAction("Index", new { tab = "profile" });
            }

            // Lưu thông tin mới vào database
            var (success, error) = await _profileService.UpdateProfileAsync(userId.Value, model);
            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "Cập nhật thông tin thất bại.";
                return RedirectToAction("Index", new { tab = "profile" });
            }

            // Đồng bộ lại tên mới vào Session (chuẩn CatBaBooking)
            HttpContext.Session.SetString("UserName", model.FullName);

            TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
            return RedirectToAction("Index", new { tab = "profile" });
        }
    }
}
