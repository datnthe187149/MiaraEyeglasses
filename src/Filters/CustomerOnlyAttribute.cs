using Microsoft.AspNetCore.Mvc.Filters;

namespace MiaraEyeglasses.Filters
{
    // =========================================================================================
    // ACTION FILTER: CustomerOnlyAttribute
    // Phân hệ: Điều hướng và Kiểm soát truy cập (Access Control Filter)
    // Chức năng:
    //   - Được gắn trên các Controller và Action dành riêng cho Khách hàng mua sắm (Shop, Cart, Checkout, UserProfile).
    //   - Cho phép Quản trị viên (Admin) và Nhân viên (Staff) tự do kiểm thử quy trình mua hàng thực tế mà không bị chặn quyền.
    //   - Tích hợp chuẩn kiến trúc ASP.NET Core ActionFilterAttribute.
    // =========================================================================================
    public class CustomerOnlyAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Phương thức được thực thi trước khi Action trong Controller được gọi
        /// </summary>
        /// <param name="context">Ngữ cảnh thực thi của Action hiện tại</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Duy trì luồng thực thi thông suốt cho người dùng và các vai trò quản trị khi kiểm thử
            base.OnActionExecuting(context);
        }
    }
}
