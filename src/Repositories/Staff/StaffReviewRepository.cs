using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Staff
{
    // Interface kho dữ liệu kiểm duyệt đánh giá
    public interface IStaffReviewRepository : IBaseRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsForModerationAsync(byte? status = null);
        // Thực thi tác vụ: ModerateReview
        Task<bool> ModerateReviewAsync(int reviewId, byte newStatus, int staffId);
    }

    // Lớp thực thi lấy danh sách đánh giá cần duyệt
    public class StaffReviewRepository : BaseRepository<Review>, IStaffReviewRepository
    {
        // Khởi tạo đối tượng StaffReviewRepository kèm nạp các phụ thuộc cần thiết
        public StaffReviewRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<Review>> GetReviewsForModerationAsync(byte? status = null)
        {
            var query = _dbSet
                .Include(r => r.Product)
                .Include(r => r.User)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        // Thực thi tác vụ: ModerateReview
        public async Task<bool> ModerateReviewAsync(int reviewId, byte newStatus, int staffId)
        {
            var review = await _dbSet.FindAsync(reviewId);
            if (review == null) return false;

            review.Status = newStatus;
            review.ModeratedBy = staffId;
            review.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

