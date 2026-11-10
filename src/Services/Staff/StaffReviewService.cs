using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Staff;

namespace MiaraEyeglasses.Services.Staff
{
    public interface IStaffReviewService
    {
        Task<IEnumerable<Review>> GetReviewsAsync(byte? status = null);
        Task<(bool Success, string? ErrorMessage)> ModerateReviewAsync(int reviewId, byte newStatus, int staffId);
    }

    public class StaffReviewService : IStaffReviewService
    {
        private readonly IStaffReviewRepository _reviewRepository;

        public StaffReviewService(IStaffReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<IEnumerable<Review>> GetReviewsAsync(byte? status = null)
        {
            return await _reviewRepository.GetReviewsForModerationAsync(status);
        }

        public async Task<(bool Success, string? ErrorMessage)> ModerateReviewAsync(int reviewId, byte newStatus, int staffId)
        {
            var success = await _reviewRepository.ModerateReviewAsync(reviewId, newStatus, staffId);
            if (!success) return (false, "Không tìm thấy đánh giá.");

            return (true, null);
        }
    }
}

