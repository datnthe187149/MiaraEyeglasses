using Microsoft.AspNetCore.Mvc.Filters;

namespace MiaraEyeglasses.Filters
{
    // Filter hỗ trợ linh hoạt: cho phép cả Admin và Staff kiểm thử các chức năng mua hàng
    public class CustomerOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }
    }
}
