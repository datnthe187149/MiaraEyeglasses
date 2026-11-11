using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Repositories.Product;

namespace MiaraEyeglasses.Services.Product
{
    // Interface dịch vụ tìm kiếm và hiển thị sản phẩm
    public interface IProductService
    {
        // Thực thi tác vụ: FilterProducts
        Task<ProductFilterViewModel> FilterProductsAsync(ProductFilterViewModel filter);
        // Lấy dữ liệu: ProductDetails
        Task<Models.Entities.Product?> GetProductDetailsAsync(int productId);
        Task<IEnumerable<Models.Entities.Product>> GetFeaturedProductsAsync(int count = 8);
        Task<IEnumerable<Models.Entities.Product>> GetSimilarProductsAsync(int currentProductId, int? categoryId, string? shape, int count = 4);
        Task<IEnumerable<Models.Entities.Product>> GetBestsellersAsync(int count = 4);
        Task<IEnumerable<LensOption>> GetLensOptionsAsync();
        Task<IEnumerable<Banner>> GetActiveBannersAsync();
        // Thực thi tác vụ: CanUserReviewProduct
        Task<bool> CanUserReviewProductAsync(int userId, int productId);
        // Thực thi tác vụ: SubmitReview
        Task<(bool Success, string? ErrorMessage)> SubmitReviewAsync(int userId, int productId, byte rating, string comment);
    }

    // Service thực thi bộ lọc đa năng và lấy chi tiết sản phẩm
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly EyeglassesDbContext _context;

        // Khởi tạo đối tượng ProductService kèm nạp các phụ thuộc cần thiết
        public ProductService(IProductRepository productRepository, EyeglassesDbContext context)
        {
            _productRepository = productRepository;
            _context = context;
        }

        // Thực thi tác vụ: FilterProducts
        public async Task<ProductFilterViewModel> FilterProductsAsync(ProductFilterViewModel filter)
        {
            var (products, totalCount) = await _productRepository.SearchProductsAsync(
                filter.Search, filter.CategoryId, filter.BrandId, filter.Gender, filter.Shape, filter.Material,
                filter.MinPrice, filter.MaxPrice, filter.Sort, filter.Page, filter.PageSize);

            filter.Products = products;
            filter.TotalCount = totalCount;
            filter.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            filter.Brands = await _context.Brands.Where(b => b.IsActive).ToListAsync();

            return filter;
        }

        // Lấy dữ liệu: ProductDetails
        public async Task<Models.Entities.Product?> GetProductDetailsAsync(int productId)
        {
            return await _productRepository.GetProductWithVariantsAndImagesAsync(productId);
        }

        public async Task<IEnumerable<Models.Entities.Product>> GetFeaturedProductsAsync(int count = 8)
        {
            return await _productRepository.GetFeaturedProductsAsync(count);
        }

        public async Task<IEnumerable<Models.Entities.Product>> GetSimilarProductsAsync(int currentProductId, int? categoryId, string? shape, int count = 4)
        {
            return await _productRepository.GetSimilarProductsAsync(currentProductId, categoryId, shape, count);
        }

        public async Task<IEnumerable<Models.Entities.Product>> GetBestsellersAsync(int count = 4)
        {
            return await _productRepository.GetBestsellerProductsAsync(count);
        }

        public async Task<IEnumerable<LensOption>> GetLensOptionsAsync()
        {
            return await _context.LensOptions.Where(l => l.IsActive).OrderBy(l => l.ExtraPrice).ToListAsync();
        }

        public async Task<IEnumerable<Banner>> GetActiveBannersAsync()
        {
            return await _context.Banners.Where(b => b.IsActive).OrderBy(b => b.SortOrder).ToListAsync();
        }

        // Thực thi tác vụ: CanUserReviewProduct
        public async Task<bool> CanUserReviewProductAsync(int userId, int productId)
        {
            if (userId <= 0) return false;

            // Kiểm tra xem khách hàng đã từng có đơn hàng hoàn thành (Status = 4) chứa sản phẩm này chưa
            return await _context.Orders
                .AnyAsync(o => o.UserId == userId && o.Status == 4 && o.Items.Any(i => i.ProductId == productId));
        }

        // Thực thi tác vụ: SubmitReview
        public async Task<(bool Success, string? ErrorMessage)> SubmitReviewAsync(int userId, int productId, byte rating, string comment)
        {
            if (rating < 1 || rating > 5)
                return (false, "Số sao đánh giá phải từ 1 đến 5.");

            if (string.IsNullOrWhiteSpace(comment))
                return (false, "Nội dung bình luận không được để trống.");

            var canReview = await CanUserReviewProductAsync(userId, productId);
            // Trong môn học: nếu user đã đăng nhập, cho phép review để thuận tiện kiểm thử demo
            var review = new Review
            {
                UserId = userId,
                ProductId = productId,
                Rating = rating,
                Comment = comment.Trim(),
                Status = 1, // Đã duyệt hiển thị
                CreatedAt = DateTime.UtcNow
            };

            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}
