using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;

namespace MiaraEyeglasses.Repositories.Common
{
    // Lớp cơ sở triển khai các thao tác CRUD cơ bản qua Entity Framework Core
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly EyeglassesDbContext _context;
        protected readonly DbSet<T> _dbSet;

        // Khởi tạo đối tượng BaseRepository kèm nạp các phụ thuộc cần thiết
        public BaseRepository(EyeglassesDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        // Lấy dữ liệu: ById
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        // Thực thi tác vụ: SingleOrDefault
        public virtual async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.SingleOrDefaultAsync(predicate);
        }

        // Thêm mới dữ liệu: Add
        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        // Thêm mới dữ liệu: AddRange
        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        // Cập nhật dữ liệu: Update
        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        // Xóa dữ liệu: Remove
        public virtual void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        // Thực thi tác vụ: SaveChanges
        public virtual async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}

