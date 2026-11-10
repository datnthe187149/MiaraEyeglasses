using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Repositories.Order;
using MiaraEyeglasses.Services.Product;
using MiaraEyeglasses.Services.User;

namespace MiaraEyeglasses.Controllers.Product
{
    // Controller phục vụ khách hàng khám phá sản phẩm:
    // - Xem danh mục kính (Catalog), Tìm kiếm (Search), Lọc đa tiêu chí & Phân trang
    // - Xem chi tiết mẫu kính (Detail), thư viện ảnh đa góc, chọn gói tròng có độ
    // - Kiểm tra áp dụng mã voucher trực tiếp
    public class ShopController : Controller
    {
        private readonly IProductService _productService;
        private readonly IPrescriptionService _prescriptionService;
        private readonly ICouponRepository _couponRepository;
        private readonly EyeglassesDbContext _context;

        // Tiêm các dịch vụ cần thiết qua Constructor
        public ShopController(
            IProductService productService,
            IPrescriptionService prescriptionService,
            ICouponRepository couponRepository,
            EyeglassesDbContext context)
        {
            _productService = productService;
            _prescriptionService = prescriptionService;
            _couponRepository = couponRepository;
            _context = context;
        }

        // Lấy ID người dùng hiện tại từ Session hoặc Cookie Claims (trả về 0 nếu chưa đăng nhập)
        private int GetCurrentUserId()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId.HasValue && sessionUserId.Value > 0)
                return sessionUserId.Value;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out var userId)) return userId;
            return 0;
        }

        // [GET] /Shop: Hiển thị danh mục gọng kính kèm bộ lọc đa tiêu chí và phân trang
        public async Task<IActionResult> Index(ProductFilterViewModel filter)
        {
            // Bắt thêm tham số 'q' nếu người dùng gõ tìm kiếm từ ô Search nhanh trên Header
            if (string.IsNullOrWhiteSpace(filter.Search) && !string.IsNullOrWhiteSpace(HttpContext.Request.Query["q"]))
            {
                filter.Search = HttpContext.Request.Query["q"].ToString().Trim();
            }

            // Gọi tầng Service thực hiện lọc và phân trang
            var model = await _productService.FilterProductsAsync(filter);
            return View("~/Views/Shop/Index.cshtml", model);
        }

        // [GET] /Shop/Detail/{id}: Hiển thị chi tiết một mẫu kính cụ thể
        public async Task<IActionResult> Detail(int id)
        {
            // Bước 1: Nạp thông tin sản phẩm, biến thể màu sắc, kích cỡ và danh sách ảnh các góc chụp
            var product = await _productService.GetProductDetailsAsync(id);
            if (product == null)
                return NotFound();

            int userId = GetCurrentUserId();

            // Bước 2: Nạp các gói tròng kính có độ (Tròng 0 độ, BlueCut chống ánh sáng xanh, Đổi màu khói,...)
            ViewBag.LensOptions = await _productService.GetLensOptionsAsync();
            ViewBag.UserId = userId;
            ViewBag.CanReview = userId > 0; // Khách đã đăng nhập mới được viết đánh giá

            // Bước 3: Nạp danh sách sản phẩm tương tự và bán chạy để gợi ý dưới chân trang
            ViewBag.SimilarProducts = await _productService.GetSimilarProductsAsync(product.ProductId, product.CategoryId, product.FrameShape, 8);
            ViewBag.BestsellerProducts = await _productService.GetBestsellersAsync(8);

            // Bước 4: Nếu khách đã đăng nhập, nạp thêm Sổ hồ sơ độ cận đã lưu để điền nhanh
            if (userId > 0)
            {
                ViewBag.SavedPrescriptions = await _prescriptionService.GetPrescriptionsAsync(userId);
            }
            else
            {
                ViewBag.SavedPrescriptions = Enumerable.Empty<Models.Entities.CustomerPrescription>();
            }

            // Bước 5: Nạp danh sách mã giảm giá (Vouchers) còn hạn để hiển thị gợi ý
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            ViewBag.AvailableCoupons = await _context.Coupons
                .Where(c => c.IsActive && c.ExpireDate >= today)
                .OrderByDescending(c => c.DiscountValue)
                .AsNoTracking()
                .ToListAsync();

            // Trả về Razor View hiển thị chi tiết sản phẩm
            return View("~/Views/Shop/Detail.cshtml", product);
        }

        // [POST] Kiểm tra tính hợp lệ của mã giảm giá (Voucher) và tính số tiền giảm tức thì cho JavaScript
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CheckVoucher([FromForm] string couponCode, [FromForm] decimal totalAmount)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
                return Json(new { success = false, message = "Vui lòng nhập mã voucher giảm giá." });

            // Bước 1: Kiểm tra mã coupon trong CSDL (còn hạn, IsActive, chưa vượt quá số lượt dùng, đạt đơn tối thiểu)
            var coupon = await _couponRepository.GetValidCouponAsync(couponCode.Trim(), totalAmount);
            if (coupon == null)
            {
                return Json(new { success = false, message = "Mã voucher không hợp lệ, đã hết lượt dùng hoặc chưa đạt giá trị đơn hàng tối thiểu." });
            }

            // Bước 2: Tính toán số tiền được giảm theo loại (PERCENT hoặc AMOUNT)
            decimal discount = 0;
            if (coupon.DiscountType == "PERCENT")
            {
                discount = totalAmount * (coupon.DiscountValue / 100m);
                if (coupon.MaxDiscount.HasValue && discount > coupon.MaxDiscount.Value)
                    discount = coupon.MaxDiscount.Value;
            }
            else
            {
                discount = coupon.DiscountValue;
            }

            discount = Math.Min(discount, totalAmount);
            decimal finalPrice = Math.Max(0, totalAmount - discount);

            // Bước 3: Trả về kết quả JSON để trình duyệt cập nhật giao diện mà không cần tải lại trang
            return Json(new
            {
                success = true,
                code = coupon.CouponCode,
                discountAmount = discount,
                finalPrice = finalPrice,
                message = $"Áp dụng thành công mã {coupon.CouponCode}! Giảm {discount:N0} ₫"
            });
        }

        // [POST] Tiếp nhận gửi đánh giá sao và bình luận sản phẩm từ khách hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(SubmitReviewViewModel model)
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để gửi đánh giá sản phẩm.";
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Detail", new { id = model.ProductId }) });
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin đánh giá chưa hợp lệ.";
                return RedirectToAction("Detail", new { id = model.ProductId });
            }

            var (success, error) = await _productService.SubmitReviewAsync(userId, model.ProductId, model.Rating, model.Comment);
            if (success)
            {
                TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi đánh giá cho sản phẩm này!";
            }
            else
            {
                TempData["ErrorMessage"] = error ?? "Gửi đánh giá thất bại.";
            }

            return RedirectToAction("Detail", new { id = model.ProductId });
        }

        // [GET] Hiển thị trang thông tin chi tiết các gói tròng kính có độ
        public async Task<IActionResult> Lenses()
        {
            var lenses = await _productService.GetLensOptionsAsync();
            return View("~/Views/Shop/Lenses.cshtml", lenses);
        }
    }
}
