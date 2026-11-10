using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Staff;

namespace MiaraEyeglasses.Controllers.Staff
{
    // Controller phục vụ Kỹ thuật viên (Staff) mài cắt tròng và xử lý tiến độ đơn hàng
    // Quyền hạn: [Authorize(Roles = "Admin,Staff")]
    // Chức năng: Xem đơn, xem phiếu thông số cắt tròng (SPH, CYL, Trục, PD), cập nhật trạng thái đơn
    [Authorize(Roles = "Admin,Staff")]
    public class StaffOrderController : Controller
    {
        private readonly IStaffOrderService _staffOrderService;

        // Tiêm phụ thuộc IStaffOrderService qua Constructor
        public StaffOrderController(IStaffOrderService staffOrderService)
        {
            _staffOrderService = staffOrderService;
        }

        // Lấy ID của nhân viên đang đăng nhập để ghi nhận vào lịch sử thao tác đơn (Audit Log)
        private int GetCurrentStaffId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) return userId;
            return 2;
        }

        // [GET] /Staff hoặc /Staff/Orders: Hiển thị danh sách đơn hàng cần xử lý
        // status: 1=Đã thanh toán, 2=Đang cắt tròng, 3=Đang giao,...
        [HttpGet]
        [Route("Staff")]
        [Route("Staff/Index")]
        [Route("Staff/Orders")]
        [Route("StaffOrder")]
        public async Task<IActionResult> Index(byte? status = null)
        {
            // Nếu tài khoản là Admin, tự động chuyển về trang AdminOrder để giữ nguyên menu 11 mục của Admin
            if (User.IsInRole("Admin") || HttpContext.Session.GetString("UserRole") == "Admin")
            {
                return RedirectToAction("Index", "AdminOrder");
            }

            // Gọi Service lấy danh sách đơn hàng
            var orders = await _staffOrderService.GetAssignedOrdersAsync(status);
            ViewBag.SelectedStatus = status;
            return View("~/Views/Staff/Index.cshtml", orders);
        }

        // [GET] Hiển thị Phiếu kỹ thuật chi tiết đơn hàng (Thông số độ cận SPH, CYL, Trục, PD để cắt kính)
        [HttpGet]
        public async Task<IActionResult> OrderDetail(int id)
        {
            var order = await _staffOrderService.GetOrderDetailsAsync(id);
            if (order == null)
                return NotFound();

            return View("~/Views/Staff/OrderDetail.cshtml", order);
        }

        // [POST] Bắt đầu quá trình mài cắt tròng kính: Chuyển Status = 2 (Đang xử lý / Cắt tròng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCutting(int orderId)
        {
            int staffId = GetCurrentStaffId();
            var (success, error) = await _staffOrderService.ProcessLensCuttingAsync(orderId, staffId);
            if (success)
                TempData["SuccessMessage"] = $"Đơn hàng #{orderId} đã chuyển sang trạng thái: Đang cắt tròng kính.";
            else
                TempData["ErrorMessage"] = error ?? "Thao tác thất bại.";

            return RedirectToAction("Index");
        }

        // [POST] Hoàn tất mài lắp tròng vào gọng: Chuyển Status = 3 (Đang giao hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ship(int orderId)
        {
            int staffId = GetCurrentStaffId();
            var (success, error) = await _staffOrderService.ShipOrderAsync(orderId, staffId);
            if (success)
                TempData["SuccessMessage"] = $"Đơn hàng #{orderId} đã chuyển sang trạng thái: Đang giao hàng.";
            else
                TempData["ErrorMessage"] = error ?? "Thao tác thất bại.";

            return RedirectToAction("Index");
        }

        // [POST] Khách hàng đã nhận kính: Chuyển Status = 4 (Hoàn thành đơn hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int orderId)
        {
            int staffId = GetCurrentStaffId();
            var (success, error) = await _staffOrderService.CompleteOrderAsync(orderId, staffId);
            if (success)
                TempData["SuccessMessage"] = $"Đơn hàng #{orderId} đã hoàn thành!";
            else
                TempData["ErrorMessage"] = error ?? "Thao tác thất bại.";

            return RedirectToAction("Index");
        }
    }
}
