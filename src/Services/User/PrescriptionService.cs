using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Repositories.User;

namespace MiaraEyeglasses.Services.User
{
    public interface IPrescriptionService
    {
        Task<IEnumerable<CustomerPrescription>> GetPrescriptionsAsync(int userId);
        Task<bool> AddPrescriptionAsync(int userId, AddPrescriptionViewModel model);
        Task<bool> DeletePrescriptionAsync(int userId, int prescriptionId);
        Task<bool> SetDefaultPrescriptionAsync(int userId, int prescriptionId);
    }

    public class PrescriptionService : IPrescriptionService
    {
        private readonly IPrescriptionRepository _prescriptionRepository;

        public PrescriptionService(IPrescriptionRepository prescriptionRepository)
        {
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<IEnumerable<CustomerPrescription>> GetPrescriptionsAsync(int userId)
        {
            return await _prescriptionRepository.GetPrescriptionsByUserIdAsync(userId);
        }

        public async Task<bool> AddPrescriptionAsync(int userId, AddPrescriptionViewModel model)
        {
            if (model.IsDefault)
            {
                await _prescriptionRepository.ClearDefaultPrescriptionsAsync(userId);
            }
            else
            {
                var hasAny = await _prescriptionRepository.HasAnyPrescriptionAsync(userId);
                if (!hasAny) model.IsDefault = true;
            }

            var p = new CustomerPrescription
            {
                UserId = userId,
                ProfileName = model.ProfileName,
                RightSph = model.RightSph,
                RightCyl = model.RightCyl,
                RightAxis = model.RightAxis,
                LeftSph = model.LeftSph,
                LeftCyl = model.LeftCyl,
                LeftAxis = model.LeftAxis,
                PD = model.PD,
                PrescriptionImg = model.PrescriptionImg,
                IsDefault = model.IsDefault,
                CreatedAt = DateTime.UtcNow
            };

            await _prescriptionRepository.AddAsync(p);
            await _prescriptionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePrescriptionAsync(int userId, int prescriptionId)
        {
            var p = await _prescriptionRepository.GetPrescriptionByIdAsync(userId, prescriptionId);
            if (p == null) return false;

            _prescriptionRepository.Remove(p);
            await _prescriptionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetDefaultPrescriptionAsync(int userId, int prescriptionId)
        {
            var list = await _prescriptionRepository.GetPrescriptionsByUserIdAsync(userId);
            var target = list.FirstOrDefault(x => x.UserPrescriptionId == prescriptionId);
            if (target == null) return false;

            await _prescriptionRepository.ClearDefaultPrescriptionsAsync(userId);
            target.IsDefault = true;
            _prescriptionRepository.Update(target);
            await _prescriptionRepository.SaveChangesAsync();
            return true;
        }
    }
}

