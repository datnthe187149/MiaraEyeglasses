using Microsoft.AspNetCore.Mvc;

namespace MiaraEyeglasses.Controllers.Auth
{
    // Controller đăng xuất thuần Session (y hệt CatBaBooking)
    [Route("logout")]
    public class LogoutController : Controller
    {
        [HttpGet, HttpPost]
        public IActionResult Index()
        {
            HttpContext.Session.Clear();
            return Redirect("/login");
        }
    }
}
