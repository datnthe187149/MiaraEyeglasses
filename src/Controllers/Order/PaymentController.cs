using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Order;

namespace MiaraEyeglasses.Controllers.Order
{
    // Controller chuyên biệt xử lý Tích hợp Thanh toán trực tuyến (Payment Gateway)
    // Hỗ trợ Cổng thanh toán VNPAY (Sandbox & Production):
    // - Nhận kết quả từ trình duyệt (VnPayReturn)
    // - Nhận webhook IPN từ máy chủ VNPAY (VnPayIpn)
    // - Hỗ trợ thanh toán lại cho đơn chờ xử lý (RetryPayment)
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly IOrderService _orderService;

        // Tiêm phụ thuộc IVnPayService và IOrderService qua Constructor
        public PaymentController(IVnPayService vnPayService, IOrderService orderService)
        {
            _vnPayService = vnPayService;
            _orderService = orderService;
        }

        // [GET] /Payment/VnPayReturn: Nhận kết quả thanh toán khi khách hàng được chuyển hướng về từ VNPAY
        [HttpGet]
        public async Task<IActionResult> VnPayReturn()
        {
            // Bước 1: Chuyển toàn bộ tham số nhận được trong Request.Query sang VnPayService để kiểm tra chữ ký HMAC-SHA512
            var (result, message) = await _vnPayService.ProcessPaymentReturnAsync(Request.Query);

            // Bước 2: Nạp các thông tin quan trọng sang ViewBag để hiển thị trên màn hình xác nhận đơn
            ViewBag.Result = result;                                         // "SUCCESS", "FAILED", "INVALID_SIGNATURE"
            ViewBag.Message = message;                                       // Lời nhắn giải thích kết quả
            ViewBag.TxnRef = Request.Query["vnp_TxnRef"].ToString();         // Mã tham chiếu đơn hàng
            ViewBag.Amount = Request.Query["vnp_Amount"].ToString();         // Số tiền đã thanh toán (đơn vị VNPAY = x100)
            ViewBag.TransactionNo = Request.Query["vnp_TransactionNo"].ToString(); // Mã giao dịch của cổng VNPAY

            // Bước 3: Trả về Razor View thông báo kết quả cho khách hàng
            return View("~/Views/Payment/VnPayReturn.cshtml");
        }

        // [GET] Webhook (IPN - Instant Payment Notification) nhận thông báo từ máy chủ VNPAY gửi sang MIARA
        [HttpGet]
        public async Task<IActionResult> VnPayIpn()
        {
            var (result, message) = await _vnPayService.ProcessPaymentReturnAsync(Request.Query);

            // Trả về mã phản hồi chuẩn theo quy định tích hợp kỹ thuật của VNPAY
            if (result == "SUCCESS")
                return Json(new { RspCode = "00", Message = "Confirm Success" });
            if (result == "ALREADY_PROCESSED")
                return Json(new { RspCode = "02", Message = "Order already confirmed" });
            if (result == "INVALID_AMOUNT")
                return Json(new { RspCode = "04", Message = "Invalid amount" });

            return Json(new { RspCode = "99", Message = message ?? "Unknown error" });
        }

        // [POST] Cho phép khách hàng thanh toán lại đơn hàng cũ nếu bị gián đoạn hoặc hủy thanh toán
        [HttpPost]
        public async Task<IActionResult> RetryPayment(int orderId)
        {
            // Bước 1: Lấy chi tiết đơn hàng
            var order = await _orderService.GetOrderDetailsAsync(orderId);
            if (order == null || order.Status != 0)
                return BadRequest("Đơn hàng không ở trạng thái chờ thanh toán.");

            // Bước 2: Tạo URL thanh toán mới với VNPAY
            var paymentUrl = _vnPayService.CreatePaymentUrl(order, HttpContext);

            // Bước 3: Chuyển hướng người dùng sang cổng thanh toán VNPAY
            return Redirect(paymentUrl);
        }
    }
}
