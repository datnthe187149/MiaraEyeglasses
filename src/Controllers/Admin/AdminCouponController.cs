using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Admin;

namespace MiaraEyeglasses.Controllers.Admin
{
    // Controller quản lý Mã giảm giá (Coupons) cho Admin
    [Authorize(Roles = "Admin")]
    public class AdminCouponController : Controller
    {
        private readonly IAdminCouponService _couponService;

        public AdminCouponController(IAdminCouponService couponService)
        {
            _couponService = couponService;
        }

        // [GET] /Admin/Coupons: Hiển thị danh sách mã giảm giá
        [HttpGet]
        [Route("Admin/Coupons")]
        [Route("AdminCoupon")]
        public async Task<IActionResult> Index()
        {
            var coupons = await _couponService.GetAllCouponsAsync();
            return View("~/Views/Admin/Coupons.cshtml", coupons);
        }

        // [POST] /Admin/Coupons/Add: Thêm mới mã giảm giá (kèm validate)
        [HttpPost]
        [Route("Admin/Coupons/Add")]
        [Route("AdminCoupon/Add")]
        public async Task<IActionResult> Add(
            string? code, decimal discountAmount, decimal minOrderAmount,
            DateTime startAt, DateTime expireAt, int usageLimit)
        {
            // 1. Validate: Mã voucher bắt buộc
            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập ký hiệu mã giảm giá (ví dụ: MIARA50K).";
                return RedirectToAction("Index");
            }

            // 2. Validate: Mức giảm giá phải lớn hơn 0
            if (discountAmount <= 0)
            {
                TempData["ErrorMessage"] = "Giá trị giảm giá phải lớn hơn 0 ₫.";
                return RedirectToAction("Index");
            }

            // 3. Validate: Ngày hết hạn phải lớn hơn ngày bắt đầu
            if (expireAt <= startAt)
            {
                TempData["ErrorMessage"] = "Ngày hết hạn phải sau ngày bắt đầu áp dụng.";
                return RedirectToAction("Index");
            }

            // 4. Validate: Giới hạn số lượt dùng không được âm
            if (usageLimit < 0) usageLimit = 0;
            if (minOrderAmount < 0) minOrderAmount = 0;

            await _couponService.CreateCouponAsync(code.Trim().ToUpper(), discountAmount, minOrderAmount, startAt, expireAt, usageLimit);
            TempData["SuccessMessage"] = $"Đã tạo mã giảm giá '{code.Trim().ToUpper()}' thành công!";
            return RedirectToAction("Index");
        }

        // [POST] Bật / Tắt trạng thái hoạt động của mã giảm giá
        [HttpPost]
        [Route("Admin/Coupons/ToggleActive")]
        [Route("AdminCoupon/ToggleActive")]
        public async Task<IActionResult> ToggleActive(int couponId)
        {
            if (couponId <= 0)
            {
                TempData["ErrorMessage"] = "Mã giảm giá không hợp lệ.";
                return RedirectToAction("Index");
            }

            await _couponService.ToggleCouponActiveAsync(couponId);
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái mã giảm giá.";
            return RedirectToAction("Index");
        }
    }
}
