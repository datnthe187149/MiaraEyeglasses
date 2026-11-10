using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Models.ViewModels;
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

        // [GET] /Admin/Inventory: Hiển thị danh sách tồn kho với Tìm kiếm, Lọc và Phân trang
        [HttpGet]
        [Route("Admin/Inventory")]
        [Route("AdminInventory")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null, string? filter = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (variants, totalCount, totalPages, lowCount, outCount, allCount) = 
                await _inventoryService.GetInventoryPagedAsync(page, pageSize, search, filter);

            var pagination = new PaginationModel
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = totalPages,
                Controller = "AdminInventory",
                Action = "Index"
            };

            if (!string.IsNullOrWhiteSpace(search)) pagination.RouteValues["search"] = search;
            if (!string.IsNullOrWhiteSpace(filter)) pagination.RouteValues["filter"] = filter;

            ViewBag.Pagination = pagination;
            ViewBag.Search = search;
            ViewBag.Filter = filter;
            ViewBag.AllCount = allCount;
            ViewBag.LowCount = lowCount;
            ViewBag.OutCount = outCount;

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
