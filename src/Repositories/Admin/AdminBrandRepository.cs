using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Admin
{
    // Interface định nghĩa: IAdminBrandRepository
    public interface IAdminBrandRepository : IBaseRepository<Brand>
    {
        Task<IEnumerable<Brand>> GetAllBrandsWithProductsAsync();
        // Lấy dữ liệu: BrandById
        Task<Brand?> GetBrandByIdAsync(int brandId);
        // Cập nhật dữ liệu: UpdateBrand
        Task UpdateBrandAsync(Brand brand);
        // Đổi trạng thái kích hoạt: ToggleActive
        Task ToggleActiveAsync(int brandId);
    }

    // Class định nghĩa: AdminBrandRepository
    public class AdminBrandRepository : BaseRepository<Brand>, IAdminBrandRepository
    {
        // Khởi tạo đối tượng AdminBrandRepository kèm nạp các phụ thuộc cần thiết
        public AdminBrandRepository(EyeglassesDbContext context) : base(context) { }

        // Lấy danh sách thương hiệu kèm danh sách các sản phẩm thuộc thương hiệu đó
        public async Task<IEnumerable<Brand>> GetAllBrandsWithProductsAsync()
        {
            return await _dbSet.Include(b => b.Products).ToListAsync();
        }

        // Lấy thông tin thương hiệu theo ID
        public async Task<Brand?> GetBrandByIdAsync(int brandId)
        {
            return await _dbSet.FindAsync(brandId);
        }

        // Cập nhật thông tin thương hiệu
        public async Task UpdateBrandAsync(Brand brand)
        {
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync();
        }

        // Bật / tắt trạng thái phân phối của thương hiệu
        public async Task ToggleActiveAsync(int brandId)
        {
            var brand = await _dbSet.FindAsync(brandId);
            if (brand != null)
            {
                brand.IsActive = !brand.IsActive;
                await _context.SaveChangesAsync();
            }
        }
    }
}

