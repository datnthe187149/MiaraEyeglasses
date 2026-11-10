using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Admin;

namespace MiaraEyeglasses.Controllers.Admin
{
    // Controller quản lý Đơn hàng cho Quản trị viên (Admin)
    // Hệ thống chỉ theo dõi 3 trạng thái: Đang thanh toán, Đã thanh toán, Đã hủy.
    // Quản trị viên chỉ có quyền xem chi tiết đơn hàng, không tự ý chỉnh sửa hay hủy đơn.
    [Authorize(Roles = "Admin")]
    public class AdminOrderController : Controller
    {
        private readonly IAdminOrderService _orderService;

        public AdminOrderController(IAdminOrderService orderService)
        {
            _orderService = orderService;
        }

        // [GET] /Admin/Orders: Hiển thị danh sách đơn hàng lọc theo 3 trạng thái
        [HttpGet]
        [Route("Admin/Orders")]
        [Route("AdminOrder")]
        public async Task<IActionResult> Index(byte? status = null)
        {
            var orders = await _orderService.GetAllOrdersAsync(status);
            ViewBag.SelectedStatus = status;
            return View("~/Views/Admin/Orders.cshtml", orders);
        }

        // [GET] /Admin/Orders/Detail/{id}: Xem thông tin chi tiết một đơn hàng
        [HttpGet]
        [Route("Admin/Orders/Detail/{id}")]
        [Route("AdminOrder/Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Mã đơn hàng không hợp lệ.";
                return RedirectToAction("Index");
            }

            var order = await _orderService.GetOrderDetailsAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng yêu cầu.";
                return RedirectToAction("Index");
            }

            return View("~/Views/Admin/OrderDetail.cshtml", order);
        }
    }
}

