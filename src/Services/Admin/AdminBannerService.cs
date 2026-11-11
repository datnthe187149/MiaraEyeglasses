using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    // Interface dịch vụ quản lý banner quảng cáo cho Admin
    public interface IAdminBannerService
    {
        Task<IEnumerable<Banner>> GetAllBannersAsync();
        // Thêm mới dữ liệu: CreateBanner
        Task<(bool Success, string? ErrorMessage)> CreateBannerAsync(
            string title, string imageUrl, string? linkUrl, int sortOrder);
        // Đổi trạng thái kích hoạt: ToggleBannerActive
        Task ToggleBannerActiveAsync(int bannerId);
    }

    // Service triển khai thêm, sửa, bật tắt banner
    public class AdminBannerService : IAdminBannerService
    {
        private readonly IAdminBannerRepository _bannerRepository;

        // Khởi tạo đối tượng AdminBannerService kèm nạp các phụ thuộc cần thiết
        public AdminBannerService(IAdminBannerRepository bannerRepository)
        {
            _bannerRepository = bannerRepository;
        }

        public async Task<IEnumerable<Banner>> GetAllBannersAsync()
        {
            return await _bannerRepository.GetAllBannersAsync();
        }

        // Thêm mới dữ liệu: CreateBanner
        public async Task<(bool Success, string? ErrorMessage)> CreateBannerAsync(
            string title, string imageUrl, string? linkUrl, int sortOrder)
        {
            var banner = new Banner
            {
                Title = title,
                ImageUrl = imageUrl,
                LinkUrl = linkUrl ?? "/",
                SortOrder = sortOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _bannerRepository.AddAsync(banner);
            await _bannerRepository.SaveChangesAsync();
            return (true, null);
        }

        // Đổi trạng thái kích hoạt: ToggleBannerActive
        public async Task ToggleBannerActiveAsync(int bannerId)
        {
            await _bannerRepository.ToggleActiveAsync(bannerId);
        }
    }
}

