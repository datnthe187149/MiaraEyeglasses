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
    public class AddressController : Controller
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
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
        [Route("User/AddAddress")]
        [Route("Address/Add")]
        public async Task<IActionResult> Add(AddAddressViewModel model)
        {
            int userId = GetCurrentUserId();
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ các trường thông tin địa chỉ.";
                return RedirectToAction("Index", "Profile", new { tab = "addresses" });
            }

            await _addressService.AddAddressAsync(userId, model);
            TempData["SuccessMessage"] = "Thêm địa chỉ giao hàng thành công!";
            return RedirectToAction("Index", "Profile", new { tab = "addresses" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/DeleteAddress")]
        [Route("Address/Delete")]
        public async Task<IActionResult> Delete(int addressId)
        {
            int userId = GetCurrentUserId();
            await _addressService.DeleteAddressAsync(userId, addressId);
            TempData["SuccessMessage"] = "Đã xóa địa chỉ.";
            return RedirectToAction("Index", "Profile", new { tab = "addresses" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("User/SetDefaultAddress")]
        [Route("Address/SetDefault")]
        public async Task<IActionResult> SetDefault(int addressId)
        {
            int userId = GetCurrentUserId();
            await _addressService.SetDefaultAddressAsync(userId, addressId);
            TempData["SuccessMessage"] = "Đã đổi địa chỉ mặc định.";
            return RedirectToAction("Index", "Profile", new { tab = "addresses" });
        }
    }
}
