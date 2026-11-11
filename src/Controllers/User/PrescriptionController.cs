using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // Controller quản lý Sổ đơn đo khúc xạ mắt (Độ cận SPH, Độ loạn CYL, Trục AXIS, Khoảng cách đồng tử PD)
    [Authorize]
    [CustomerOnly]
    public class PrescriptionController : Controller
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        // Lấy ID người dùng hiện tại
        private int GetCurrentUserId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) return userId;
            return 1;
        }

        // [POST] Thêm hồ sơ đơn đo thị lực mới vào tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/AddPrescription")]
        [Route("Prescription/Add")]
        public async Task<IActionResult> Add(AddPrescriptionViewModel model)
        {
            int userId = GetCurrentUserId();

            // Kiểm tra các thông số độ cận, độ loạn có đúng định dạng không
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông số độ cận.";
                return RedirectToAction("Index", "Profile", new { tab = "prescriptions" });
            }

            // Lưu hồ sơ độ cận vào database
            await _prescriptionService.AddPrescriptionAsync(userId, model);
            TempData["SuccessMessage"] = "Lưu hồ sơ độ cận thành công!";
            return RedirectToAction("Index", "Profile", new { tab = "prescriptions" });
        }

        // [POST] Xóa hồ sơ độ cận khỏi tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/DeletePrescription")]
        [Route("Prescription/Delete")]
        public async Task<IActionResult> Delete(int prescriptionId)
        {
            int userId = GetCurrentUserId();

            // Xóa hồ sơ đo mắt theo ID
            await _prescriptionService.DeletePrescriptionAsync(userId, prescriptionId);
            TempData["SuccessMessage"] = "Đã xóa hồ sơ độ cận.";
            return RedirectToAction("Index", "Profile", new { tab = "prescriptions" });
        }

        // [POST] Đặt làm hồ sơ độ cận mặc định khi cắt kính
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/SetDefaultPrescription")]
        [Route("Prescription/SetDefault")]
        public async Task<IActionResult> SetDefault(int prescriptionId)
        {
            int userId = GetCurrentUserId();

            // Cập nhật hồ sơ này thành mặc định
            await _prescriptionService.SetDefaultPrescriptionAsync(userId, prescriptionId);
            TempData["SuccessMessage"] = "Đã đặt làm hồ sơ độ cận mặc định.";
            return RedirectToAction("Index", "Profile", new { tab = "prescriptions" });
        }
    }
}
