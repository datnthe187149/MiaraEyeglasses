using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.Auth;

namespace MiaraEyeglasses.Controllers.Auth
{
    public class RegisterController : Controller
    {
        private readonly IRegisterService _registerService;

        public RegisterController(IRegisterService registerService)
        {
            _registerService = registerService;
        }

        [HttpGet]
        [Route("Register")]
        [Route("Auth/Register")]
        public IActionResult Index()
        {
            return View("~/Views/Auth/Register.cshtml", new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Register")]
        [Route("Auth/Register")]
        public async Task<IActionResult> Index(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Auth/Register.cshtml", model);

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

