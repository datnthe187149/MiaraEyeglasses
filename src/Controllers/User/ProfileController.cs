using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // =========================================================================================
    // CONTROLLER: Quản lý Hồ sơ cá nhân của Khách hàng (User Profile Controller)
    // Phân hệ: Tài khoản khách hàng & Trung tâm quản lý thông tin cá nhân
    // Chức năng:
    //   - Hiển thị thông tin hồ sơ lý lịch: Họ tên, Email, SĐT, Giới tính, Ngày sinh.
    //   - Cập nhật thông tin cá nhân của khách hàng.
    //   - Xem thống kê đơn hàng, tổng chi tiêu mua sắm và đơn thuốc đo mắt.
    // =========================================================================================
    [Authorize]
    [CustomerOnly]
    public class ProfileController : Controller
    {
        // Khai báo tầng Service xử lý nghiệp vụ hồ sơ người dùng
        private readonly IProfileService _profileService;

        /// <summary>
        /// Hàm khởi tạo tiêm phụ thuộc IProfileService (Dependency Injection)
        /// </summary>
        /// <param name="profileService">Dịch vụ quản lý hồ sơ người dùng</param>
        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        /// <summary>
        /// Hàm bổ trợ lấy mã định danh người dùng (UserId) hiện tại đang đăng nhập
        /// </summary>
        /// <returns>Mã ID của người dùng trong hệ thống</returns>
        private int GetCurrentUserId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) 
                return userId;

            return 1;
        }

        /// <summary>
        /// [GET] Hiển thị trang trung tâm tài khoản khách hàng gồm các tab: Hồ sơ, Sổ địa chỉ, Đổi mật khẩu
        /// Route hỗ trợ: /User/Account, /User/Profile, /Profile
        /// </summary>
        /// <param name="tab">Tên tab cần mở trực tiếp (profile, addresses, password)</param>
        /// <returns>Razor View kèm dữ liệu hồ sơ cá nhân</returns>
        [HttpGet]
        [Route("User/Account")]
        [Route("User/Profile")]
        [Route("Profile")]
        public async Task<IActionResult> Index(string? tab = null)
        {
            int userId = GetCurrentUserId();

            // Truy vấn thông tin người dùng kèm danh bạ địa chỉ và đơn đo mắt
            var user = await _profileService.GetUserProfileAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Truyền tên tab đang chọn sang View qua ViewBag
            ViewBag.ActiveTab = tab ?? "profile";
            return View("~/Views/User/Profile.cshtml", user);
        }

        /// <summary>
        /// [POST] Cập nhật thông tin lý lịch cá nhân của khách hàng
        /// </summary>
        /// <param name="model">Dữ liệu cập nhật: Họ tên, Số điện thoại, Giới tính, Ngày sinh</param>
        /// <returns>Chuyển hướng về trang Hồ sơ kèm thông báo kết quả</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/UpdateProfile")]
        [Route("Profile/Update")]
        public async Task<IActionResult> Update(UpdateProfileViewModel model)
        {
            int userId = GetCurrentUserId();

            // 1. Kiểm tra tính hợp lệ của dữ liệu form
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu cập nhật hồ sơ không hợp lệ. Vui lòng kiểm tra lại.";
                return RedirectToAction("Index", new { tab = "profile" });
            }

            // 2. Gọi Service thực hiện cập nhật vào cơ sở dữ liệu
            var (success, error) = await _profileService.UpdateProfileAsync(userId, model);
            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "Cập nhật thông tin thất bại.";
                return RedirectToAction("Index", new { tab = "profile" });
            }

            // 3. Đồng bộ lại Họ tên mới vào Session đăng nhập
            HttpContext.Session.SetString("UserName", model.FullName);

            TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
            return RedirectToAction("Index", new { tab = "profile" });
        }
    }
}
