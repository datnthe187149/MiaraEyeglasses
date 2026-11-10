using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Admin;

namespace MiaraEyeglasses.Controllers.Admin
{
    // Controller quản lý Người dùng & Phân quyền tài khoản cho Quản trị viên (Admin)
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

        // [GET] /Admin/Users/Detail/{id}: Xem thông tin chi tiết người dùng, lịch sử đơn hàng, địa chỉ và đơn kính
        [HttpGet]
        [Route("Admin/Users/Detail/{id}")]
        [Route("AdminUser/Detail/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Mã tài khoản không hợp lệ.";
                return RedirectToAction("Index");
            }

            var user = await _userService.GetUserDetailsAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản người dùng yêu cầu.";
                return RedirectToAction("Index");
            }

            return View("~/Views/Admin/UserDetail.cshtml", user);
        }

        // [POST] /Admin/Users/Create: Thêm mới tài khoản người dùng với phân quyền linh hoạt
        [HttpPost]
        [Route("Admin/Users/Create")]
        [Route("AdminUser/Create")]
        [Route("Admin/Users/CreateStaff")]
        [Route("AdminUser/CreateStaff")]
        public async Task<IActionResult> Create(
            string? fullName, string? email, string? phone, string? password, int roleId = 1, string? gender = "Nam", string? birthDateStr = null)
        {
            // 1. Validate: Họ tên bắt buộc
            if (string.IsNullOrWhiteSpace(fullName))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập họ và tên người dùng.";
                return RedirectToAction("Index");
            }

            // 2. Validate: Email bắt buộc và đúng định dạng
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
            {
                TempData["ErrorMessage"] = "Địa chỉ email không hợp lệ (cần có ký tự @ và tên miền).";
                return RedirectToAction("Index");
            }

            // 3. Validate: Số điện thoại
            if (string.IsNullOrWhiteSpace(phone))
            {
                phone = "";
            }

            // 4. Validate: Mật khẩu tối thiểu 6 ký tự
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                TempData["ErrorMessage"] = "Mật khẩu khởi tạo phải có độ dài tối thiểu 6 ký tự.";
                return RedirectToAction("Index");
            }

            DateOnly? birthDate = null;
            if (!string.IsNullOrWhiteSpace(birthDateStr))
            {
                if (DateOnly.TryParse(birthDateStr, out var parsedDate))
                {
                    birthDate = parsedDate;
                }
            }

            var (success, error) = await _userService.CreateUserAsync(
                fullName.Trim(), email.Trim(), phone.Trim(), password, roleId, gender, birthDate, true);

            if (success)
            {
                TempData["SuccessMessage"] = $"Đã tạo mới tài khoản '{fullName.Trim()}' thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = error;
                if (string.IsNullOrEmpty(error))
                {
                    TempData["ErrorMessage"] = "Tạo tài khoản người dùng thất bại.";
                }
            }

            return RedirectToAction("Index");
        }

        // [GET] /Admin/Users/Edit/{id}: Hiển thị giao diện Chỉnh sửa tài khoản người dùng
        [HttpGet]
        [Route("Admin/Users/Edit/{id}")]
        [Route("AdminUser/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Mã tài khoản không hợp lệ.";
                return RedirectToAction("Index");
            }

            var user = await _userService.GetUserDetailsAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản người dùng yêu cầu.";
                return RedirectToAction("Index");
            }

            var roles = await _userService.GetAllRolesAsync();
            ViewBag.Roles = roles;

            return View("~/Views/Admin/EditUser.cshtml", user);
        }

        // [POST] /Admin/Users/Edit/{id}: Lưu cập nhật thông tin tài khoản người dùng
        [HttpPost]
        [Route("Admin/Users/Edit/{id}")]
        [Route("AdminUser/Edit/{id}")]
        public async Task<IActionResult> Edit(
            int id, string? fullName, string? phone, int roleId, string? gender, string? birthDateStr, bool isActive, string? newPassword)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Mã tài khoản không hợp lệ.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                TempData["ErrorMessage"] = "Họ và tên người dùng không được để trống.";
                return RedirectToAction("Edit", new { id });
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                phone = "";
            }

            DateOnly? birthDate = null;
            if (!string.IsNullOrWhiteSpace(birthDateStr))
            {
                if (DateOnly.TryParse(birthDateStr, out var parsedDate))
                {
                    birthDate = parsedDate;
                }
            }

            var (success, error) = await _userService.UpdateUserAsync(
                id, fullName.Trim(), phone.Trim(), roleId, gender, birthDate, isActive, newPassword);

            if (success)
            {
                TempData["SuccessMessage"] = $"Đã cập nhật thông tin tài khoản '{fullName.Trim()}' thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = error;
                if (string.IsNullOrEmpty(error))
                {
                    TempData["ErrorMessage"] = "Cập nhật tài khoản thất bại.";
                }
                return RedirectToAction("Edit", new { id });
            }
        }

        // [POST] /Admin/Users/Delete: Xóa tài khoản hoặc tự động khóa an toàn nếu đã có đơn hàng
        [HttpPost]
        [Route("Admin/Users/Delete")]
        [Route("AdminUser/Delete")]
        public async Task<IActionResult> Delete(int userId)
        {
            if (userId <= 0)
            {
                TempData["ErrorMessage"] = "Mã tài khoản không hợp lệ.";
                return RedirectToAction("Index");
            }

            // Kiểm tra không cho phép tự xóa chính tài khoản Admin đang đăng nhập
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(currentUserIdStr, out var currentUserId) && currentUserId == userId)
            {
                TempData["ErrorMessage"] = "Bạn không thể tự xóa chính tài khoản Quản trị viên đang đăng nhập.";
                return RedirectToAction("Index");
            }

            var (success, message) = await _userService.DeleteUserAsync(userId);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction("Index");
        }

        // [POST] Khóa / Mở khóa tài khoản người dùng
        [HttpPost]
        [Route("Admin/Users/ToggleActive")]
        [Route("AdminUser/ToggleActive")]
        [Route("Admin/Users/ToggleUserStatus")]
        [Route("AdminUser/ToggleUserStatus")]
        public async Task<IActionResult> ToggleActive(int userId)
        {
            if (userId <= 0)
            {
                TempData["ErrorMessage"] = "Mã tài khoản không hợp lệ.";
                return RedirectToAction("Index");
            }

            // Kiểm tra không cho phép tự khóa chính tài khoản Admin đang đăng nhập
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(currentUserIdStr, out var currentUserId) && currentUserId == userId)
            {
                TempData["ErrorMessage"] = "Bạn không thể tự khóa chính tài khoản Quản trị viên đang đăng nhập.";
                return RedirectToAction("Index");
            }

            await _userService.ToggleUserActiveAsync(userId);
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái hoạt động của tài khoản.";
            return RedirectToAction("Index");
        }
    }
}
