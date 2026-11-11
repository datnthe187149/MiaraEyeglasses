using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // Controller quản lý Sổ độ cận (Prescription) qua Session (chuẩn CatBaBooking)
    public class PrescriptionController : Controller
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        private int? GetCurrentUserId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (int.TryParse(userIdStr, out var id))
                return id;

            return null;
        }

        // [POST] Thêm hồ sơ độ cận mới
        [HttpPost]
        [Route("User/AddPrescription")]
        [Route("Prescription/Add")]
        public async Task<IActionResult> Add(AddPrescriptionViewModel model)
        {
            int? userId = GetCurrentUserId();
            if (!userId.HasValue) return Redirect("/login");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu độ cận nhập vào chưa hợp lệ.";
                return RedirectToAction("Index", "Profile", new { tab = "prescriptions" });
            }

            await _prescriptionService.AddPrescriptionAsync(userId.Value, model);
            TempData["SuccessMessage"] = "Lưu thông tin độ cận thành công!";
            return RedirectToAction("Index", "Profile", new { tab = "prescriptions" });
        }

        // [POST] Xóa hồ sơ độ cận
        [HttpPost]
        [Route("User/DeletePrescription")]
        [Route("Prescription/Delete")]
        public async Task<IActionResult> Delete(int prescriptionId)
        {
            int? userId = GetCurrentUserId();
            if (!userId.HasValue) return Redirect("/login");

            var success = await _prescriptionService.DeletePrescriptionAsync(userId.Value, prescriptionId);
            if (!success)
            {
                TempData["ErrorMessage"] = "Không thể xóa hồ sơ độ cận này.";
                return RedirectToAction("Index", "Profile", new { tab = "prescriptions" });
            }

            TempData["SuccessMessage"] = "Đã xóa hồ sơ độ cận thành công!";
            return RedirectToAction("Index", "Profile", new { tab = "prescriptions" });
        }

        // [POST] Đặt làm độ cận mặc định
        [HttpPost]
        [Route("User/SetDefaultPrescription")]
        [Route("Prescription/SetDefault")]
        public async Task<IActionResult> SetDefault(int prescriptionId)
        {
            int? userId = GetCurrentUserId();
            if (!userId.HasValue) return Redirect("/login");

            var success = await _prescriptionService.SetDefaultPrescriptionAsync(userId.Value, prescriptionId);
            if (!success)
            {
                TempData["ErrorMessage"] = "Không thể cập nhật hồ sơ mặc định.";
                return RedirectToAction("Index", "Profile", new { tab = "prescriptions" });
            }

            TempData["SuccessMessage"] = "Đã chọn làm hồ sơ độ cận mặc định!";
            return RedirectToAction("Index", "Profile", new { tab = "prescriptions" });
        }
    }
}
