using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Staff;

namespace MiaraEyeglasses.Services.Staff
{
    // Interface dịch vụ kiểm duyệt đánh giá
    public interface IStaffReviewService
    {
        Task<IEnumerable<Review>> GetReviewsAsync(byte? status = null);
        // Thực thi tác vụ: ModerateReview
        Task<(bool Success, string? ErrorMessage)> ModerateReviewAsync(int reviewId, byte newStatus, int staffId);
    }

    // Service phê duyệt hoặc ẩn nhận xét của khách hàng
    public class StaffReviewService : IStaffReviewService
    {
        private readonly IStaffReviewRepository _reviewRepository;

        // Khởi tạo đối tượng StaffReviewService kèm nạp các phụ thuộc cần thiết
        public StaffReviewService(IStaffReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<IEnumerable<Review>> GetReviewsAsync(byte? status = null)
        {
            return await _reviewRepository.GetReviewsForModerationAsync(status);
        }

        // Thực thi tác vụ: ModerateReview
        public async Task<(bool Success, string? ErrorMessage)> ModerateReviewAsync(int reviewId, byte newStatus, int staffId)
        {
            var success = await _reviewRepository.ModerateReviewAsync(reviewId, newStatus, staffId);
            if (!success) return (false, "Không tìm thấy đánh giá.");

            return (true, null);
        }
    }
}

