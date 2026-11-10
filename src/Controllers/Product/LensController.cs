using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Product;

namespace MiaraEyeglasses.Controllers.Product
{
    public class LensController : Controller
    {
        private readonly ILensService _lensService;

        public LensController(ILensService lensService)
        {
            _lensService = lensService;
        }

        [HttpGet]
        [Route("Lens")]
        [Route("Shop/Lenses")]
        public async Task<IActionResult> Index()
        {
            var lenses = await _lensService.GetActiveLensOptionsAsync();
            return View("~/Views/Shop/Lenses.cshtml", lenses);
        }
    }
}

