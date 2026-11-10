using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Staff;

namespace MiaraEyeglasses.Controllers.Staff
{
    // Controller quản lý Tồn kho (Inventory) cho Nhân viên Kỹ thuật
    [Authorize(Roles = "Admin,Staff")]
    public class StaffInventoryController : Controller
    {
        private readonly IStaffInventoryService _inventoryService;

        public StaffInventoryController(IStaffInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // [GET] /Staff/Inventory: Hiển thị danh sách tồn kho từng biến thể gọng kính
        [HttpGet]
        [Route("Staff/Inventory")]
        [Route("StaffInventory")]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin") || HttpContext.Session.GetString("UserRole") == "Admin")
            {
                return RedirectToAction("Index", "AdminInventory");
            }

            var variants = await _inventoryService.GetInventoryVariantsAsync();
            return View("~/Views/Staff/Inventory.cshtml", variants);
        }

        // [POST] Cập nhật số lượng tồn kho (Có validate số lượng không được âm)
        [HttpPost]
        [Route("Staff/UpdateStock")]
        [Route("StaffInventory/UpdateStock")]
        public async Task<IActionResult> UpdateStock(int variantId, int newStock)
        {
            // 1. Validate: ID biến thể phải hợp lệ
            if (variantId <= 0)
            {
                TempData["ErrorMessage"] = "Biến thể sản phẩm không hợp lệ.";
                return RedirectToAction("Index");
            }

            // 2. Validate: Số lượng tồn kho không được âm
            if (newStock < 0)
            {
                TempData["ErrorMessage"] = "Số lượng tồn kho không được nhỏ hơn 0.";
                return RedirectToAction("Index");
            }

            var (success, error) = await _inventoryService.UpdateVariantStockAsync(variantId, newStock);
            if (success)
                TempData["SuccessMessage"] = "Cập nhật số lượng tồn kho thành công!";
            else
                TempData["ErrorMessage"] = error ?? "Cập nhật thất bại.";

            return RedirectToAction("Index");
        }
    }
}
