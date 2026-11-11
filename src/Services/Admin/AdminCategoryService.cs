using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    // Interface dịch vụ quản lý danh mục kính cho Admin
    public interface IAdminCategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        // Thêm mới dữ liệu: CreateCategory
        Task<(bool Success, string? ErrorMessage)> CreateCategoryAsync(string categoryName, string? description);
        // Đổi trạng thái kích hoạt: ToggleCategoryActive
        Task ToggleCategoryActiveAsync(int categoryId);
        // Cập nhật dữ liệu: UpdateCategory
        Task<(bool Success, string? ErrorMessage)> UpdateCategoryAsync(int categoryId, string categoryName, string? slug);
    }

    // Service triển khai nghiệp vụ danh mục sản phẩm
    public class AdminCategoryService : IAdminCategoryService
    {
        private readonly IAdminCategoryRepository _categoryRepository;

        // Khởi tạo đối tượng AdminCategoryService kèm nạp các phụ thuộc cần thiết
        public AdminCategoryService(IAdminCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllCategoriesWithProductsAsync();
        }

        // Thêm mới dữ liệu: CreateCategory
        public async Task<(bool Success, string? ErrorMessage)> CreateCategoryAsync(string categoryName, string? description)
        {
            var cat = new Category
            {
                CategoryName = categoryName,
                Slug = categoryName.Trim().ToLower().Replace(" ", "-"),
                Description = description,
                IsActive = true
            };

            await _categoryRepository.AddAsync(cat);
            await _categoryRepository.SaveChangesAsync();
            return (true, null);
        }

        // Đổi trạng thái kích hoạt: ToggleCategoryActive
        public async Task ToggleCategoryActiveAsync(int categoryId)
        {
            await _categoryRepository.ToggleActiveAsync(categoryId);
        }

        // Cập nhật dữ liệu: UpdateCategory
        public async Task<(bool Success, string? ErrorMessage)> UpdateCategoryAsync(int categoryId, string categoryName, string? slug)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return (false, "Tên danh mục không được để trống.");
            }

            var updated = await _categoryRepository.UpdateCategoryAsync(categoryId, categoryName, slug);
            if (!updated)
            {
                return (false, "Không tìm thấy danh mục cần cập nhật.");
            }

            return (true, null);
        }
    }
}

