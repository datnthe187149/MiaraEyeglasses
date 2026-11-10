using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.Auth;

namespace MiaraEyeglasses.Controllers.Auth
{
    public class ForgotPasswordController : Controller
    {
        private readonly IForgotPasswordService _forgotPasswordService;
        private readonly IEmailService _emailService;

        public ForgotPasswordController(IForgotPasswordService forgotPasswordService, IEmailService emailService)
        {
            _forgotPasswordService = forgotPasswordService;
            _emailService = emailService;
        }

        [HttpGet]
        [Route("ForgotPassword")]
        [Route("Auth/ForgotPassword")]
        public IActionResult Index()
        {
            return View("~/Views/Auth/ForgotPassword.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ForgotPassword")]
        [Route("Auth/ForgotPassword")]
        public async Task<IActionResult> Index(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Auth/ForgotPassword.cshtml", model);

            var (success, errorMessage, token) = await _forgotPasswordService.GenerateResetTokenAsync(model.Email);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Không thể tạo mã đặt lại mật khẩu.");
                return View("~/Views/Auth/ForgotPassword.cshtml", model);
            }

            var resetLink = Url.Action("Index", "ResetPassword", new { email = model.Email, token = token }, Request.Scheme);
            var (emailSent, emailError) = await _emailService.SendPasswordResetEmailAsync(model.Email, resetLink ?? "");

            if (emailSent)
            {
                TempData["SuccessMessage"] = $"Đã gửi email khôi phục mật khẩu đến {model.Email}. Vui lòng kiểm tra hộp thư đến (hoặc hòm thư Spam) để bấm vào liên kết!";
            }
            else
            {
                TempData["SuccessMessage"] = $"Đã tạo yêu cầu thành công. (Ghi chú: Email chưa gửi được do [{emailError}] hoặc cấu hình email chưa đầy đủ; bạn có thể dùng liên kết trực tiếp bên dưới để đổi mật khẩu).";
                ViewBag.DirectResetLink = resetLink;
            }

            return View("~/Views/Auth/ForgotPasswordConfirmation.cshtml", model.Email);
        }
    }
}

