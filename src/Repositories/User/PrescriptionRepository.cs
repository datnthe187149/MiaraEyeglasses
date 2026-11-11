using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.User
{
    // Interface kho dữ liệu hồ sơ độ đo mắt cận, loạn thị
    public interface IPrescriptionRepository : IBaseRepository<CustomerPrescription>
    {
        Task<IEnumerable<CustomerPrescription>> GetPrescriptionsByUserIdAsync(int userId);
        // Lấy dữ liệu: PrescriptionById
        Task<CustomerPrescription?> GetPrescriptionByIdAsync(int userId, int prescriptionId);
        // Xóa dữ liệu: ClearDefaultPrescriptions
        Task ClearDefaultPrescriptionsAsync(int userId);
        // Kiểm tra điều kiện: HasAnyPrescription
        Task<bool> HasAnyPrescriptionAsync(int userId);
    }

    // Lớp thực thi quản lý hồ sơ thông số đo mắt
    public class PrescriptionRepository : BaseRepository<CustomerPrescription>, IPrescriptionRepository
    {
        // Khởi tạo đối tượng PrescriptionRepository kèm nạp các phụ thuộc cần thiết
        public PrescriptionRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<CustomerPrescription>> GetPrescriptionsByUserIdAsync(int userId)
        {
            return await _dbSet.Where(p => p.UserId == userId).OrderByDescending(p => p.IsDefault).ThenByDescending(p => p.CreatedAt).ToListAsync();
        }

        // Lấy dữ liệu: PrescriptionById
        public async Task<CustomerPrescription?> GetPrescriptionByIdAsync(int userId, int prescriptionId)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.UserPrescriptionId == prescriptionId && p.UserId == userId);
        }

        // Xóa dữ liệu: ClearDefaultPrescriptions
        public async Task ClearDefaultPrescriptionsAsync(int userId)
        {
            var list = await _dbSet.Where(p => p.UserId == userId && p.IsDefault).ToListAsync();
            foreach (var item in list) item.IsDefault = false;
        }

        // Kiểm tra điều kiện: HasAnyPrescription
        public async Task<bool> HasAnyPrescriptionAsync(int userId)
        {
            return await _dbSet.AnyAsync(p => p.UserId == userId);
        }
    }
}

