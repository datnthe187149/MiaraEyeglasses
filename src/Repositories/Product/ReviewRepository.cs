using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Product
{
    // Interface kho dữ liệu đánh giá sản phẩm
    public interface IReviewRepository : IBaseRepository<Review>
    {
        Task<IEnumerable<Review>> GetApprovedReviewsByProductIdAsync(int productId);
        Task<IEnumerable<Review>> GetReviewsForModerationAsync(byte? status = null);
    }

    // Lớp thực thi lưu trữ và lấy danh sách đánh giá đã duyệt
    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        // Khởi tạo đối tượng ReviewRepository kèm nạp các phụ thuộc cần thiết
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

