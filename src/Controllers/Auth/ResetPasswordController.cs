using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.Auth;

namespace MiaraEyeglasses.Controllers.Auth
{
    public class ResetPasswordController : Controller
    {
        private readonly IResetPasswordService _resetPasswordService;

        public ResetPasswordController(IResetPasswordService resetPasswordService)
        {
            _resetPasswordService = resetPasswordService;
        }

        [HttpGet]
        [Route("ResetPassword")]
        [Route("Auth/ResetPassword")]
        public IActionResult Index(string email, string token)
        {
            var model = new ResetPasswordViewModel { Email = email, Token = token };
            return View("~/Views/Auth/ResetPassword.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ResetPassword")]
        [Route("Auth/ResetPassword")]
        public async Task<IActionResult> Index(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Auth/ResetPassword.cshtml", model);

            var (success, errorMessage) = await _resetPasswordService.ResetPasswordAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Đặt lại mật khẩu thất bại.");
                return View("~/Views/Auth/ResetPassword.cshtml", model);
            }

            TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập bằng mật khẩu mới.";
            return RedirectToAction("Index", "Login");
        }
    }
}

