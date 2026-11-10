using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // =========================================================================================
    // CONTROLLER: Quản lý Sổ địa chỉ giao hàng của Khách hàng (User Address Controller)
    // Phân hệ: Hồ sơ cá nhân & Danh bạ địa chỉ nhận hàng
    // Chức năng:
    //   - Thêm địa chỉ nhận hàng mới kèm họ tên người nhận và số điện thoại.
    //   - Đặt một địa chỉ làm địa chỉ giao hàng mặc định khi đặt đơn kính mắt.
    //   - Xóa địa chỉ không còn sử dụng khỏi sổ địa chỉ cá nhân.
    // =========================================================================================
    [Authorize]
    [CustomerOnly]
    public class AddressController : Controller
    {
        // Khai báo tầng Service xử lý nghiệp vụ sổ địa chỉ
        private readonly IAddressService _addressService;

        /// <summary>
        /// Hàm khởi tạo tiêm phụ thuộc IAddressService (Dependency Injection)
        /// </summary>
        /// <param name="addressService">Dịch vụ quản lý địa chỉ người dùng</param>
        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        /// <summary>
        /// Hàm bổ trợ lấy mã định danh người dùng (UserId) hiện tại đang đăng nhập
        /// </summary>
        /// <returns>Mã ID của người dùng trong hệ thống</returns>
        private int GetCurrentUserId()
        {
            // 1. Kiểm tra UserId đã lưu trong Session khi đăng nhập thành công
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            // 2. Trích xuất UserId từ Claims (NameIdentifier) của Cookie Authentication
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) 
                return userId;

            // 3. Fallback mặc định
            return 1;
        }

        /// <summary>
        /// [POST] Thêm mới địa chỉ nhận hàng vào sổ địa chỉ cá nhân
        /// </summary>
        /// <param name="model">Dữ liệu địa chỉ gồm Người nhận, SĐT, Số nhà, Phường/Xã, Quận/Huyện, Tỉnh/Thành</param>
        /// <returns>Chuyển hướng về tab 'addresses' trong trang Hồ sơ cá nhân</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/AddAddress")]
        [Route("Address/Add")]
        public async Task<IActionResult> Add(AddAddressViewModel model)
        {
            int userId = GetCurrentUserId();

            // 1. Kiểm tra tính hợp lệ của dữ liệu đầu vào (Validation)
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ các trường thông tin địa chỉ giao nhận.";
                return RedirectToAction("Index", "Profile", new { tab = "addresses" });
            }

            // 2. Gọi Service lưu địa chỉ mới vào cơ sở dữ liệu
            await _addressService.AddAddressAsync(userId, model);
            TempData["SuccessMessage"] = "Đã lưu địa chỉ giao hàng mới thành công!";

            // 3. Chuyển hướng về tab Sổ địa chỉ
            return RedirectToAction("Index", "Profile", new { tab = "addresses" });
        }

        /// <summary>
        /// [POST] Xóa một địa chỉ khỏi sổ địa chỉ cá nhân của người dùng
        /// </summary>
        /// <param name="addressId">Mã định danh địa chỉ cần xóa</param>
        /// <returns>Chuyển hướng về tab 'addresses' kèm thông báo kết quả</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/DeleteAddress")]
        [Route("Address/Delete")]
        public async Task<IActionResult> Delete(int addressId)
        {
            int userId = GetCurrentUserId();

            // Gọi Service thực hiện xóa địa chỉ (có kiểm tra quyền sở hữu của user)
            await _addressService.DeleteAddressAsync(userId, addressId);
            TempData["SuccessMessage"] = "Đã xóa địa chỉ nhận hàng khỏi danh bạ.";

            return RedirectToAction("Index", "Profile", new { tab = "addresses" });
        }

        /// <summary>
        /// [POST] Đặt một địa chỉ làm địa chỉ nhận hàng mặc định (Default Address)
        /// </summary>
        /// <param name="addressId">Mã định danh địa chỉ được chọn làm mặc định</param>
        /// <returns>Chuyển hướng về tab 'addresses' kèm thông báo thành công</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/SetDefaultAddress")]
        [Route("Address/SetDefault")]
        public async Task<IActionResult> SetDefault(int addressId)
        {
            int userId = GetCurrentUserId();

            // Gọi Service cập nhật IsDefault = 1 cho địa chỉ này và hủy mặc định các địa chỉ còn lại
            await _addressService.SetDefaultAddressAsync(userId, addressId);
            TempData["SuccessMessage"] = "Đã đổi địa chỉ giao hàng mặc định thành công!";

            return RedirectToAction("Index", "Profile", new { tab = "addresses" });
        }
    }
}
