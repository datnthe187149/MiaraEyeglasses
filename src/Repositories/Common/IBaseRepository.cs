using System.Linq.Expressions;

namespace MiaraEyeglasses.Repositories.Common
{
    // Interface kho dữ liệu dùng chung (Generic Repository CRUD)
    public interface IBaseRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        // Lấy dữ liệu: ById
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        // Thực thi tác vụ: SingleOrDefault
        Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);
        // Thêm mới dữ liệu: Add
        Task AddAsync(T entity);
        // Thêm mới dữ liệu: AddRange
        Task AddRangeAsync(IEnumerable<T> entities);
        // Cập nhật dữ liệu: Update
        void Update(T entity);
        // Xóa dữ liệu: Remove
        void Remove(T entity);
        // Thực thi tác vụ: SaveChanges
        Task<int> SaveChangesAsync();
    }
}

