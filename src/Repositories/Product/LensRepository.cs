using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Product
{
    // Interface kho dữ liệu tùy chọn tròng kính
    public interface ILensRepository : IBaseRepository<LensOption>
    {
        Task<IEnumerable<LensOption>> GetActiveLensOptionsAsync();
    }

    // Lớp thực thi truy xuất bảng giá các loại tròng kính
    public class LensRepository : BaseRepository<LensOption>, ILensRepository
    {
        // Khởi tạo đối tượng LensRepository kèm nạp các phụ thuộc cần thiết
        public LensRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<LensOption>> GetActiveLensOptionsAsync()
        {
            return await _dbSet.Where(l => l.IsActive).OrderBy(l => l.ExtraPrice).ToListAsync();
        }
    }
}

