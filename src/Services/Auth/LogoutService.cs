using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace MiaraEyeglasses.Services.Auth
{
    // Interface dịch vụ đăng xuất
    public interface ILogoutService
    {
        // Thực thi tác vụ: SignOut
        Task SignOutAsync(HttpContext httpContext);
    }

    // Service xóa sạch phiên đăng nhập và cookie người dùng
    public class LogoutService : ILogoutService
    {
        // Thực thi tác vụ: SignOut
        public async Task SignOutAsync(HttpContext httpContext)
        {
            httpContext.Session.Clear();
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}

