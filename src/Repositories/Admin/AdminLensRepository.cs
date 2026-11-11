using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Admin
{
    // Interface kho dữ liệu quản lý danh mục tròng kính
    public interface IAdminLensRepository : IBaseRepository<LensOption>
    {
        Task<IEnumerable<LensOption>> GetAllLensesAsync();
        // Đổi trạng thái kích hoạt: ToggleActive
        Task ToggleActiveAsync(int lensOptionId);
        // Cập nhật dữ liệu: UpdateLens
        Task<bool> UpdateLensAsync(int lensOptionId, string lensName, decimal refractiveIndex, decimal extraPrice, string? description, string? rxLimit);
    }

    // Lớp thực thi quản lý các tùy chọn tròng kính cho Admin
    public class AdminLensRepository : BaseRepository<LensOption>, IAdminLensRepository
    {
        // Khởi tạo đối tượng AdminLensRepository kèm nạp các phụ thuộc cần thiết
        public AdminLensRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<LensOption>> GetAllLensesAsync()
        {
            return await _dbSet.ToListAsync();
        }

        // Đổi trạng thái kích hoạt: ToggleActive
        public async Task ToggleActiveAsync(int lensOptionId)
        {
            var l = await _dbSet.FindAsync(lensOptionId);
            if (l != null)
            {
                l.IsActive = !l.IsActive;
                await _context.SaveChangesAsync();
            }
        }

        // Cập nhật dữ liệu: UpdateLens
        public async Task<bool> UpdateLensAsync(int lensOptionId, string lensName, decimal refractiveIndex, decimal extraPrice, string? description, string? rxLimit)
        {
            var l = await _dbSet.FindAsync(lensOptionId);
            if (l == null)
            {
                return false;
            }

            l.LensName = lensName.Trim();
            if (refractiveIndex > 0)
            {
                l.RefractiveIndex = refractiveIndex;
            }
            l.ExtraPrice = extraPrice;
            l.Description = description?.Trim();
            l.RxLimit = rxLimit?.Trim();

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

