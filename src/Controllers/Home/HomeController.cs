using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Product;

namespace MiaraEyeglasses.Controllers.Home
{
    // Controller Trang chủ và các trang thông tin chính sách của hệ thống MIARA
    public class HomeController : Controller
    {
        private readonly IProductService _productService;

        public HomeController(IProductService productService)
        {
            _productService = productService;
        }

        // [GET] / : Hiển thị trang chủ với danh sách 8 mẫu kính mắt nổi bật nhất
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách sản phẩm nổi bật
            var featuredProducts = await _productService.GetFeaturedProductsAsync(8);
            return View("~/Views/Home/Index.cshtml", featuredProducts);
        }

        // [GET] /Home/Privacy: Trang chính sách bảo mật thông tin
        public IActionResult Privacy()
        {
            return View("~/Views/Home/Privacy.cshtml");
        }

        // [GET] /Home/FAQ: Trang câu hỏi thường gặp khi mua kính mắt
        public IActionResult FAQ()
        {
            return View("~/Views/Home/FAQ.cshtml");
        }

        // [GET] /Home/Shipping: Trang chính sách vận chuyển và đổi trả kính
        public IActionResult Shipping()
        {
            return View("~/Views/Home/Shipping.cshtml");
        }
    }
}
