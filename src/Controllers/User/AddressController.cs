using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Filters;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // Controller quản lý Sổ địa chỉ giao hàng của khách hàng
    // Cho phép thêm mới, xóa và đặt địa chỉ nhận hàng mặc định
    [Authorize]
    [CustomerOnly]
    public class AddressController : Controller
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        // Lấy ID người dùng hiện tại đang đăng nhập
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

        // [POST] Thêm địa chỉ nhận hàng mới vào danh bạ
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/AddAddress")]
        [Route("Address/Add")]
        public async Task<IActionResult> Add(AddAddressViewModel model)
        {
            int userId = GetCurrentUserId();

            // Kiểm tra các trường thông tin có hợp lệ không
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ các trường thông tin địa chỉ.";
                return RedirectToAction("Index", "Profile", new { tab = "addresses" });
            }

            // Lưu địa chỉ mới vào database
            await _addressService.AddAddressAsync(userId, model);
            TempData["SuccessMessage"] = "Thêm địa chỉ giao hàng thành công!";

            return RedirectToAction("Index", "Profile", new { tab = "addresses" });
        }

        // [POST] Xóa địa chỉ khỏi sổ địa chỉ cá nhân
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/DeleteAddress")]
        [Route("Address/Delete")]
        public async Task<IActionResult> Delete(int addressId)
        {
            int userId = GetCurrentUserId();

            // Xóa địa chỉ theo ID
            await _addressService.DeleteAddressAsync(userId, addressId);
            TempData["SuccessMessage"] = "Đã xóa địa chỉ.";

            return RedirectToAction("Index", "Profile", new { tab = "addresses" });
        }

        // [POST] Đặt làm địa chỉ giao hàng mặc định
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/SetDefaultAddress")]
        [Route("Address/SetDefault")]
        public async Task<IActionResult> SetDefault(int addressId)
        {
            int userId = GetCurrentUserId();

            // Cập nhật địa chỉ này làm mặc định
            await _addressService.SetDefaultAddressAsync(userId, addressId);
            TempData["SuccessMessage"] = "Đã đổi địa chỉ mặc định.";

            return RedirectToAction("Index", "Profile", new { tab = "addresses" });
        }
    }
}
