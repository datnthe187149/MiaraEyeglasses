using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiaraEyeglasses.Services.Admin;

namespace MiaraEyeglasses.Controllers.Admin
{
    // Controller quản lý Sản phẩm (Products), Biến thể và Gallery ảnh cho Admin
    [Authorize(Roles = "Admin")]
    public class AdminProductController : Controller
    {
        private readonly IAdminProductService _productService;
        private readonly IAdminCategoryService _categoryService;
        private readonly IAdminBrandService _brandService;

        public AdminProductController(
            IAdminProductService productService,
            IAdminCategoryService categoryService,
            IAdminBrandService brandService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
        }

        // [GET] /Admin/Products: Hiển thị danh sách sản phẩm với Tìm kiếm và Phân trang
        [HttpGet]
        [Route("Admin/Products")]
        [Route("AdminProduct")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null)
        {
            // Validate: Đảm bảo số trang và kích thước trang hợp lệ
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (products, totalCount, totalPages) = await _productService.GetProductsPagedAsync(page, pageSize, search);
            var categories = await _categoryService.GetAllCategoriesAsync();
            var brands = await _brandService.GetAllBrandsAsync();

            ViewBag.Categories = categories.Where(c => c.IsActive);
            ViewBag.Brands = brands.Where(b => b.IsActive);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            return View("~/Views/Admin/Products.cshtml", products);
        }

        // [POST] /Admin/Products/Add: Thêm mới sản phẩm kính mắt kèm validate dữ liệu chặt chẽ
        [HttpPost]
        [Route("Admin/Products/Add")]
        [Route("AdminProduct/Add")]
        public async Task<IActionResult> Add(
            string? productName, string? productCode, int categoryId, int brandId, decimal price,
            string? description, string? frameShape, string? material, string? gender, string? specs,
            string? imageUrl, IFormFile? primaryImageFile, List<IFormFile>? galleryImageFiles,
            string? colorName, string? sizeLabel, int stockQty, string? sizesStock)
        {
            // 1. Validate: Tên sản phẩm bắt buộc
            if (string.IsNullOrWhiteSpace(productName))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tên mẫu kính (không được để trống).";
                return RedirectToAction("Index");
            }

            // 2. Validate: Mã sản phẩm (SKU) bắt buộc
            if (string.IsNullOrWhiteSpace(productCode))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập mã sản phẩm / SKU (không được để trống).";
                return RedirectToAction("Index");
            }

            // 3. Validate: Giá bán phải lớn hơn 0
            if (price <= 0)
            {
                TempData["ErrorMessage"] = "Giá bán của sản phẩm phải lớn hơn 0 ₫.";
                return RedirectToAction("Index");
            }

            // 4. Validate: Danh mục và Thương hiệu phải được chọn hợp lệ
            if (categoryId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn danh mục gọng kính hợp lệ.";
                return RedirectToAction("Index");
            }

            if (brandId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn thương hiệu hợp lệ.";
                return RedirectToAction("Index");
            }

            // 5. Xử lý chuẩn hóa thông số đặc tả nếu người dùng nhập gộp: "Vuông · Kim loại · Unisex"
            if (string.IsNullOrWhiteSpace(frameShape) && !string.IsNullOrWhiteSpace(specs))
            {
                var parts = specs.Split('·').Select(s => s.Trim()).ToArray();
                if (parts.Length > 0) frameShape = parts[0];
                if (parts.Length > 1) material = parts[1];
                if (parts.Length > 2) gender = parts[2];
            }

            // Gán giá trị mặc định nếu người dùng để trống
            if (string.IsNullOrWhiteSpace(frameShape))
            {
                frameShape = "Vuông";
            }
            else
            {
                frameShape = frameShape.Trim();
            }

            if (string.IsNullOrWhiteSpace(material))
            {
                material = "Nhựa";
            }
            else if (material.Contains("Titan", StringComparison.OrdinalIgnoreCase))
            {
                material = "Titan";
            }
            else if (material.Contains("Kim loại", StringComparison.OrdinalIgnoreCase))
            {
                material = "Kim loại";
            }
            else
            {
                material = "Nhựa";
            }

            if (string.IsNullOrWhiteSpace(gender))
            {
                gender = "Unisex";
            }
            else
            {
                gender = gender.Trim();
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                description = productName.Trim() + " - Gọng " + material + " cao cấp dáng " + frameShape;
            }
            else
            {
                description = description.Trim();
            }

            // 6. Xử lý kích cỡ và số lượng tồn kho ban đầu
            if (string.IsNullOrWhiteSpace(sizeLabel) && !string.IsNullOrWhiteSpace(sizesStock))
            {
                var match = System.Text.RegularExpressions.Regex.Match(sizesStock, @"([A-Za-z]+)\s*\((\d+)\)");
                if (match.Success)
                {
                    sizeLabel = match.Groups[1].Value;
                    if (int.TryParse(match.Groups[2].Value, out var parsedQty)) stockQty = parsedQty;
                }
            }

            if (string.IsNullOrWhiteSpace(sizeLabel))
            {
                sizeLabel = "M";
            }
            else
            {
                sizeLabel = sizeLabel.Trim();
            }

            if (stockQty < 0)
            {
                stockQty = 0; // Tồn kho không được âm
            }

            if (string.IsNullOrWhiteSpace(colorName))
            {
                colorName = "Tiêu chuẩn";
            }
            else
            {
                colorName = colorName.Trim();
            }

            // 7. Gọi tầng Service tạo sản phẩm và lưu ảnh
            var (success, error) = await _productService.CreateProductAsync(
                productName.Trim(), productCode.Trim(), categoryId, brandId, price,
                description, frameShape, material, gender,
                imageUrl, primaryImageFile, galleryImageFiles,
                colorName, sizeLabel, stockQty);

