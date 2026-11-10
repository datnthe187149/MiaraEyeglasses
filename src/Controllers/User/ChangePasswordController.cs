using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // =========================================================================================
    // CONTROLLER: Quản lý Đổi mật khẩu tài khoản người dùng (Change Password Controller)
    // Phân hệ: Bảo mật tài khoản & Xác thực người dùng
    // Chức năng:
    //   - Xác thực mật khẩu hiện tại bằng giải thuật mã hóa băm BCrypt.
    //   - Kiểm tra ràng buộc độ phức tạp của mật khẩu mới (tối thiểu 6 ký tự).
    //   - Cập nhật mật khẩu mới và băm an toàn vào cơ sở dữ liệu.
    // =========================================================================================
    [Authorize]
    [CustomerOnly]
    public class ChangePasswordController : Controller
    {
        // Khai báo tầng Service xử lý nghiệp vụ đổi mật khẩu
        private readonly IChangePasswordService _changePasswordService;

        /// <summary>
        /// Hàm khởi tạo tiêm phụ thuộc IChangePasswordService (Dependency Injection)
        /// </summary>
        /// <param name="changePasswordService">Dịch vụ đổi mật khẩu tài khoản</param>
        public ChangePasswordController(IChangePasswordService changePasswordService)
        {
            _changePasswordService = changePasswordService;
        }

        /// <summary>
        /// [POST] Tiếp nhận yêu cầu đổi mật khẩu từ form thông tin tài khoản
        /// </summary>
        /// <param name="model">Dữ liệu đổi mật khẩu: Mật khẩu cũ, Mật khẩu mới, Xác nhận mật khẩu mới</param>
        /// <returns>Chuyển hướng về trang Hồ sơ cá nhân kèm thông báo thành công hoặc lỗi</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ChangePassword")]
        [Route("User/ChangePassword")]
        public async Task<IActionResult> Index(ChangePasswordViewModel model)
        {
            // Bước 1: Kiểm tra UserId từ Session để đảm bảo người dùng đang đăng nhập
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            // Bước 2: Kiểm tra dữ liệu đầu vào theo DataAnnotations (Validation)
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu đổi mật khẩu không hợp lệ. Vui lòng kiểm tra lại.";
                return RedirectToAction("Index", "Profile", new { tab = "password" });
            }

            // Bước 3: Gọi tầng Service xác thực mật khẩu cũ và lưu mật khẩu mới đã băm BCrypt
            var (success, error) = await _changePasswordService.ChangePasswordAsync(userId.Value, model);
            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "Đổi mật khẩu thất bại.";
                return RedirectToAction("Index", "Profile", new { tab = "password" });
            }

            // Bước 4: Thông báo thành công và chuyển về trang cá nhân
            TempData["SuccessMessage"] = "Đổi mật khẩu tài khoản thành công! Vui lòng sử dụng mật khẩu mới cho lần đăng nhập sau.";
            return RedirectToAction("Index", "Profile", new { tab = "password" });
        }
    }
}
