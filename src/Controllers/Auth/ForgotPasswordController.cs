using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Auth;

namespace MiaraEyeglasses.Controllers.Auth
{
    // Controller xử lý Quên mật khẩu: gửi OTP và lưu phiên vào Session (chuẩn CatBaBooking)
    public class ForgotPasswordController : Controller
    {
        private readonly IForgotPasswordService _forgotPasswordService;

        public ForgotPasswordController(IForgotPasswordService forgotPasswordService)
        {
            _forgotPasswordService = forgotPasswordService;
        }

        // [GET] Hiển thị giao diện nhập Email
        [HttpGet("/forgot-password")]
        [HttpGet("/Auth/ForgotPassword")]
        public IActionResult Index()
        {
            return View("~/Views/Auth/ForgotPassword.cshtml");
        }

        // [POST] Gửi mã OTP về email và lưu thông tin vào Session
        [HttpPost("/forgot-password")]
        [HttpPost("/Auth/ForgotPassword")]
        public async Task<IActionResult> SendOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Email không được để trống.";
                return View("~/Views/Auth/ForgotPassword.cshtml");
            }

            string cleanEmail = email.Trim();
            string? otp = await _forgotPasswordService.SendOTPAsync(cleanEmail);
            if (otp == null)
            {
                ViewBag.Error = "Email không tồn tại trên hệ thống hoặc tài khoản đang bị khóa.";
                return View("~/Views/Auth/ForgotPassword.cshtml");
            }

            // Lưu thông tin vào Session (chuẩn CatBaBooking)
            HttpContext.Session.SetString("EmailForgot", cleanEmail);
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("Timeout", DateTime.Now.AddMinutes(5).Ticks.ToString());

            // Chuyển hướng sang trang nhập mã OTP
            return Redirect("/verify-otp");
        }
    }
}