            if (success)
                TempData["SuccessMessage"] = $"Đã thêm sản phẩm mới '{productName.Trim()}' thành công!";
            else
                TempData["ErrorMessage"] = error ?? "Thêm sản phẩm thất bại.";

            return RedirectToAction("Index");
        }

        // [GET] /Admin/Products/Edit/{id}: Hiển thị giao diện Chỉnh sửa thông tin chi tiết sản phẩm
        [HttpGet]
        [Route("Admin/Products/Edit/{id}")]
        [Route("AdminProduct/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Mã sản phẩm không hợp lệ.";
                return RedirectToAction("Index");
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin sản phẩm yêu cầu.";
                return RedirectToAction("Index");
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            var brands = await _brandService.GetAllBrandsAsync();

            ViewBag.Categories = categories.Where(c => c.IsActive);
            ViewBag.Brands = brands.Where(b => b.IsActive);

            return View("~/Views/Admin/EditProduct.cshtml", product);
        }

        // [POST] /Admin/Products/Edit/{id}: Lưu cập nhật thông tin sản phẩm & tồn kho biến thể
        [HttpPost]
        [Route("Admin/Products/Edit/{id}")]
        [Route("AdminProduct/Edit/{id}")]
        public async Task<IActionResult> Edit(
            int id,
            string? productName,
            string? productCode,
            int categoryId,
            int brandId,
            decimal price,
            string? frameShape,
            string? material,
            string? gender,
            string? description,
            bool isActive,
            IFormFile? primaryImageFile,
            List<IFormFile>? galleryImageFiles,
            Dictionary<int, int>? variantStock,
            Dictionary<int, string>? variantColor)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Mã sản phẩm không hợp lệ.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(productName))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tên mẫu kính (không được để trống).";
                return RedirectToAction("Edit", new { id });
            }

            if (string.IsNullOrWhiteSpace(productCode))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập mã sản phẩm / SKU (không được để trống).";
                return RedirectToAction("Edit", new { id });
            }

            if (price <= 0)
            {
                TempData["ErrorMessage"] = "Giá bán của sản phẩm phải lớn hơn 0 ₫.";
                return RedirectToAction("Edit", new { id });
            }

            if (categoryId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn danh mục gọng kính hợp lệ.";
                return RedirectToAction("Edit", new { id });
            }

            if (brandId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn thương hiệu hợp lệ.";
                return RedirectToAction("Edit", new { id });
            }

            // Chuẩn hóa Dáng gọng theo CHECK constraint: Chữ nhật, Vuông, Tròn, Oval, Mắt mèo, Đa giác
            if (string.IsNullOrWhiteSpace(frameShape))
            {
                frameShape = "Vuông";
            }
            else
            {
                frameShape = frameShape.Trim();
            }

            // Chuẩn hóa Chất liệu theo CHECK constraint: Nhựa, Kim loại, Titan
            if (string.IsNullOrWhiteSpace(material))
            {
                material = "Nhựa";
            }
            else if (material.Contains("Titan", StringComparison.OrdinalIgnoreCase))
            {
                material = "Titan";
            }
            else if (material.Contains("Kim loại", StringComparison.OrdinalIgnoreCase))
            {
                material = "Kim loại";
            }
            else
            {
                material = "Nhựa";
            }

            if (string.IsNullOrWhiteSpace(gender))
            {
                gender = "Unisex";
            }
            else
            {
                gender = gender.Trim();
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                description = productName.Trim() + " - Gọng " + material + " dáng " + frameShape;
            }
            else
            {
                description = description.Trim();
            }

            var (success, error) = await _productService.UpdateProductAsync(
                id, productName.Trim(), productCode.Trim(), categoryId, brandId, price,
                description, frameShape, material, gender, isActive,
                primaryImageFile, galleryImageFiles, variantStock, variantColor);

            if (success)
            {
                TempData["SuccessMessage"] = $"Đã cập nhật thông tin sản phẩm '{productName.Trim()}' thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = error;
                if (string.IsNullOrEmpty(error))
                {
                    TempData["ErrorMessage"] = "Cập nhật sản phẩm thất bại.";
                }
                return RedirectToAction("Edit", new { id });
            }
        }

        // [POST] Bật / Tắt trạng thái hiển thị của sản phẩm (Ẩn / Hiện)
        [HttpPost]
        [Route("Admin/ToggleProductActive")]
        [Route("AdminProduct/ToggleActive")]
        public async Task<IActionResult> ToggleActive(int productId)
        {
            if (productId <= 0)
            {
                TempData["ErrorMessage"] = "Mã sản phẩm không hợp lệ.";
                return RedirectToAction("Index");
            }

            await _productService.ToggleProductActiveAsync(productId);
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái hiển thị của sản phẩm.";
            return RedirectToAction("Index");
        }

        // [POST] /Admin/Products/DeleteImage: Xóa một ảnh phụ của sản phẩm
        [HttpPost]
        [Route("Admin/Products/DeleteImage")]
        [Route("AdminProduct/DeleteImage")]
        public async Task<IActionResult> DeleteImage(int imageId, int productId)
        {
            if (imageId <= 0 || productId <= 0)
            {
                TempData["ErrorMessage"] = "Thông tin ảnh không hợp lệ.";
                return RedirectToAction("Edit", new { id = productId });
            }

            bool deleted = await _productService.DeleteProductImageAsync(imageId);
            if (deleted)
            {
                TempData["SuccessMessage"] = "Đã xóa ảnh phụ thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa ảnh này hoặc ảnh không tồn tại.";
            }

            return RedirectToAction("Edit", new { id = productId });
        }
    }
}
