using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Auth;

namespace MiaraEyeglasses.Controllers.Auth
{
    // Controller xử lý Đăng xuất khỏi hệ thống
    public class LogoutController : Controller
    {
        private readonly ILogoutService _logoutService;

        public LogoutController(ILogoutService logoutService)
        {
            _logoutService = logoutService;
        }

        // [GET/POST] /Logout: Xóa cookie xác thực và hủy session đăng nhập
        [HttpPost]
        [HttpGet]
        [Route("Logout")]
        [Route("Auth/Logout")]
        public async Task<IActionResult> Index()
        {
            // Thực hiện đăng xuất khỏi Cookie Authentication và xóa session
            await _logoutService.SignOutAsync(HttpContext);
            return RedirectToAction("Index", "Home");
        }
    }
}
