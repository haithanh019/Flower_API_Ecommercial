using DataAccess.DTOs.CategoryDTOs;

namespace Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int categoryId);
        Task<int> CreateAsync(CategoryCreateDto dto);
        Task<bool> UpdateAsync(int categoryId, CategoryUpdateDto dto);
        Task<bool> DeleteAsync(int categoryId);
    }
}
