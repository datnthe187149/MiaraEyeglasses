using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Product;

namespace MiaraEyeglasses.Controllers.Home
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;

        public HomeController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Banners = await _productService.GetActiveBannersAsync();
            var featuredProducts = await _productService.GetFeaturedProductsAsync(8);
            return View("~/Views/Home/Index.cshtml", featuredProducts);
        }

        public IActionResult Privacy()
        {
            return View("~/Views/Home/Privacy.cshtml");
        }

        public IActionResult FAQ()
        {
            return View("~/Views/Home/FAQ.cshtml");
        }

        public IActionResult Shipping()
        {
            return View("~/Views/Home/Shipping.cshtml");
        }
    }
}

