using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Admin;

namespace MiaraEyeglasses.Controllers.Admin
{
    // Controller quản lý Người dùng & Tài khoản nhân viên cho Admin
    [Authorize(Roles = "Admin")]
    public class AdminUserController : Controller
    {
        private readonly IAdminUserService _userService;

        public AdminUserController(IAdminUserService userService)
        {
            _userService = userService;
        }

        // [GET] /Admin/Users: Xem danh sách tài khoản theo vai trò hoặc tìm kiếm
        [HttpGet]
        [Route("Admin/Users")]
        [Route("AdminUser")]
        public async Task<IActionResult> Index(int? roleId = null, string? search = null)
        {
            var users = await _userService.GetUsersAsync(roleId, search);
            var roles = await _userService.GetAllRolesAsync();

            ViewBag.Roles = roles;
            ViewBag.SelectedRoleId = roleId;
            ViewBag.SearchTerm = search;

            return View("~/Views/Admin/Users.cshtml", users);
        }

        // [POST] /Admin/Users/CreateStaff: Tạo tài khoản kỹ thuật viên/nhân viên mới (kèm validate)
        [HttpPost]
        [Route("Admin/Users/CreateStaff")]
        [Route("AdminUser/CreateStaff")]
        public async Task<IActionResult> CreateStaff(string? fullName, string? email, string? phone, string? password)
        {
            // 1. Validate: Họ tên bắt buộc
            if (string.IsNullOrWhiteSpace(fullName))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập họ và tên nhân viên.";
                return RedirectToAction("Index");
            }

            // 2. Validate: Email bắt buộc và đúng định dạng
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
            {
                TempData["ErrorMessage"] = "Địa chỉ email không hợp lệ (cần có ký tự @ và dấu chấm).";
                return RedirectToAction("Index");
            }

            // 3. Validate: Mật khẩu tối thiểu 6 ký tự
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                TempData["ErrorMessage"] = "Mật khẩu phải có độ dài tối thiểu 6 ký tự.";
                return RedirectToAction("Index");
            }

            var (success, error) = await _userService.CreateStaffUserAsync(fullName.Trim(), email.Trim(), phone?.Trim() ?? "", password);
            if (success)
                TempData["SuccessMessage"] = $"Đã tạo tài khoản nhân viên cho '{fullName.Trim()}' thành công!";
            else
                TempData["ErrorMessage"] = error ?? "Tạo nhân viên thất bại.";

            return RedirectToAction("Index");
        }

        // [POST] Khóa / Mở khóa tài khoản người dùng
        [HttpPost]
        [Route("Admin/Users/ToggleActive")]
        [Route("AdminUser/ToggleActive")]
        public async Task<IActionResult> ToggleActive(int userId)
        {
            if (userId <= 0)
            {
                TempData["ErrorMessage"] = "Mã tài khoản không hợp lệ.";
                return RedirectToAction("Index");
            }

            await _userService.ToggleUserActiveAsync(userId);
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái tài khoản.";
            return RedirectToAction("Index");
        }
    }
}
