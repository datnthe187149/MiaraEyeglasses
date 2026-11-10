using System.Text.Json;
using System.Text.RegularExpressions;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Admin;
using MiaraEyeglasses.Services.Common;
using ProductEntity = MiaraEyeglasses.Models.Entities.Product;

namespace MiaraEyeglasses.Services.Admin
{
    public interface IAdminProductService
    {
        Task<IEnumerable<ProductEntity>> GetAllProductsAsync();
        Task<(IEnumerable<ProductEntity> Products, int TotalCount, int TotalPages)> GetProductsPagedAsync(int page, int pageSize, string? search = null);
        Task<ProductEntity?> GetProductByIdAsync(int productId);
        Task<(bool Success, string? ErrorMessage)> CreateProductAsync(
            string productName, string productCode, int categoryId, int brandId, decimal price,
            string description, string frameShape, string material, string gender,
            string? imageUrl, IFormFile? primaryImageFile, List<IFormFile>? galleryImageFiles,
            string colorName, string sizeLabel, int stockQty);
        Task<(bool Success, string? ErrorMessage)> UpdateProductAsync(
            int productId, string productName, string productCode, int categoryId, int brandId, decimal price,
            string description, string frameShape, string material, string gender, bool isActive,
            IFormFile? primaryImageFile, List<IFormFile>? galleryImageFiles,
            Dictionary<int, int>? variantStocks, Dictionary<int, string>? variantColors);
        Task ToggleProductActiveAsync(int productId);
        Task<bool> DeleteProductImageAsync(int imageId);
        Task<(int ImportedCount, List<string> Messages)> ImportFromScraperFolderAsync(string scraperOutputDir);
    }

    public class AdminProductService : IAdminProductService
    {
        private readonly IAdminProductRepository _productRepository;
        private readonly IFileUploadService _fileUploadService;

        public AdminProductService(
            IAdminProductRepository productRepository,
            IFileUploadService fileUploadService)
        {
            _productRepository = productRepository;
            _fileUploadService = fileUploadService;
        }

        public async Task<IEnumerable<ProductEntity>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllProductsWithDetailsAsync();
        }

        public async Task<ProductEntity?> GetProductByIdAsync(int productId)
        {
            if (productId <= 0)
            {
                return null;
            }
            return await _productRepository.GetProductByIdWithDetailsAsync(productId);
        }

        public async Task<(IEnumerable<ProductEntity> Products, int TotalCount, int TotalPages)> GetProductsPagedAsync(int page, int pageSize, string? search = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (products, totalCount) = await _productRepository.GetProductsPagedAsync(page, pageSize, search);
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            if (totalPages < 1) totalPages = 1;

            return (products, totalCount, totalPages);
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateProductAsync(
            string productName, string productCode, int categoryId, int brandId, decimal price,
            string description, string frameShape, string material, string gender,
            string? imageUrl, IFormFile? primaryImageFile, List<IFormFile>? galleryImageFiles,
            string colorName, string sizeLabel, int stockQty)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return (false, "Tên sản phẩm không được để trống.");

            if (string.IsNullOrWhiteSpace(productCode))
                return (false, "Mã sản phẩm không được để trống.");

            if (await _productRepository.ProductCodeExistsAsync(productCode))
                return (false, $"Mã sản phẩm '{productCode}' đã tồn tại trong hệ thống.");

            // 1. Xử lý lưu ảnh đại diện (Primary Image) với đổi tên chống trùng
            string primaryPath = string.Empty;
            if (primaryImageFile != null && primaryImageFile.Length > 0)
            {
                primaryPath = await _fileUploadService.SaveImageAsync(primaryImageFile);
            }
            else if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                primaryPath = imageUrl.Trim();
            }
            else
            {
                primaryPath = "/images/products/p1.png";
            }

            var product = new ProductEntity
            {
                ProductName = productName.Trim(),
                ProductCode = productCode.Trim(),
                CategoryId = categoryId,
                BrandId = brandId,
                Price = price,
                Description = description,
                FrameShape = frameShape,
                Material = material,
                Gender = gender,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var images = new List<ProductImage>
            {
                new ProductImage
                {
                    ImageUrl = primaryPath,
                    IsPrimary = true,
                    SortOrder = 0
                }
            };

            // 2. Xử lý các ảnh phụ (Gallery Images) nếu người dùng tải lên nhiều ảnh
            if (galleryImageFiles != null && galleryImageFiles.Any())
            {
                int sort = 1;
                foreach (var file in galleryImageFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        var gPath = await _fileUploadService.SaveImageAsync(file);
                        if (!string.IsNullOrEmpty(gPath))
                        {
                            images.Add(new ProductImage
                            {
                                ImageUrl = gPath,
                                IsPrimary = false,
                                SortOrder = sort++
                            });
                        }
                    }
                }
            }

