using FinPal.Expense.Api.Data;
using FinPal.Expense.Api.DTO.Categories;
using Microsoft.EntityFrameworkCore;
using FinPal.Expense.Api.Entities;

namespace FinPal.Expense.Api.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly FinPalDbContext _db;

        public CategoryService(FinPalDbContext db)
        {
            _db = db;
        }

        //CreateCategory and return created
        public async Task<CategoryResponseDto> CreateAsync(int userId, CreateCategoryRequestDto request)
        {
            var userExists = await _db.Users
                .AnyAsync(u => u.UserID == userId && u.IsActive);

            if (!userExists)
            {
                throw new KeyNotFoundException("Invalid user");
            }

            var categoryExists = await _db.Categories
                .AnyAsync(c => c.CategoryName == request.CategoryName && c.UserId == userId && c.IsActive);

            if (categoryExists)
            {
                throw new Exception("Category already exists");
            }

            var category = new Category
            {
                UserId = userId,
                CategoryName = request.CategoryName.Trim(),
                IsActive = true
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var response = new CategoryResponseDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,                
            };
           
            return response;
        }
        
        //GetCategory by user
        public async Task<List<CategoryResponseDto>> GetByUserAsync(int userId)
        {
            var response = await _db.Categories
                .AsNoTracking()
                .Where(c => c.UserId == userId && c.IsActive)
                .Select(c => new CategoryResponseDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                })
                .ToListAsync();

            return response;
        }

        //UpdateCategory
        public async Task UpdateAsync(int id, int userId, CreateCategoryRequestDto request)
        {                           
            var category = await _db.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.IsActive);

            if (category == null)
            {
                throw new KeyNotFoundException("Category does not exist");
            }

            var duplicate = await _db.Categories
                .AnyAsync(c => c.UserId == userId && c.CategoryName == request.CategoryName && c.IsActive && c.CategoryId != id);

            if (duplicate)
            {
                throw new InvalidOperationException("Duplicate category");
            }

            category.CategoryName = request.CategoryName.Trim();

            await _db.SaveChangesAsync();
        }

        //SoftDelete categories
        public async Task DeleteAsync(int id)
        {
            var category = await _db.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.IsActive);

            if (category == null)
            {
                throw new KeyNotFoundException("Category not found!");
            }

            category.IsActive = false;

            await _db.SaveChangesAsync();
        }
    }
}
