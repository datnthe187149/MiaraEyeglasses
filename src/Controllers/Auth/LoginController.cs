using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Auth;

namespace MiaraEyeglasses.Controllers.Auth
{
    // Controller đăng nhập thuần Session (y hệt CatBaBooking)
    [Route("login")]
    public class LoginController : Controller
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        // GET: /login
        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Auth/Login.cshtml");
        }

        // POST: /login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email và mật khẩu không được để trống";
                return View("~/Views/Auth/Login.cshtml");
            }

            var user = await _loginService.LoginAsync(email, password);
            if (user == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không chính xác hoặc tài khoản của bạn đang bị khóa.";
                return View("~/Views/Auth/Login.cshtml");
            }

            string roleName = user.RoleId == 3 ? "Admin" : (user.RoleId == 2 ? "Staff" : "Customer");

            // Lưu trực tiếp các thông tin người dùng vào Session (chuẩn CatBaBooking)
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("RoleId", user.RoleId.ToString());
            HttpContext.Session.SetString("RoleName", roleName);
            HttpContext.Session.SetString("UserRole", roleName);
            HttpContext.Session.SetString("UserEmail", user.Email);

            string? role = HttpContext.Session.GetString("RoleName");

            // Phân luồng điều hướng theo Role (chuẩn CatBaBooking)
            switch (role?.ToLower().Trim())
            {
                case "admin":
                    return Redirect("/Admin/Dashboard");
                case "staff":
                    return Redirect("/Staff/Order");
                default:
                    return Redirect("/");
            }
        }
    }
}
