using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    public interface IAdminCouponService
    {
        Task<IEnumerable<Coupon>> GetAllCouponsAsync();
        Task<(bool Success, string? ErrorMessage)> CreateCouponAsync(
            string code, decimal discountAmount, decimal minOrderAmount,
            DateTime startAt, DateTime expireAt, int usageLimit);
        Task ToggleCouponActiveAsync(int couponId);
        Task<(bool Success, string? ErrorMessage)> UpdateCouponAsync(
            int couponId, string code, string discountType, decimal discountValue, decimal minOrderAmount,
            DateTime expireAt, int? usageLimit);
    }

    public class AdminCouponService : IAdminCouponService
    {
        private readonly IAdminCouponRepository _couponRepository;

        public AdminCouponService(IAdminCouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        public async Task<IEnumerable<Coupon>> GetAllCouponsAsync()
        {
            return await _couponRepository.GetAllCouponsAsync();
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateCouponAsync(
            string code, decimal discountAmount, decimal minOrderAmount,
            DateTime startAt, DateTime expireAt, int usageLimit)
        {
            var coupon = new Coupon
            {
                CouponCode = code.Trim().ToUpper(),
                DiscountType = "AMOUNT",
                DiscountValue = discountAmount,
                MinOrderAmount = minOrderAmount,
                StartDate = DateOnly.FromDateTime(startAt),
                ExpireDate = DateOnly.FromDateTime(expireAt),
                UsageLimit = usageLimit,
                UsedCount = 0,
                IsActive = true
            };

            await _couponRepository.AddAsync(coupon);
            await _couponRepository.SaveChangesAsync();
            return (true, null);
        }

        public async Task ToggleCouponActiveAsync(int couponId)
        {
            await _couponRepository.ToggleActiveAsync(couponId);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateCouponAsync(
            int couponId, string code, string discountType, decimal discountValue, decimal minOrderAmount,
            DateTime expireAt, int? usageLimit)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return (false, "Mã giảm giá không được để trống.");
            }

            if (discountValue <= 0)
            {
                return (false, "Mức giảm giá phải lớn hơn 0.");
            }

            var updated = await _couponRepository.UpdateCouponAsync(
                couponId, code.Trim().ToUpper(), discountType, discountValue, minOrderAmount,
                DateOnly.FromDateTime(expireAt), usageLimit);

            if (!updated)
            {
                return (false, "Không tìm thấy mã giảm giá cần cập nhật.");
            }

            return (true, null);
        }
    }
}

