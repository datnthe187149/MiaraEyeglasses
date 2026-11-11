using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // Controller đổi mật khẩu cá nhân qua Session (chuẩn CatBaBooking)
    public class ChangePasswordController : Controller
    {
        private readonly IChangePasswordService _changePasswordService;

        public ChangePasswordController(IChangePasswordService changePasswordService)
        {
            _changePasswordService = changePasswordService;
        }

        // [POST] Tiếp nhận form đổi mật khẩu
        [HttpPost]
        [Route("ChangePassword")]
        [Route("User/ChangePassword")]
        public async Task<IActionResult> Index(ChangePasswordViewModel model)
        {
            // Kiểm tra trạng thái đăng nhập từ Session
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Redirect("/login");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu đổi mật khẩu không hợp lệ.";
                return RedirectToAction("Index", "Profile", new { tab = "password" });
            }

            // Gọi Service đổi mật khẩu (so sánh pass cũ và lưu pass mới đã băm BCrypt)
            var (success, error) = await _changePasswordService.ChangePasswordAsync(userId, model);
            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "Đổi mật khẩu thất bại.";
                return RedirectToAction("Index", "Profile", new { tab = "password" });
            }

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index", "Profile", new { tab = "password" });
        }
    }
}
