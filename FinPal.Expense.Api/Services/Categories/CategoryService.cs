using FinPal.Expense.Api.Data;
using FinPal.Expense.Api.DTO.Categories;
using Microsoft.EntityFrameworkCore;
using FinPal.Expense.Api.Entities;
using FinPal.Expense.Api.Infrastructure.CurrentUser;

namespace FinPal.Expense.Api.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly FinPalDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(FinPalDbContext db, ICurrentUserService currentUser, ILogger<CategoryService> logger)
        {
            _db = db;
            _currentUser = currentUser;
            _logger = logger;
        }

        private int UserId => _currentUser.UserId;

        //CreateCategory and return created
        public async Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto request)
        {
            var userExists = await _db.Users
                .AnyAsync(u => u.UserID == UserId && u.IsActive);

            if (!userExists)
            {
                _logger.LogWarning("Invalid user {UserId} attempted to create category.", UserId);

                throw new KeyNotFoundException("Invalid user");
            }

            var categoryExists = await _db.Categories
                .AnyAsync(c => c.CategoryName == request.CategoryName && c.UserId == UserId && c.IsActive);

            if (categoryExists)
            {
                _logger.LogWarning("User {UserId} attempted to create duplicate category.", UserId);

                throw new Exception("Category already exists");
            }

            var category = new Category
            {
                UserId = UserId,
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

            _logger.LogInformation("Category {CategoryId} created successfully for user {UserId}", response.CategoryId, UserId);

            return response;
        }
        
        //GetCategory by user
        public async Task<List<CategoryResponseDto>> GetByUserAsync()
        {
            var response = await _db.Categories
                .AsNoTracking()
                .Where(c => c.UserId == UserId && c.IsActive)
                .Select(c => new CategoryResponseDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                })
                .ToListAsync();

            _logger.LogInformation("User {UserId} fetched {Count} categories.", UserId, response.Count);

            return response;
        }

        //UpdateCategory
        public async Task UpdateAsync(int id, CreateCategoryRequestDto request)
        {
            _logger.LogInformation("User {UserId} attempting to update category {CategoryId}.", UserId, id);

            var category = await _db.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.IsActive);

            if (category == null)
            {
                _logger.LogWarning("User {UserId} attempted to update invalid category.", UserId);

                throw new KeyNotFoundException("Category does not exist");
            }

            var duplicate = await _db.Categories
                .AnyAsync(c => c.UserId == UserId && c.CategoryName == request.CategoryName && c.IsActive && c.CategoryId != id);

            if (duplicate)
            {
                _logger.LogWarning("User {UserId} failed to update category {CategoryId}: The name {CategoryName} is already in use.", UserId, id, request.CategoryName);

                throw new InvalidOperationException("Duplicate category");
            }

            category.CategoryName = request.CategoryName.Trim();

            await _db.SaveChangesAsync();

            _logger.LogInformation("User {UserId} updated category {CategoryId}.", UserId, id);
        }

        //SoftDelete categories
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("User {UserId} attempting to delete category {CategoryId}", UserId, id);

            var category = await _db.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.IsActive);

            if (category == null)
            {
                _logger.LogWarning("User {UserId} attempted to delete invalid category {CategoryId}", UserId, id);

                throw new KeyNotFoundException("Category not found!");
            }

            category.IsActive = false;

            await _db.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted category {CategoryId}", UserId, id);
        }
    }
}