            // 3. Biến thể khởi tạo
            string cleanSize = "M";
            if (!string.IsNullOrWhiteSpace(sizeLabel))
            {
                cleanSize = sizeLabel.Trim();
            }

            int finalStock = 10;
            if (stockQty > 0)
            {
                finalStock = stockQty;
            }

            var variant = new ProductVariant
            {
                Sku = $"{productCode.Trim()}-{colorName.Trim().ToUpper()}",
                ColorName = colorName.Trim(),
                ColorGroup = "Đen",
                SizeLabel = cleanSize,
                StockQty = finalStock,
                IsActive = true
            };

            await _productRepository.AddProductWithVariantsAndImagesAsync(product, new[] { variant }, images);
            return (true, null);
        }

        public async Task ToggleProductActiveAsync(int productId)
        {
            await _productRepository.ToggleProductActiveAsync(productId);
        }

        public async Task<bool> DeleteProductImageAsync(int imageId)
        {
            if (imageId <= 0) return false;
            return await _productRepository.DeleteProductImageAsync(imageId);
        }

        // Cập nhật thông tin chi tiết của sản phẩm, giá bán, tồn kho các biến thể và gallery ảnh
        public async Task<(bool Success, string? ErrorMessage)> UpdateProductAsync(
            int productId, string productName, string productCode, int categoryId, int brandId, decimal price,
            string description, string frameShape, string material, string gender, bool isActive,
            IFormFile? primaryImageFile, List<IFormFile>? galleryImageFiles,
            Dictionary<int, int>? variantStocks, Dictionary<int, string>? variantColors)
        {
            // 1. Kiểm tra tồn tại sản phẩm
            var product = await _productRepository.GetProductByIdWithDetailsAsync(productId);
            if (product == null)
            {
                return (false, "Không tìm thấy sản phẩm cần cập nhật trong hệ thống.");
            }

            // 2. Validate dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(productName))
            {
                return (false, "Tên mẫu kính không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(productCode))
            {
                return (false, "Mã sản phẩm / SKU không được để trống.");
            }

            if (price <= 0)
            {
                return (false, "Giá bán của sản phẩm phải lớn hơn 0 ₫.");
            }

            if (categoryId <= 0)
            {
                return (false, "Vui lòng chọn danh mục gọng kính hợp lệ.");
            }

            if (brandId <= 0)
            {
                return (false, "Vui lòng chọn thương hiệu hợp lệ.");
            }

            // Kiểm tra mã sản phẩm có bị trùng với sản phẩm khác hay không
            bool codeExists = await _productRepository.ProductCodeExistsAsync(productCode.Trim(), productId);
            if (codeExists)
            {
                return (false, $"Mã sản phẩm '{productCode.Trim()}' đã được dùng cho sản phẩm khác.");
            }

            // 3. Gán cập nhật các trường thông tin cơ bản
            product.ProductName = productName.Trim();
            product.ProductCode = productCode.Trim();
            product.CategoryId = categoryId;
            product.BrandId = brandId;
            product.Price = price;
            product.Description = description;
            product.FrameShape = frameShape;
            product.Material = material;
            product.Gender = gender;
            product.IsActive = isActive;

            // 4. Xử lý ảnh đại diện mới nếu có tải lên
            ProductImage? newPrimary = null;
            if (primaryImageFile != null && primaryImageFile.Length > 0)
            {
                string savedPath = await _fileUploadService.SaveImageAsync(primaryImageFile);
                if (!string.IsNullOrEmpty(savedPath))
                {
                    newPrimary = new ProductImage
                    {
                        ImageUrl = savedPath,
                        IsPrimary = true,
                        SortOrder = 0
                    };
                }
            }

            // 5. Xử lý các ảnh phụ mới tải lên
            List<ProductImage>? newGalleries = null;
            if (galleryImageFiles != null && galleryImageFiles.Count > 0)
            {
                newGalleries = new List<ProductImage>();
                int currentSort = 1;
                if (product.Images != null && product.Images.Count > 0)
                {
                    currentSort = product.Images.Count;
                }

                foreach (var file in galleryImageFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        string gPath = await _fileUploadService.SaveImageAsync(file);
                        if (!string.IsNullOrEmpty(gPath))
                        {
                            newGalleries.Add(new ProductImage
                            {
                                ImageUrl = gPath,
                                IsPrimary = false,
                                SortOrder = currentSort
                            });
                            currentSort++;
                        }
                    }
                }
            }

            // 6. Cập nhật số lượng tồn kho của các biến thể trực tiếp trên entity đã tracking
            if (product.Variants != null && product.Variants.Count > 0)
            {
                foreach (var variant in product.Variants)
                {
                    // Cập nhật tồn kho nếu có truyền vào
                    if (variantStocks != null && variantStocks.ContainsKey(variant.VariantId))
                    {
                        int newStock = variantStocks[variant.VariantId];
                        if (newStock < 0)
                        {
                            newStock = 0;
                        }
                        variant.StockQty = newStock;
                    }
                }
            }

            // 7. Gọi Repository lưu vào CSDL
            await _productRepository.UpdateProductWithVariantsAndImagesAsync(
                product, null, newPrimary, newGalleries);

            return (true, null);
        }

        public async Task<(int ImportedCount, List<string> Messages)> ImportFromScraperFolderAsync(string scraperOutputDir)
        {
            var messages = new List<string>();
            int imported = 0;

            if (string.IsNullOrWhiteSpace(scraperOutputDir) || !Directory.Exists(scraperOutputDir))
            {
                messages.Add($"Thư mục scraper không tồn tại: {scraperOutputDir}");
                return (0, messages);
            }

            var productFolderDir = Path.Combine(scraperOutputDir, "by_product_folder");
            if (!Directory.Exists(productFolderDir))
            {
                productFolderDir = scraperOutputDir;
            }

            var subDirs = Directory.GetDirectories(productFolderDir);
            foreach (var dir in subDirs)
            {
                var jsonFile = Path.Combine(dir, "thong-tin-san-pham.json");
                if (!File.Exists(jsonFile)) continue;

                try
                {
                    var jsonContent = await File.ReadAllTextAsync(jsonFile);
                    using var doc = JsonDocument.Parse(jsonContent);
                    var root = doc.RootElement;

                    // Đọc thông tin từ JSON bằng các câu lệnh if rõ ràng, dễ hiểu
                    string brandName = "MIARA";
                    if (root.TryGetProperty("brand", out var bEl) && !string.IsNullOrWhiteSpace(bEl.GetString()))
                    {
                        brandName = bEl.GetString()!;
                    }

                    string name = "";
                    if (root.TryGetProperty("name", out var nEl) && !string.IsNullOrWhiteSpace(nEl.GetString()))
                    {
                        name = nEl.GetString()!;
                    }

                    string code = "";
                    if (root.TryGetProperty("productCode", out var cEl) && !string.IsNullOrWhiteSpace(cEl.GetString()))
                    {
                        code = cEl.GetString()!;
                    }

                    decimal price = 249000;
                    if (root.TryGetProperty("price", out var pEl))
                    {
                        price = pEl.GetDecimal();
                    }

                    string desc = "";
                    if (root.TryGetProperty("description", out var dEl) && !string.IsNullOrWhiteSpace(dEl.GetString()))
                    {
                        desc = dEl.GetString()!;
                    }

                    string material = "Nhựa";
                    if (root.TryGetProperty("material", out var mEl) && !string.IsNullOrWhiteSpace(mEl.GetString()))
                    {
                        material = mEl.GetString()!;
                    }

                    string sizeStr = "M";
                    if (root.TryGetProperty("size", out var sEl) && !string.IsNullOrWhiteSpace(sEl.GetString()))
                    {
                        sizeStr = sEl.GetString()!;
                    }

                    if (string.IsNullOrWhiteSpace(code))
                    {
                        code = Path.GetFileName(dir);
                    }

                    // Nếu mã đã tồn tại -> bỏ qua để không trùng
                    if (await _productRepository.ProductCodeExistsAsync(code))
                    {
                        continue;
                    }

                    // Thương hiệu
                    var brand = await _productRepository.GetOrCreateBrandAsync(brandName);

                    // Phân loại danh mục
                    string catName = "Gọng kính cận";
                    string catSlug = "gong-kinh-can";
                    if (material.Contains("Titan", StringComparison.OrdinalIgnoreCase) || material.Contains("Kim loại", StringComparison.OrdinalIgnoreCase))
                    {
                        catName = "Gọng Titan & Kim loại";
                        catSlug = "gong-titan-kim-loai";
                    }
                    else if (desc.Contains("mát", StringComparison.OrdinalIgnoreCase))
                    {
                        catName = "Kính mát có độ";
                        catSlug = "kinh-mat-co-do";
                    }
                    else if (desc.Contains("ánh sáng xanh", StringComparison.OrdinalIgnoreCase))
                    {
                        catName = "Kính chống ánh sáng xanh";
                        catSlug = "kinh-chong-anh-sang-xanh";
                    }
                    var category = await _productRepository.GetOrCreateCategoryAsync(catName, catSlug);

                    // Dáng kính
                    string shape = "Chữ nhật";
                    if (desc.Contains("tròn", StringComparison.OrdinalIgnoreCase)) shape = "Tròn";
                    else if (desc.Contains("vuông", StringComparison.OrdinalIgnoreCase)) shape = "Vuông";
                    else if (desc.Contains("oval", StringComparison.OrdinalIgnoreCase)) shape = "Oval";
                    else if (desc.Contains("mắt mèo", StringComparison.OrdinalIgnoreCase)) shape = "Mắt mèo";
                    else if (desc.Contains("đa giác", StringComparison.OrdinalIgnoreCase)) shape = "Đa giác";

                    // Quét các file ảnh trong thư mục sản phẩm
                    var imageFiles = Directory.GetFiles(dir)
                        .Where(f => !f.EndsWith("ALL_ANGLES_COMBINED.jpg", StringComparison.OrdinalIgnoreCase)
                                 && (f.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)))
                        .OrderBy(f => Path.GetFileName(f))
                        .ToList();

                    var productImages = new List<ProductImage>();
                    int sortIdx = 0;

                    foreach (var imgFile in imageFiles)
                    {
                        // Copy ảnh vào wwwroot/img với tên duy nhất có chứa timestamp
                        var uploadedRelPath = await _fileUploadService.SaveImageFromPathAsync(imgFile);
                        if (!string.IsNullOrEmpty(uploadedRelPath))
                        {
                            bool isPrimary = (sortIdx == 0);
                            productImages.Add(new ProductImage
                            {
                                ImageUrl = uploadedRelPath,
                                IsPrimary = isPrimary,
                                SortOrder = sortIdx++
                            });
                        }
                    }

                    if (!productImages.Any())
                    {
                        productImages.Add(new ProductImage
                        {
                            ImageUrl = "/images/products/p1.png",
                            IsPrimary = true,
                            SortOrder = 0
                        });
                    }

                    // Tách màu sắc
                    string colorName = "Tiêu chuẩn";
                    if (root.TryGetProperty("colors", out var colEl) && colEl.ValueKind == JsonValueKind.Array && colEl.GetArrayLength() > 0)
                    {
                        colorName = colEl[0].GetString() ?? "Tiêu chuẩn";
                    }

                    // Parse thông số size: e.g. "52 – 17 – 145 mm"
                    short? lensW = 52, bridgeW = 18, templeL = 145;
                    var matchSize = Regex.Match(sizeStr, @"(\d+)\s*[-–]\s*(\d+)\s*[-–]\s*(\d+)");
                    if (matchSize.Success)
                    {
                        if (short.TryParse(matchSize.Groups[1].Value, out var w)) lensW = w;
                        if (short.TryParse(matchSize.Groups[2].Value, out var b)) bridgeW = b;
                        if (short.TryParse(matchSize.Groups[3].Value, out var t)) templeL = t;
                    }

                    // Chuẩn hóa tên chất liệu bằng câu lệnh if-else rõ ràng
                    string cleanMaterial = "Nhựa";
                    if (material.Contains("Titan"))
                    {
                        cleanMaterial = "Titan";
                    }
                    else if (material.Contains("Kim loại"))
                    {
                        cleanMaterial = "Kim loại";
                    }

                    // Chuẩn hóa nhóm màu bằng câu lệnh if-else rõ ràng
                    string cleanColorGroup = "Trong suốt";
                    if (colorName.Contains("Đen"))
                    {
                        cleanColorGroup = "Đen";
                    }
                    else if (colorName.Contains("Nâu"))
                    {
                        cleanColorGroup = "Nâu";
                    }

                    var newProduct = new ProductEntity
                    {
                        ProductName = $"{brandName} {name}".Trim(),
                        ProductCode = code,
                        CategoryId = category.CategoryId,
                        BrandId = brand.BrandId,
                        Price = price,
                        Description = desc,
                        FrameShape = shape,
                        Material = cleanMaterial,
                        Gender = "Unisex",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var newVariant = new ProductVariant
                    {
                        Sku = $"{code.Replace(" ", "-")}-{Regex.Replace(colorName, @"[^a-zA-Z0-9]", "")}".ToUpper(),
                        ColorName = colorName,
                        ColorGroup = cleanColorGroup,
                        SizeLabel = "M",
                        LensWidth = lensW,
                        BridgeWidth = bridgeW,
                        TempleLength = templeL,
                        StockQty = 15,
                        IsActive = true
                    };

                    await _productRepository.AddProductWithVariantsAndImagesAsync(newProduct, new[] { newVariant }, productImages);
                    imported++;
                }
                catch (Exception ex)
                {
                    messages.Add($"Lỗi khi import thư mục {Path.GetFileName(dir)}: {ex.Message}");
                }
            }

            messages.Add($"Đã import thành công {imported} sản phẩm và lưu ảnh vào wwwroot/img.");
            return (imported, messages);
        }
    }
}
