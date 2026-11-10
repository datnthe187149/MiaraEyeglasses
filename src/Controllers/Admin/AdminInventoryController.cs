using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Staff;

namespace MiaraEyeglasses.Controllers.Admin
{
    // Controller quản lý Tồn kho (Inventory) cho Quản trị viên (Admin)
    [Authorize(Roles = "Admin")]
    public class AdminInventoryController : Controller
    {
        private readonly IStaffInventoryService _inventoryService;

        public AdminInventoryController(IStaffInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // [GET] /Admin/Inventory: Hiển thị danh sách tồn kho các biến thể gọng kính
        [HttpGet]
        [Route("Admin/Inventory")]
        [Route("AdminInventory")]
        public async Task<IActionResult> Index()
        {
            var variants = await _inventoryService.GetInventoryVariantsAsync();
            return View("~/Views/Admin/Inventory.cshtml", variants);
        }

        // [POST] Cập nhật số lượng tồn kho (Validate không cho nhập số âm)
        [HttpPost]
        [Route("Admin/Inventory/UpdateStock")]
        [Route("AdminInventory/UpdateStock")]
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
