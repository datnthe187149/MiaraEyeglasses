using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace MiaraEyeglasses.Services.Auth
{
    public interface ILogoutService
    {
        Task SignOutAsync(HttpContext httpContext);
    }

    public class LogoutService : ILogoutService
    {
        public async Task SignOutAsync(HttpContext httpContext)
        {
            httpContext.Session.Clear();
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}

