using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Product;

namespace MiaraEyeglasses.Controllers.Product
{
    // Controller giới thiệu các gói tròng kính cho khách hàng (Tròng chống ánh sáng xanh, đổi màu, siêu mỏng)
    public class LensController : Controller
    {
        private readonly ILensService _lensService;

        public LensController(ILensService lensService)
        {
            _lensService = lensService;
        }

        // [GET] /Shop/Lenses: Bảng thông tin so sánh các gói tròng kính cận, viễn, loạn
        [HttpGet]
        [Route("Lens")]
        [Route("Shop/Lenses")]
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách các tùy chọn tròng kính đang hoạt động
            var lenses = await _lensService.GetActiveLensOptionsAsync();
            return View("~/Views/Shop/Lenses.cshtml", lenses);
        }
    }
}
