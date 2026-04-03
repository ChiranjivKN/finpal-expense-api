using FinPal.Expense.Api.DTO.Categories;

namespace FinPal.Expense.Api.Services.Categories
{
    public interface ICategoryService
    {
        Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto request);
        Task<List<CategoryResponseDto>> GetByUserAsync();
        Task UpdateAsync(int id, CreateCategoryRequestDto request);
        Task DeleteAsync(int id);
    }
}
