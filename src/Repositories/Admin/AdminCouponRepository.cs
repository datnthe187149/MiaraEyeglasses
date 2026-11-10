using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Admin
{
    public interface IAdminCouponRepository : IBaseRepository<Coupon>
    {
        Task<IEnumerable<Coupon>> GetAllCouponsAsync();
        Task ToggleActiveAsync(int couponId);
    }

    public class AdminCouponRepository : BaseRepository<Coupon>, IAdminCouponRepository
    {
        public AdminCouponRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<Coupon>> GetAllCouponsAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task ToggleActiveAsync(int couponId)
        {
            var c = await _dbSet.FindAsync(couponId);
            if (c != null)
            {
                c.IsActive = !c.IsActive;
                await _context.SaveChangesAsync();
            }
        }
    }
}

