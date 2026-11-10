using Microsoft.AspNetCore.Mvc.Filters;

namespace MiaraEyeglasses.Filters
{
    // Filter cho phép Admin và Staff kiểm thử các chức năng mua hàng của Khách hàng
    public class CustomerOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }
    }
}
