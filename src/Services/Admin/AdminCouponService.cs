using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    // Interface dịch vụ quản lý mã giảm giá cho Admin
    public interface IAdminCouponService
    {
        Task<IEnumerable<Coupon>> GetAllCouponsAsync();
        // Thêm mới dữ liệu: CreateCoupon
        Task<(bool Success, string? ErrorMessage)> CreateCouponAsync(
            string code, decimal discountAmount, decimal minOrderAmount,
            DateTime startAt, DateTime expireAt, int usageLimit);
        // Đổi trạng thái kích hoạt: ToggleCouponActive
        Task ToggleCouponActiveAsync(int couponId);
        // Cập nhật dữ liệu: UpdateCoupon
        Task<(bool Success, string? ErrorMessage)> UpdateCouponAsync(
            int couponId, string code, string discountType, decimal discountValue, decimal minOrderAmount,
            DateTime expireAt, int? usageLimit);
    }

    // Service tạo và quản lý thời hạn của mã khuyến mãi
    public class AdminCouponService : IAdminCouponService
    {
        private readonly IAdminCouponRepository _couponRepository;

        // Khởi tạo đối tượng AdminCouponService kèm nạp các phụ thuộc cần thiết
        public AdminCouponService(IAdminCouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        public async Task<IEnumerable<Coupon>> GetAllCouponsAsync()
        {
            return await _couponRepository.GetAllCouponsAsync();
        }

        // Thêm mới dữ liệu: CreateCoupon
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

        // Đổi trạng thái kích hoạt: ToggleCouponActive
        public async Task ToggleCouponActiveAsync(int couponId)
        {
            await _couponRepository.ToggleActiveAsync(couponId);
        }

        // Cập nhật dữ liệu: UpdateCoupon
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

