using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.Auth;

namespace MiaraEyeglasses.Controllers.Auth
{
    // Controller xử lý Xác thực & Đăng nhập (Authentication) cho toàn bộ hệ thống
    // Quản lý cấp phát Cookie xác thực, Claims phân quyền và lưu trữ Session
    public class LoginController : Controller
    {
        private readonly ILoginService _loginService;

        // Tiêm phụ thuộc ILoginService qua Constructor để xử lý nghiệp vụ kiểm tra tài khoản
        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        // [GET] /Login hoặc /Auth/Login: Hiển thị trang giao diện Form đăng nhập
        // returnUrl: Đường dẫn trang người dùng định vào trước khi bị chuyển về login
        [HttpGet]
        [Route("Login")]
        [Route("Auth/Login")]
        public IActionResult Index(string? returnUrl = null)
        {
            // Nếu người dùng đã đăng nhập từ trước, tự động chuyển về portal tương ứng
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin")) return RedirectToAction("Index", "AdminDashboard");
                if (User.IsInRole("Staff")) return RedirectToAction("Index", "StaffOrder");
                return RedirectToAction("Index", "Home");
            }

            return View("~/Views/Auth/Login.cshtml", new LoginViewModel { ReturnUrl = returnUrl });
        }

        // [POST] Tiếp nhận thông tin đăng nhập từ Form người dùng gửi lên
        [HttpPost]
        [ValidateAntiForgeryToken] // Chống tấn công giả mạo CSRF
        [Route("Login")]
        [Route("Auth/Login")]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            // Bước 1: Kiểm tra tính hợp lệ dữ liệu Form ([Required], [EmailAddress])
            if (!ModelState.IsValid)
                return View("~/Views/Auth/Login.cshtml", model);

            // Bước 2: Gọi tầng Service xác thực tài khoản (so khớp hash BCrypt, kiểm tra cờ IsActive)
            var (success, errorMessage, user) = await _loginService.AuthenticateAsync(model.Email, model.Password);
            if (!success || user == null)
            {
                // Thêm lỗi vào ModelState để Razor View hiển thị thông báo lỗi màu đỏ trên form
                ModelState.AddModelError(string.Empty, errorMessage ?? "Đăng nhập thất bại.");
                return View("~/Views/Auth/Login.cshtml", model);
            }

            // Bước 3: Xác định tên vai trò và lưu thông tin vào Session
            string roleName = GetRoleName(user.RoleId);
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", roleName);

            // Bước 4: Khởi tạo danh sách Claims định danh danh tính và vai trò
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Bước 5: Ghi nhận đăng nhập và phát hành Cookie mã hóa an toàn về trình duyệt
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe, // Ghi nhớ đăng nhập
                    ExpiresUtc = DateTime.UtcNow.AddDays(7) // Cookie có hạn 7 ngày
                });

            // Bước 6: Điều hướng thông minh sau khi đăng nhập thành công
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            // Phân luồng điều hướng theo Role
            if (user.RoleId == 3)
                return RedirectToAction("Index", "AdminDashboard"); // Quản trị viên vào Admin Dashboard
            if (user.RoleId == 2)
                return RedirectToAction("Index", "StaffOrder");     // Kỹ thuật viên vào Staff Portal

            // Khách hàng quay về Trang chủ
            return RedirectToAction("Index", "Home");
        }

        // [GET] Trang thông báo lỗi khi truy cập trang không đủ quyền hạn (403 Forbidden)
        [HttpGet]
        [Route("Auth/AccessDenied")]
        [Route("AccessDenied")]
        public IActionResult AccessDenied()
        {
            TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này.";
            return RedirectToAction("Index", "Home");
        }

        // Chuyển mã RoleId (số nguyên) thành tên vai trò để dễ đọc và quản lý
        private static string GetRoleName(int roleId)
        {
            if (roleId == 3)
            {
                return "Admin";
            }
            else if (roleId == 2)
            {
                return "Staff";
            }
            else
            {
                return "Customer";
            }
        }
    }
}
