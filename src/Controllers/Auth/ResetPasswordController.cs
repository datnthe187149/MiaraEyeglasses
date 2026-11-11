using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Helpers;
using MiaraEyeglasses.Services.Auth;

namespace MiaraEyeglasses.Controllers.Auth
{
    // Controller đặt lại mật khẩu mới thông qua Session (chuẩn CatBaBooking)
    public class ResetPasswordController : Controller
    {
        private readonly IForgotPasswordService _forgotPasswordService;

        public ResetPasswordController(IForgotPasswordService forgotPasswordService)
        {
            _forgotPasswordService = forgotPasswordService;
        }

        // [GET] Hiển thị form tạo mật khẩu mới (chỉ cho phép khi đã xác thực OTP thành công)
        [HttpGet("/reset-password")]
        [HttpGet("/Auth/ResetPassword")]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("IsVerify") != "true")
            {
                return Redirect("/forgot-password");
            }

            ViewBag.Email = HttpContext.Session.GetString("EmailForgot");
            return View("~/Views/Auth/ResetPassword.cshtml");
        }

        // [POST] Thực hiện cập nhật mật khẩu mới
        [HttpPost("/reset-password")]
        [HttpPost("/Auth/ResetPassword")]
        public async Task<IActionResult> ResetPassword(string newPassword, string confirmPassword)
        {
            // Kiểm tra trạng thái đã xác thực OTP trong Session
            if (HttpContext.Session.GetString("IsVerify") != "true")
            {
                return Redirect("/forgot-password");
            }

            string? email = HttpContext.Session.GetString("EmailForgot");
            ViewBag.Email = email;

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ mật khẩu mới và xác nhận mật khẩu.";
                return View("~/Views/Auth/ResetPassword.cshtml");
            }

            if (!newPassword.Equals(confirmPassword))
            {
                ViewBag.Error = "Mật khẩu mới và xác nhận mật khẩu không khớp nhau.";
                return View("~/Views/Auth/ResetPassword.cshtml");
            }

            if (!PasswordHelper.IsStrongPassword(newPassword))
            {
                ViewBag.Error = "Mật khẩu tối thiểu phải từ 6 ký tự trở lên.";
                return View("~/Views/Auth/ResetPassword.cshtml");
            }

            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Không tìm thấy thông tin email trong phiên làm việc. Vui lòng thực hiện lại.";
                return Redirect("/forgot-password");
            }

            // Gọi service cập nhật mật khẩu mới vào CSDL
            bool isSuccess = await _forgotPasswordService.ResetPasswordAsync(email, newPassword);
            if (!isSuccess)
            {
                ViewBag.Error = "Đã xảy ra lỗi khi cập nhật mật khẩu. Vui lòng thử lại.";
                return View("~/Views/Auth/ResetPassword.cshtml");
            }

            // Dọn dẹp toàn bộ dữ liệu Session tạm (OTP, IsVerify, EmailForgot)
            HttpContext.Session.Clear();

            TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Mời bạn đăng nhập bằng mật khẩu mới.";
            return Redirect("/login");
        }
    }
}
