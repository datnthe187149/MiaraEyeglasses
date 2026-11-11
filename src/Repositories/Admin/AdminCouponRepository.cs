using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Admin
{
    // Interface kho dữ liệu quản lý mã giảm giá khuyến mãi
    public interface IAdminCouponRepository : IBaseRepository<Coupon>
    {
        Task<IEnumerable<Coupon>> GetAllCouponsAsync();
        // Đổi trạng thái kích hoạt: ToggleActive
        Task ToggleActiveAsync(int couponId);
        // Cập nhật dữ liệu: UpdateCoupon
        Task<bool> UpdateCouponAsync(int couponId, string code, string discountType, decimal discountValue, decimal minOrderAmount, DateOnly expireDate, int? usageLimit);
    }

    // Lớp thực thi quản lý mã ưu đãi chiết khấu cho Admin
    public class AdminCouponRepository : BaseRepository<Coupon>, IAdminCouponRepository
    {
        // Khởi tạo đối tượng AdminCouponRepository kèm nạp các phụ thuộc cần thiết
        public AdminCouponRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<Coupon>> GetAllCouponsAsync()
        {
            return await _dbSet.ToListAsync();
        }

        // Đổi trạng thái kích hoạt: ToggleActive
        public async Task ToggleActiveAsync(int couponId)
        {
            var c = await _dbSet.FindAsync(couponId);
            if (c != null)
            {
                c.IsActive = !c.IsActive;
                await _context.SaveChangesAsync();
            }
        }

        // Cập nhật dữ liệu: UpdateCoupon
        public async Task<bool> UpdateCouponAsync(int couponId, string code, string discountType, decimal discountValue, decimal minOrderAmount, DateOnly expireDate, int? usageLimit)
        {
            var c = await _dbSet.FindAsync(couponId);
            if (c == null)
            {
                return false;
            }

            c.CouponCode = code.Trim().ToUpper();
            c.DiscountType = discountType.Trim().ToUpper();
            c.DiscountValue = discountValue;
            c.MinOrderAmount = minOrderAmount;
            c.ExpireDate = expireDate;
            c.UsageLimit = usageLimit;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

