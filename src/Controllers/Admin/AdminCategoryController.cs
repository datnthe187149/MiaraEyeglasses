using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Admin;

namespace MiaraEyeglasses.Controllers.Admin
{
    // Controller quản lý Danh mục gọng kính (Categories) dành cho Quản trị viên (Admin)
    [Authorize(Roles = "Admin")]
    public class AdminCategoryController : Controller
    {
        private readonly IAdminCategoryService _categoryService;

        public AdminCategoryController(IAdminCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // [GET] /Admin/Categories: Hiển thị danh sách danh mục
        [HttpGet]
        [Route("Admin/Categories")]
        [Route("AdminCategory")]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View("~/Views/Admin/Categories.cshtml", categories);
        }

        // [POST] /Admin/Categories/Add: Thêm mới danh mục gọng kính (Có validate dữ liệu)
        [HttpPost]
        [Route("Admin/Categories/Add")]
        [Route("AdminCategory/Add")]
        public async Task<IActionResult> Add(string? categoryName, string? description)
        {
            // 1. Validate: Kiểm tra tên danh mục không được để trống
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tên danh mục gọng kính (không được để trống).";
                return RedirectToAction("Index");
            }

            // 2. Validate: Giới hạn độ dài tên danh mục
            if (categoryName.Trim().Length > 100)
            {
                TempData["ErrorMessage"] = "Tên danh mục không được vượt quá 100 ký tự.";
                return RedirectToAction("Index");
            }

            // 3. Thực hiện thêm mới khi dữ liệu đã hợp lệ
            await _categoryService.CreateCategoryAsync(categoryName.Trim(), description?.Trim());
            TempData["SuccessMessage"] = $"Đã thêm danh mục '{categoryName.Trim()}' thành công!";
            return RedirectToAction("Index");
        }

        // [POST] /Admin/Categories/Update: Cập nhật thông tin danh mục
        [HttpPost]
        [Route("Admin/Categories/Update")]
        [Route("AdminCategory/Update")]
        public async Task<IActionResult> Update(int categoryId, string? categoryName, string? slug)
        {
            if (categoryId <= 0)
            {
                TempData["ErrorMessage"] = "Mã danh mục không hợp lệ.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tên danh mục gọng kính.";
                return RedirectToAction("Index");
            }

            if (categoryName.Trim().Length > 100)
            {
                TempData["ErrorMessage"] = "Tên danh mục không được vượt quá 100 ký tự.";
                return RedirectToAction("Index");
            }

            var result = await _categoryService.UpdateCategoryAsync(categoryId, categoryName.Trim(), slug?.Trim());
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = $"Cập nhật danh mục '{categoryName.Trim()}' thành công!";
            return RedirectToAction("Index");
        }

        // [POST] Kích hoạt hoặc Vô hiệu hóa một danh mục (Ẩn/Hiện trên trang bán hàng)
        [HttpPost]
        [Route("Admin/Categories/ToggleActive")]
        [Route("AdminCategory/ToggleActive")]
        public async Task<IActionResult> ToggleActive(int categoryId)
        {
            // Validate: ID danh mục phải hợp lệ
            if (categoryId <= 0)
            {
                TempData["ErrorMessage"] = "Mã danh mục không hợp lệ.";
                return RedirectToAction("Index");
            }

            await _categoryService.ToggleCategoryActiveAsync(categoryId);
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái danh mục.";
            return RedirectToAction("Index");
        }
    }
}
