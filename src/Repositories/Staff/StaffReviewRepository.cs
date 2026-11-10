using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Staff
{
    public interface IStaffReviewRepository : IBaseRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsForModerationAsync(byte? status = null);
        Task<bool> ModerateReviewAsync(int reviewId, byte newStatus, int staffId);
    }

    public class StaffReviewRepository : BaseRepository<Review>, IStaffReviewRepository
    {
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

