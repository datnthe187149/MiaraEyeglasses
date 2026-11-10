using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    public interface IAdminBannerService
    {
        Task<IEnumerable<Banner>> GetAllBannersAsync();
        Task<(bool Success, string? ErrorMessage)> CreateBannerAsync(
            string title, string imageUrl, string? linkUrl, int sortOrder);
        Task ToggleBannerActiveAsync(int bannerId);
    }

    public class AdminBannerService : IAdminBannerService
    {
        private readonly IAdminBannerRepository _bannerRepository;

        public AdminBannerService(IAdminBannerRepository bannerRepository)
        {
            _bannerRepository = bannerRepository;
        }

        public async Task<IEnumerable<Banner>> GetAllBannersAsync()
        {
            return await _bannerRepository.GetAllBannersAsync();
        }

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

        public async Task ToggleBannerActiveAsync(int bannerId)
        {
            await _bannerRepository.ToggleActiveAsync(bannerId);
        }
    }
}

