using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    [Authorize]
    [CustomerOnly]
    public class ChangePasswordController : Controller
    {
        private readonly IChangePasswordService _changePasswordService;

        public ChangePasswordController(IChangePasswordService changePasswordService)
        {
            _changePasswordService = changePasswordService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ChangePassword")]
        [Route("User/ChangePassword")]
        public async Task<IActionResult> Index(ChangePasswordViewModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Index", "Login");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu đổi mật khẩu không hợp lệ.";
                return RedirectToAction("Index", "Profile");
            }

            var (success, error) = await _changePasswordService.ChangePasswordAsync(userId.Value, model);
            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "Đổi mật khẩu thất bại.";
                return RedirectToAction("Index", "Profile");
            }

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index", "Profile");
        }
    }
}

