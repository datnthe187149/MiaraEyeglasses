using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // Controller xử lý đổi mật khẩu tài khoản người dùng
    // Xác thực mật khẩu cũ và băm mật khẩu mới bằng BCrypt
    [Authorize]
    [CustomerOnly]
    public class ChangePasswordController : Controller
    {
        private readonly IChangePasswordService _changePasswordService;

        public ChangePasswordController(IChangePasswordService changePasswordService)
        {
            _changePasswordService = changePasswordService;
        }

        // [POST] Tiếp nhận form đổi mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ChangePassword")]
        [Route("User/ChangePassword")]
        public async Task<IActionResult> Index(ChangePasswordViewModel model)
        {
            // Kiểm tra người dùng đã đăng nhập chưa
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Index", "Login");

            // Kiểm tra form nhập có hợp lệ không
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu đổi mật khẩu không hợp lệ.";
                return RedirectToAction("Index", "Profile", new { tab = "password" });
            }

            // Gọi Service đổi mật khẩu (kiểm tra pass cũ và lưu pass mới đã băm)
            var (success, error) = await _changePasswordService.ChangePasswordAsync(userId.Value, model);
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
