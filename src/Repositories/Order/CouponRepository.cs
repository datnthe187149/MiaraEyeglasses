using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Order
{
    public interface ICouponRepository : IBaseRepository<Coupon>
    {
        Task<Coupon?> GetValidCouponAsync(string code, decimal subtotal);
    }

    public class CouponRepository : BaseRepository<Coupon>, ICouponRepository
    {
        public CouponRepository(EyeglassesDbContext context) : base(context) { }

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

