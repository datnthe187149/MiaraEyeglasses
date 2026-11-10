using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MiaraEyeglasses.Filters
{
    // Filter này có tác dụng: Chặn tài khoản Quản trị viên (Admin) và Nhân viên (Staff) 
    // truy cập vào các trang dành riêng cho việc mua sắm của Khách hàng (như Giỏ hàng, Đặt hàng Checkout).
    // Nếu Admin hoặc Staff cố tình vào mua hàng, hệ thống sẽ tự động điều hướng họ về trang quản trị tương ứng.
    public class CustomerOnlyAttribute : ActionFilterAttribute
    {
        // Hàm này tự động chạy TRƯỚC KHI Controller Action được thực thi
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Lấy thông tin người dùng hiện tại đang gửi yêu cầu
            var user = context.HttpContext.User;

            // Kiểm tra xem người dùng này đã đăng nhập hay chưa
            if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
            {
                // Nếu người đăng nhập có vai trò là Quản trị viên (Admin)
                if (user.IsInRole("Admin"))
                {
                    var controller = context.Controller as Controller;
                    if (controller != null)
                    {
                        controller.TempData["ErrorMessage"] = "Tài khoản Quản trị viên (Admin) không thể thực hiện các thao tác mua hàng của Khách hàng. Vui lòng sử dụng trang Admin Portal.";
                    }
                    // Chuyển hướng ngay lập tức về trang bảng điều khiển của Admin
                    context.Result = new RedirectToActionResult("Index", "AdminDashboard", null);
                    return;
                }

                // Nếu người đăng nhập có vai trò là Nhân viên (Staff)
                if (user.IsInRole("Staff"))
                {
                    var controller = context.Controller as Controller;
                    if (controller != null)
                    {
                        controller.TempData["ErrorMessage"] = "Tài khoản Nhân viên (Staff) không thể thực hiện các thao tác mua hàng của Khách hàng. Vui lòng sử dụng trang Staff Portal.";
                    }
                    // Chuyển hướng ngay lập tức về trang quản lý đơn của Nhân viên
                    context.Result = new RedirectToActionResult("Index", "StaffOrder", null);
                    return;
                }
            }

            // Nếu là Khách hàng (Customer) hoặc Khách vãng lai (Guest chưa đăng nhập) thì cho phép đi tiếp
            base.OnActionExecuting(context);
        }
    }
}
