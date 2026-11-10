using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    [Authorize]
    [CustomerOnly]
    public class PrescriptionController : Controller
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        private int GetCurrentUserId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) return userId;
            return 1;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/AddPrescription")]
        [Route("Prescription/Add")]
        public async Task<IActionResult> Add(AddPrescriptionViewModel model)
        {
            int userId = GetCurrentUserId();
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông số độ cận.";
                return RedirectToAction("Index", "Profile", new { tab = "prescriptions" });
            }

            await _prescriptionService.AddPrescriptionAsync(userId, model);
            TempData["SuccessMessage"] = "Lưu hồ sơ độ cận thành công!";
            return RedirectToAction("Index", "Profile", new { tab = "prescriptions" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/DeletePrescription")]
        [Route("Prescription/Delete")]
        public async Task<IActionResult> Delete(int prescriptionId)
        {
            int userId = GetCurrentUserId();
            await _prescriptionService.DeletePrescriptionAsync(userId, prescriptionId);
            TempData["SuccessMessage"] = "Đã xóa hồ sơ độ cận.";
            return RedirectToAction("Index", "Profile", new { tab = "prescriptions" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/SetDefaultPrescription")]
        [Route("Prescription/SetDefault")]
        public async Task<IActionResult> SetDefault(int prescriptionId)
        {
            int userId = GetCurrentUserId();
            await _prescriptionService.SetDefaultPrescriptionAsync(userId, prescriptionId);
            TempData["SuccessMessage"] = "Đã đặt làm hồ sơ độ cận mặc định.";
            return RedirectToAction("Index", "Profile", new { tab = "prescriptions" });
        }
    }
}
