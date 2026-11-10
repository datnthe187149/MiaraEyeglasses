using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Admin;

namespace MiaraEyeglasses.Controllers.Admin
{
    // Controller quản lý Tùy chọn tròng kính (LensOptions) cho Admin
    [Authorize(Roles = "Admin")]
    public class AdminLensController : Controller
    {
        private readonly IAdminLensService _lensService;

        public AdminLensController(IAdminLensService lensService)
        {
            _lensService = lensService;
        }

        // [GET] /Admin/Lenses: Hiển thị danh sách các gói tròng kính
        [HttpGet]
        [Route("Admin/Lenses")]
        [Route("AdminLens")]
        public async Task<IActionResult> Index()
        {
            var lenses = await _lensService.GetAllLensesAsync();
            return View("~/Views/Admin/Lenses.cshtml", lenses);
        }

        // [POST] /Admin/Lenses/Add: Thêm mới gói tròng kính (kèm validate)
        [HttpPost]
        [Route("Admin/Lenses/Add")]
        [Route("AdminLens/Add")]
        public async Task<IActionResult> Add(string? lensName, decimal extraPrice, string? description, string? rxLimit)
        {
            // 1. Validate: Tên gói tròng bắt buộc
            if (string.IsNullOrWhiteSpace(lensName))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tên gói tròng kính (ví dụ: Tròng BlueCut).";
                return RedirectToAction("Index");
            }

            // 2. Validate: Phụ thu không được âm
            if (extraPrice < 0)
            {
                TempData["ErrorMessage"] = "Giá phụ thu tròng kính không được nhỏ hơn 0 ₫.";
                return RedirectToAction("Index");
            }

            await _lensService.CreateLensAsync(lensName.Trim(), extraPrice, description?.Trim(), rxLimit?.Trim());
            TempData["SuccessMessage"] = $"Đã thêm tùy chọn tròng kính '{lensName.Trim()}' thành công!";
            return RedirectToAction("Index");
        }

        // [POST] Bật / Tắt trạng thái tròng kính
        [HttpPost]
        [Route("Admin/Lenses/ToggleActive")]
        [Route("AdminLens/ToggleActive")]
        public async Task<IActionResult> ToggleActive(int lensOptionId)
        {
            if (lensOptionId <= 0)
            {
                TempData["ErrorMessage"] = "Mã gói tròng không hợp lệ.";
                return RedirectToAction("Index");
            }

            await _lensService.ToggleLensActiveAsync(lensOptionId);
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái tròng kính.";
            return RedirectToAction("Index");
        }
    }
}
