using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Order
{
    // Interface kho dữ liệu kiểm tra mã giảm giá
    public interface ICouponRepository : IBaseRepository<Coupon>
    {
        // Lấy dữ liệu: ValidCoupon
        Task<Coupon?> GetValidCouponAsync(string code, decimal subtotal);
    }

    // Lớp thực thi xác thực mã giảm giá hợp lệ
    public class CouponRepository : BaseRepository<Coupon>, ICouponRepository
    {
        // Khởi tạo đối tượng CouponRepository kèm nạp các phụ thuộc cần thiết
        public CouponRepository(EyeglassesDbContext context) : base(context) { }

        // Lấy dữ liệu: ValidCoupon
        public async Task<Coupon?> GetValidCouponAsync(string code, decimal subtotal)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return await _dbSet.FirstOrDefaultAsync(c =>
                c.CouponCode == code && c.IsActive &&
                (c.StartDate == null || c.StartDate <= today) &&
                c.ExpireDate >= today &&
                c.MinOrderAmount <= subtotal &&
                (c.UsageLimit == null || c.UsedCount < c.UsageLimit));
        }
    }
}

