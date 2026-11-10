using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Product
{
    public interface IReviewRepository : IBaseRepository<Review>
    {
        Task<IEnumerable<Review>> GetApprovedReviewsByProductIdAsync(int productId);
        Task<IEnumerable<Review>> GetReviewsForModerationAsync(byte? status = null);
    }

    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        public ReviewRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<Review>> GetApprovedReviewsByProductIdAsync(int productId)
        {
            return await _dbSet.Include(r => r.User)
                               .Where(r => r.ProductId == productId && r.Status == 1)
                               .OrderByDescending(r => r.CreatedAt)
                               .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsForModerationAsync(byte? status = null)
        {
            var query = _dbSet.Include(r => r.User)
                              .Include(r => r.Product)
                              .AsQueryable();

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }
    }
}

