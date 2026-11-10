using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Admin;

namespace MiaraEyeglasses.Controllers.Admin
{
    // Controller quản lý Banner quảng cáo trang chủ cho Admin
    [Authorize(Roles = "Admin")]
    public class AdminBannerController : Controller
    {
        private readonly IAdminBannerService _bannerService;

        public AdminBannerController(IAdminBannerService bannerService)
        {
            _bannerService = bannerService;
        }

        // [GET] /Admin/Banners: Hiển thị danh sách banner quảng cáo
        [HttpGet]
        [Route("Admin/Banners")]
        [Route("AdminBanner")]
        public async Task<IActionResult> Index()
        {
            var banners = await _bannerService.GetAllBannersAsync();
            return View("~/Views/Admin/Banners.cshtml", banners);
        }

        // [POST] /Admin/Banners/Add: Thêm mới banner trình chiếu (kèm validate)
        [HttpPost]
        [Route("Admin/Banners/Add")]
        [Route("AdminBanner/Add")]
        public async Task<IActionResult> Add(string? title, string? imageUrl, string? linkUrl, int sortOrder)
        {
            // 1. Validate: Tiêu đề banner bắt buộc
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tiêu đề cho banner quảng cáo.";
                return RedirectToAction("Index");
            }

            // 2. Validate: Đường dẫn hình ảnh bắt buộc
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đường dẫn hình ảnh banner.";
                return RedirectToAction("Index");
            }

            if (sortOrder < 0) sortOrder = 0;

            await _bannerService.CreateBannerAsync(title.Trim(), imageUrl.Trim(), linkUrl?.Trim(), sortOrder);
            TempData["SuccessMessage"] = "Đã thêm banner trình chiếu mới thành công!";
            return RedirectToAction("Index");
        }

        // [POST] Bật / Tắt trạng thái hiển thị của banner
        [HttpPost]
        [Route("Admin/Banners/ToggleActive")]
        [Route("AdminBanner/ToggleActive")]
        public async Task<IActionResult> ToggleActive(int bannerId)
        {
            if (bannerId <= 0)
            {
                TempData["ErrorMessage"] = "Mã banner không hợp lệ.";
                return RedirectToAction("Index");
            }

            await _bannerService.ToggleBannerActiveAsync(bannerId);
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái banner.";
            return RedirectToAction("Index");
        }
    }
}
