using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.User
{
    // Controller quản lý Sổ địa chỉ giao hàng qua Session (chuẩn CatBaBooking)
    public class AddressController : Controller
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
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

        // [POST] Thêm địa chỉ nhận hàng mới vào danh bạ
        [HttpPost]
        [Route("User/AddAddress")]
        [Route("Address/Add")]
        public async Task<IActionResult> Add(AddAddressViewModel model)
        {
            int? userId = GetCurrentUserId();
            if (!userId.HasValue) return Redirect("/login");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ các trường thông tin địa chỉ.";
                return RedirectToAction("Index", "Profile", new { tab = "addresses" });
            }

            await _addressService.AddAddressAsync(userId.Value, model);
            TempData["SuccessMessage"] = "Thêm địa chỉ giao hàng thành công!";
            return RedirectToAction("Index", "Profile", new { tab = "addresses" });
        }

        // [POST] Xóa địa chỉ khỏi sổ địa chỉ cá nhân
        [HttpPost]
        [Route("User/DeleteAddress")]
        [Route("Address/Delete")]
        public async Task<IActionResult> Delete(int addressId)
        {
            int? userId = GetCurrentUserId();
            if (!userId.HasValue) return Redirect("/login");

            var success = await _addressService.DeleteAddressAsync(userId.Value, addressId);
            if (!success)
            {
                TempData["ErrorMessage"] = "Không thể xóa địa chỉ này.";
                return RedirectToAction("Index", "Profile", new { tab = "addresses" });
            }

            TempData["SuccessMessage"] = "Đã xóa địa chỉ thành công!";
            return RedirectToAction("Index", "Profile", new { tab = "addresses" });
        }

        // [POST] Thiết lập địa chỉ nhận hàng mặc định
        [HttpPost]
        [Route("User/SetDefaultAddress")]
        [Route("Address/SetDefault")]
        public async Task<IActionResult> SetDefault(int addressId)
        {
            int? userId = GetCurrentUserId();
            if (!userId.HasValue) return Redirect("/login");

            var success = await _addressService.SetDefaultAddressAsync(userId.Value, addressId);
            if (!success)
            {
                TempData["ErrorMessage"] = "Không thể cập nhật địa chỉ mặc định.";
                return RedirectToAction("Index", "Profile", new { tab = "addresses" });
            }

            TempData["SuccessMessage"] = "Đã chọn làm địa chỉ nhận hàng mặc định!";
            return RedirectToAction("Index", "Profile", new { tab = "addresses" });
        }
    }
}
