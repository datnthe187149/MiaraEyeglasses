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
        Task<bool> EmailExistsAsync(string email);
        Task ToggleActiveAsync(int userId);
    }

    public class AdminUserRepository : BaseRepository<Models.Entities.User>, IAdminUserRepository
    {
        public AdminUserRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<IEnumerable<Models.Entities.User>> GetUsersAsync(int? roleId = null, string? search = null)
        {
            var query = _dbSet.Include(u => u.Role).AsQueryable();

            if (roleId.HasValue)
                query = query.Where(u => u.RoleId == roleId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(term) || u.Email.ToLower().Contains(term) || u.Phone.Contains(term));
            }

            return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task ToggleActiveAsync(int userId)
        {
            var u = await _dbSet.FindAsync(userId);
            if (u != null)
            {
                u.IsActive = !u.IsActive;
                await _context.SaveChangesAsync();
            }
        }
    }
}

