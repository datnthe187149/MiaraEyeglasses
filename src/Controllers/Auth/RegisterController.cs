using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.Auth;

namespace MiaraEyeglasses.Controllers.Auth
{
    // Controller xử lý Đăng ký tài khoản mới cho Khách hàng
    public class RegisterController : Controller
    {
        private readonly IRegisterService _registerService;

        public RegisterController(IRegisterService registerService)
        {
            _registerService = registerService;
        }

        // [GET] /Register: Hiển thị form đăng ký thành viên
        [HttpGet]
        [Route("Register")]
        [Route("Auth/Register")]
        public IActionResult Index()
        {
            return View("~/Views/Auth/Register.cshtml", new RegisterViewModel());
        }

        // [POST] /Register: Tiếp nhận thông tin đăng ký (Họ tên, Email, Mật khẩu, SĐT)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Register")]
        [Route("Auth/Register")]
        public async Task<IActionResult> Index(RegisterViewModel model)
        {
            // Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
                return View("~/Views/Auth/Register.cshtml", model);

            // Gọi Service tạo tài khoản mới (băm mật khẩu BCrypt và gán quyền Customer)
            var (success, errorMessage, user) = await _registerService.RegisterAsync(model);
            if (!success || user == null)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Đăng ký thất bại.");
                return View("~/Views/Auth/Register.cshtml", model);
            }

            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Index", "Login");
        }
    }
}
