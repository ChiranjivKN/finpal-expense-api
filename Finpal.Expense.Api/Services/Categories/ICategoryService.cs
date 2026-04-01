using FinPal.Expense.Api.DTO.Categories;

namespace FinPal.Expense.Api.Services.Categories
{
    public interface ICategoryService
    {
        Task<CategoryResponseDto> CreateAsync(int userId, CreateCategoryRequestDto request);
        Task<List<CategoryResponseDto>> GetByUserAsync(int userId);
        Task UpdateAsync(int id, int userId, CreateCategoryRequestDto request);
        Task DeleteAsync(int id);
    }
}
