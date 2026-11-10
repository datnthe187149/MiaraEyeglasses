using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MiaraEyeglasses.Filters
{
    /// <summary>
    /// Chặn tài khoản Quản trị viên (Admin) và Nhân viên (Staff) truy cập vào các trang dành riêng cho Khách hàng mua sắm
    /// (như Profile cá nhân, Sổ địa chỉ, Độ cận cá nhân, Giỏ hàng, Đặt hàng Checkout...).
    /// </summary>
    public class CustomerOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
            {
                if (user.IsInRole("Admin"))
                {
                    if (context.Controller is Controller controller)
                    {
                        controller.TempData["ErrorMessage"] = "Tài khoản Quản trị viên (Admin) không thể thực hiện các thao tác mua hàng của Khách hàng. Vui lòng sử dụng trang Admin Portal.";
                    }
                    context.Result = new RedirectToActionResult("Index", "AdminDashboard", null);
                    return;
                }

                if (user.IsInRole("Staff"))
                {
                    if (context.Controller is Controller controller)
                    {
                        controller.TempData["ErrorMessage"] = "Tài khoản Nhân viên (Staff) không thể thực hiện các thao tác mua hàng của Khách hàng. Vui lòng sử dụng trang Staff Portal.";
                    }
                    context.Result = new RedirectToActionResult("Index", "StaffOrder", null);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}

