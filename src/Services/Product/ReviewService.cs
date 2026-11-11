using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Product;

namespace MiaraEyeglasses.Services.Product
{
    // Interface dịch vụ đánh giá sản phẩm
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetApprovedReviewsByProductAsync(int productId);
        // Thực thi tác vụ: CanUserReviewProduct
        Task<bool> CanUserReviewProductAsync(int userId, int productId);
        // Thực thi tác vụ: SubmitReview
        Task<(bool Success, string? ErrorMessage)> SubmitReviewAsync(int userId, int productId, byte rating, string comment);
    }

    // Service tiếp nhận và hiển thị đánh giá của khách hàng
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly EyeglassesDbContext _context;

        // Khởi tạo đối tượng ReviewService kèm nạp các phụ thuộc cần thiết
        public ReviewService(IReviewRepository reviewRepository, EyeglassesDbContext context)
        {
            _reviewRepository = reviewRepository;
            _context = context;
        }

        public async Task<IEnumerable<Review>> GetApprovedReviewsByProductAsync(int productId)
        {
            return await _reviewRepository.GetApprovedReviewsByProductIdAsync(productId);
        }

        // Thực thi tác vụ: CanUserReviewProduct
        public async Task<bool> CanUserReviewProductAsync(int userId, int productId)
        {
            if (userId <= 0) return false;
            return await _context.Orders.AnyAsync(o => o.UserId == userId && o.Status == 4 && o.Items.Any(i => i.ProductId == productId));
        }

        // Thực thi tác vụ: SubmitReview
        public async Task<(bool Success, string? ErrorMessage)> SubmitReviewAsync(int userId, int productId, byte rating, string comment)
        {
            if (rating < 1 || rating > 5)
                return (false, "Số sao đánh giá phải từ 1 đến 5.");

            if (string.IsNullOrWhiteSpace(comment))
                return (false, "Nội dung bình luận không được để trống.");

            var review = new Review
            {
                UserId = userId,
                ProductId = productId,
                Rating = rating,
                Comment = comment.Trim(),
                Status = 1,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);
            await _reviewRepository.SaveChangesAsync();
            return (true, null);
        }
    }
}

