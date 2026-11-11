using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.Auth;

namespace MiaraEyeglasses.Controllers.Auth
{
    // Controller xử lý Đặt lại mật khẩu mới thông qua Token bảo mật
    public class ResetPasswordController : Controller
    {
        private readonly IResetPasswordService _resetPasswordService;

        public ResetPasswordController(IResetPasswordService resetPasswordService)
        {
            _resetPasswordService = resetPasswordService;
        }

        // [GET] /ResetPassword: Màn hình nhập mật khẩu mới (nhận email và token từ liên kết email)
        [HttpGet]
        [Route("ResetPassword")]
        [Route("Auth/ResetPassword")]
        public IActionResult Index(string email, string token)
        {
            var model = new ResetPasswordViewModel { Email = email, Token = token };
            return View("~/Views/Auth/ResetPassword.cshtml", model);
        }

        // [POST] /ResetPassword: Lưu mật khẩu mới đã xác thực token
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ResetPassword")]
        [Route("Auth/ResetPassword")]
        public async Task<IActionResult> Index(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Auth/ResetPassword.cshtml", model);

            // Gọi Service xác thực token hợp lệ và băm mật khẩu mới bằng BCrypt
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
