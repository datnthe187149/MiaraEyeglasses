using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Admin;

namespace MiaraEyeglasses.Controllers.Admin
{
    // Controller quản lý Thương hiệu gọng kính (Brands) dành cho Quản trị viên (Admin)
    [Authorize(Roles = "Admin")]
    public class AdminBrandController : Controller
    {
        private readonly IAdminBrandService _brandService;

        public AdminBrandController(IAdminBrandService brandService)
        {
            _brandService = brandService;
        }

        // [GET] /Admin/Brands: Hiển thị danh sách tất cả các thương hiệu gọng kính
        [HttpGet]
        [Route("Admin/Brands")]
        [Route("AdminBrand")]
        public async Task<IActionResult> Index()
        {
            var brands = await _brandService.GetAllBrandsAsync();
            return View("~/Views/Admin/Brands.cshtml", brands);
        }

        // [POST] /Admin/Brands/Add: Thêm mới thương hiệu (Có validate dữ liệu đầu vào)
        [HttpPost]
        [Route("Admin/Brands/Add")]
        [Route("AdminBrand/Add")]
        public async Task<IActionResult> Add(string? brandName, string? originCountry, string? description)
        {
            // 1. Validate: Kiểm tra tên thương hiệu không được để trống
            if (string.IsNullOrWhiteSpace(brandName))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tên thương hiệu (không được để trống).";
                return RedirectToAction("Index");
            }

            // 2. Validate: Giới hạn độ dài tối đa tránh lỗi CSDL
            if (brandName.Trim().Length > 100)
            {
                TempData["ErrorMessage"] = "Tên thương hiệu không được vượt quá 100 ký tự.";
                return RedirectToAction("Index");
            }

            // 3. Thực hiện thêm mới khi dữ liệu đã hợp lệ
            await _brandService.CreateBrandAsync(brandName.Trim(), originCountry?.Trim(), description?.Trim());
            TempData["SuccessMessage"] = $"Đã thêm thương hiệu '{brandName.Trim()}' thành công!";
            return RedirectToAction("Index");
        }

        // [POST] /Admin/Brands/Update: Cập nhật thông tin thương hiệu
        [HttpPost]
        [Route("Admin/Brands/Update")]
        [Route("AdminBrand/Update")]
        public async Task<IActionResult> Update(int brandId, string? brandName, string? originCountry)
        {
            if (brandId <= 0)
            {
                TempData["ErrorMessage"] = "Mã thương hiệu không hợp lệ.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(brandName))
            {
                TempData["ErrorMessage"] = "Tên thương hiệu không được để trống.";
                return RedirectToAction("Index");
            }

            var (success, error) = await _brandService.UpdateBrandAsync(brandId, brandName.Trim(), originCountry?.Trim());
            if (success)
            {
                TempData["SuccessMessage"] = $"Đã cập nhật thông tin thương hiệu '{brandName.Trim()}' thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = error;
                if (string.IsNullOrEmpty(error))
                {
                    TempData["ErrorMessage"] = "Cập nhật thương hiệu thất bại.";
                }
            }

            return RedirectToAction("Index");
        }

        // [POST] Bật / Tắt trạng thái hoạt động của thương hiệu (Ẩn / Hiện)
        [HttpPost]
        [Route("Admin/Brands/ToggleActive")]
        [Route("AdminBrand/ToggleActive")]
        public async Task<IActionResult> ToggleActive(int brandId)
        {
            // Validate: ID thương hiệu phải hợp lệ
            if (brandId <= 0)
            {
                TempData["ErrorMessage"] = "Mã thương hiệu không hợp lệ.";
                return RedirectToAction("Index");
            }

            await _brandService.ToggleBrandActiveAsync(brandId);
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái thương hiệu.";
            return RedirectToAction("Index");
        }
    }
}
