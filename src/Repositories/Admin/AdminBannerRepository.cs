using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Admin
{
    // Interface kho dữ liệu quản lý banner quảng cáo trang chủ
    public interface IAdminBannerRepository : IBaseRepository<Banner>
    {
        Task<IEnumerable<Banner>> GetAllBannersAsync();
        // Đổi trạng thái kích hoạt: ToggleActive
        Task ToggleActiveAsync(int bannerId);
    }

    // Lớp thực thi quản lý danh sách banner cho Admin
    public class AdminBannerRepository : BaseRepository<Banner>, IAdminBannerRepository
    {
        // Khởi tạo đối tượng AdminBannerRepository kèm nạp các phụ thuộc cần thiết
        public AdminBannerRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<Banner>> GetAllBannersAsync()
        {
            return await _dbSet.OrderBy(b => b.SortOrder).ToListAsync();
        }

        // Đổi trạng thái kích hoạt: ToggleActive
        public async Task ToggleActiveAsync(int bannerId)
        {
            var b = await _dbSet.FindAsync(bannerId);
            if (b != null)
            {
                b.IsActive = !b.IsActive;
                await _context.SaveChangesAsync();
            }
        }
    }
}

