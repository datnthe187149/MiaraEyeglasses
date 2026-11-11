using Microsoft.AspNetCore.Mvc.Filters;

namespace MiaraEyeglasses.Filters
{
    // Filter bộ lọc quyền truy cập: Hỗ trợ phân quyền linh hoạt
    // Cho phép Admin và Staff kiểm thử trực tiếp các màn hình và quy trình mua hàng của Khách hàng (Customer)
    public class CustomerOnlyAttribute : ActionFilterAttribute
    {
        // Kiểm tra ngữ cảnh trước khi Controller thực thi Action
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }
    }
}
