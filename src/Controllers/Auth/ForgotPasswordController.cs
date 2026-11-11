using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.Auth;

namespace MiaraEyeglasses.Controllers.Auth
{
    // Controller xử lý Quên mật khẩu và gửi email liên kết đặt lại mật khẩu
    public class ForgotPasswordController : Controller
    {
        private readonly IForgotPasswordService _forgotPasswordService;
        private readonly IEmailService _emailService;

        public ForgotPasswordController(IForgotPasswordService forgotPasswordService, IEmailService emailService)
        {
            _forgotPasswordService = forgotPasswordService;
            _emailService = emailService;
        }

        // [GET] /ForgotPassword: Hiển thị form nhập email cần khôi phục mật khẩu
        [HttpGet]
        [Route("ForgotPassword")]
        [Route("Auth/ForgotPassword")]
        public IActionResult Index()
        {
            return View("~/Views/Auth/ForgotPassword.cshtml");
        }

        // [POST] /ForgotPassword: Tiếp nhận email và gửi token xác thực qua hòm thư
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ForgotPassword")]
        [Route("Auth/ForgotPassword")]
        public async Task<IActionResult> Index(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Auth/ForgotPassword.cshtml", model);

            // 1. Tạo token đặt lại mật khẩu ngẫu nhiên lưu vào database
            var (success, errorMessage, token) = await _forgotPasswordService.GenerateResetTokenAsync(model.Email);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Không thể tạo mã đặt lại mật khẩu.");
                return View("~/Views/Auth/ForgotPassword.cshtml", model);
            }

            // 2. Tạo đường link khôi phục chứa email và token bảo mật
            var resetLink = Url.Action("Index", "ResetPassword", new { email = model.Email, token = token }, Request.Scheme);

            // 3. Gửi email chứa đường dẫn đặt lại mật khẩu qua SMTP
            var (emailSent, emailError) = await _emailService.SendPasswordResetEmailAsync(model.Email, resetLink ?? "");

            if (emailSent)
            {
                TempData["SuccessMessage"] = $"Đã gửi email khôi phục mật khẩu đến {model.Email}. Vui lòng kiểm tra hộp thư đến (hoặc hòm thư Spam) để bấm vào liên kết!";
            }
            else
            {
                // Fallback khi chưa cấu hình SMTP: hiển thị link trực tiếp cho người dùng thử nghiệm
                TempData["SuccessMessage"] = $"Đã tạo yêu cầu thành công. (Ghi chú: Email chưa gửi được do [{emailError}] hoặc cấu hình email chưa đầy đủ; bạn có thể dùng liên kết trực tiếp bên dưới để đổi mật khẩu).";
                ViewBag.DirectResetLink = resetLink;
            }

            return View("~/Views/Auth/ForgotPasswordConfirmation.cshtml", model.Email);
        }
    }
}
