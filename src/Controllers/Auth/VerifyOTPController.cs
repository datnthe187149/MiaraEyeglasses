using Microsoft.AspNetCore.Mvc;

namespace MiaraEyeglasses.Controllers.Auth
{
    // Controller xác thực mã OTP từ Session (chuẩn CatBaBooking)
    public class VerifyOTPController : Controller
    {
        // [GET] Hiển thị form nhập mã OTP
        [HttpGet("/verify-otp")]
        [HttpGet("/Auth/VerifyOTP")]
        public IActionResult Index()
        {
            string? email = HttpContext.Session.GetString("EmailForgot");
            if (string.IsNullOrEmpty(email))
            {
                // Chưa gửi OTP -> chuyển về trang quên mật khẩu
                return Redirect("/forgot-password");
            }

            ViewBag.Email = email;
            return View("~/Views/Auth/VerifyOTP.cshtml");
        }

        // [POST] Kiểm tra mã OTP người dùng nhập so với Session
        [HttpPost("/verify-otp")]
        [HttpPost("/Auth/VerifyOTP")]
        public IActionResult Verify(string otp)
        {
            string? sessionOTP = HttpContext.Session.GetString("OTP");
            string? time = HttpContext.Session.GetString("Timeout");
            string? email = HttpContext.Session.GetString("EmailForgot");
            ViewBag.Email = email;

            if (string.IsNullOrEmpty(otp))
            {
                ViewBag.Error = "Vui lòng nhập mã OTP.";
                return View("~/Views/Auth/VerifyOTP.cshtml");
            }

            // Kiểm tra thời gian hết hạn (5 phút)
            if (string.IsNullOrEmpty(time) || DateTime.Now.Ticks > long.Parse(time))
            {
                ViewBag.Error = "Mã OTP đã hết hạn. Vui lòng gửi lại yêu cầu mới.";
                return View("~/Views/Auth/VerifyOTP.cshtml");
            }

            // So khớp mã OTP
            if (sessionOTP == null || !sessionOTP.Equals(otp.Trim()))
            {
                ViewBag.Error = "Mã OTP không chính xác. Vui lòng kiểm tra lại.";
                return View("~/Views/Auth/VerifyOTP.cshtml");
            }

            // Đặt cờ xác thực thành công vào Session
            HttpContext.Session.SetString("IsVerify", "true");

            // Chuyển hướng sang trang đặt lại mật khẩu mới
            return Redirect("/reset-password");
        }
    }
}
