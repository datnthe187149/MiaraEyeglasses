using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Admin;

namespace MiaraEyeglasses.Services.Admin
{
    public interface IAdminCategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<(bool Success, string? ErrorMessage)> CreateCategoryAsync(string categoryName, string? description);
        Task ToggleCategoryActiveAsync(int categoryId);
        Task<(bool Success, string? ErrorMessage)> UpdateCategoryAsync(int categoryId, string categoryName, string? slug);
    }

    public class AdminCategoryService : IAdminCategoryService
    {
        private readonly IAdminCategoryRepository _categoryRepository;

        public AdminCategoryService(IAdminCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllCategoriesWithProductsAsync();
        }

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

        public async Task ToggleCategoryActiveAsync(int categoryId)
        {
            await _categoryRepository.ToggleActiveAsync(categoryId);
        }

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

