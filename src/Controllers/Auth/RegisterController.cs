using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.Auth;

namespace MiaraEyeglasses.Controllers.Auth
{
    // Controller xử lý Đăng ký tài khoản khách hàng mới
    public class RegisterController : Controller
    {
        private readonly IRegisterService _registerService;

        public RegisterController(IRegisterService registerService)
        {
            _registerService = registerService;
        }

        // [GET] Hiển thị form đăng ký thành viên
        [HttpGet("/register")]
        [HttpGet("/register-customer")]
        [HttpGet("/Auth/Register")]
        public IActionResult Index()
        {
            return View("~/Views/Auth/Register.cshtml", new RegisterViewModel());
        }

        // [POST] Tiếp nhận thông tin đăng ký
        [HttpPost("/register")]
        [HttpPost("/register-customer")]
        [HttpPost("/Auth/Register")]
        public async Task<IActionResult> Index(RegisterViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.FullName) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                ViewBag.Error = "Thông tin không được để trống.";
                return View("~/Views/Auth/Register.cshtml", model);
            }

            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ViewBag.Error = "Mật khẩu và xác nhận mật khẩu không khớp.";
                return View("~/Views/Auth/Register.cshtml", model);
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Auth/Register.cshtml", model);
            }

            // Gọi Service tạo tài khoản mới với quyền Customer (RoleId = 1)
            var (success, errorMessage, user) = await _registerService.RegisterAsync(model);
            if (!success || user == null)
            {
                ViewBag.Error = errorMessage ?? "Đăng ký thất bại. Email có thể đã được đăng ký.";
                ModelState.AddModelError(string.Empty, ViewBag.Error);
                return View("~/Views/Auth/Register.cshtml", model);
            }

            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
            return Redirect("/login");
        }
    }
}
