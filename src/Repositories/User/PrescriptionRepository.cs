using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.User
{
    public interface IPrescriptionRepository : IBaseRepository<CustomerPrescription>
    {
        Task<IEnumerable<CustomerPrescription>> GetPrescriptionsByUserIdAsync(int userId);
        Task<CustomerPrescription?> GetPrescriptionByIdAsync(int userId, int prescriptionId);
        Task ClearDefaultPrescriptionsAsync(int userId);
        Task<bool> HasAnyPrescriptionAsync(int userId);
    }

    public class PrescriptionRepository : BaseRepository<CustomerPrescription>, IPrescriptionRepository
    {
        public PrescriptionRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<CustomerPrescription>> GetPrescriptionsByUserIdAsync(int userId)
        {
            return await _dbSet.Where(p => p.UserId == userId).OrderByDescending(p => p.IsDefault).ThenByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<CustomerPrescription?> GetPrescriptionByIdAsync(int userId, int prescriptionId)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.UserPrescriptionId == prescriptionId && p.UserId == userId);
        }

        public async Task ClearDefaultPrescriptionsAsync(int userId)
        {
            var list = await _dbSet.Where(p => p.UserId == userId && p.IsDefault).ToListAsync();
            foreach (var item in list) item.IsDefault = false;
        }

        public async Task<bool> HasAnyPrescriptionAsync(int userId)
        {
            return await _dbSet.AnyAsync(p => p.UserId == userId);
        }
    }
}

