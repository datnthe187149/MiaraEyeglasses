using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Auth;

namespace MiaraEyeglasses.Controllers.Auth
{
    public class LogoutController : Controller
    {
        private readonly ILogoutService _logoutService;

        public LogoutController(ILogoutService logoutService)
        {
            _logoutService = logoutService;
        }

        [HttpPost]
        [HttpGet]
        [Route("Logout")]
        [Route("Auth/Logout")]
        public async Task<IActionResult> Index()
        {
            await _logoutService.SignOutAsync(HttpContext);
            return RedirectToAction("Index", "Home");
        }
    }
}

