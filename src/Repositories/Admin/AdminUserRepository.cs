using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Admin
{
    public interface IAdminUserRepository : IBaseRepository<Models.Entities.User>
    {
        Task<IEnumerable<Models.Entities.User>> GetUsersAsync(int? roleId = null, string? search = null);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Models.Entities.User?> GetUserByIdWithFullDetailsAsync(int userId);
        Task<bool> EmailExistsAsync(string email, int excludeUserId = 0);
        Task<(bool Success, string Message)> DeleteUserAsync(int userId);
        Task ToggleActiveAsync(int userId);
    }

    public class AdminUserRepository : BaseRepository<Models.Entities.User>, IAdminUserRepository
    {
        public AdminUserRepository(EyeglassesDbContext context) : base(context) { }

        // Lấy danh sách người dùng kèm theo vai trò (Role), có hỗ trợ lọc và tìm kiếm
        public async Task<IEnumerable<Models.Entities.User>> GetUsersAsync(int? roleId = null, string? search = null)
        {
            var query = _dbSet.Include(u => u.Role).AsQueryable();

            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleId == roleId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(term) || u.Email.ToLower().Contains(term) || u.Phone.Contains(term));
            }

            return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }

        // Lấy danh sách tất cả vai trò trong hệ thống
        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        // Lấy thông tin chi tiết một người dùng: bao gồm Vai trò, Sổ địa chỉ, Đơn kính cận và Toàn bộ lịch sử đơn hàng
        public async Task<Models.Entities.User?> GetUserByIdWithFullDetailsAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.Role)
                .Include(u => u.Addresses)
                .Include(u => u.Prescriptions)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.Items)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.Payments)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        // Kiểm tra địa chỉ email đã tồn tại hay chưa, có thể loại trừ một UserId đang sửa
        public async Task<bool> EmailExistsAsync(string email, int excludeUserId = 0)
        {
            string cleanEmail = email.Trim().ToLower();
            if (excludeUserId > 0)
            {
                return await _dbSet.AnyAsync(u => u.Email.ToLower() == cleanEmail && u.UserId != excludeUserId);
            }
            return await _dbSet.AnyAsync(u => u.Email.ToLower() == cleanEmail);
        }

        // Xóa người dùng: Nếu người dùng đã có đơn hàng thì khóa tài khoản để bảo toàn lịch sử; nếu chưa thì xóa khỏi CSDL
        public async Task<(bool Success, string Message)> DeleteUserAsync(int userId)
        {
            var user = await _dbSet
                .Include(u => u.Orders)
                .Include(u => u.Addresses)
                .Include(u => u.Prescriptions)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return (false, "Không tìm thấy tài khoản người dùng cần xóa.");
            }

            // Nếu người dùng đã từng phát sinh đơn hàng, không được xóa vật lý để giữ dữ liệu kế toán
            if (user.Orders != null && user.Orders.Count > 0)
            {
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return (true, "Tài khoản này đã có lịch sử đơn hàng nên không thể xóa hoàn toàn. Hệ thống đã tự động chuyển trạng thái tài khoản sang 'Đã khóa'.");
            }

            // Nếu chưa có đơn hàng, xóa sổ địa chỉ và đơn kính liên quan trước, sau đó xóa tài khoản
            if (user.Addresses != null && user.Addresses.Count > 0)
            {
                _context.UserAddresses.RemoveRange(user.Addresses);
            }

            if (user.Prescriptions != null && user.Prescriptions.Count > 0)
            {
                _context.CustomerPrescriptions.RemoveRange(user.Prescriptions);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return (true, "Đã xóa tài khoản người dùng thành công.");
        }

        // Bật / tắt trạng thái kích hoạt tài khoản
        public async Task ToggleActiveAsync(int userId)
        {
            var u = await _dbSet.FindAsync(userId);
            if (u != null)
            {
                u.IsActive = !u.IsActive;
                u.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}

