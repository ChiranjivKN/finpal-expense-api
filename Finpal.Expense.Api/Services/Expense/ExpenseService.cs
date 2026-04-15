using FinPal.Expense.Api.Data;
using FinPal.Expense.Api.DTO.Expenses;
using Microsoft.EntityFrameworkCore;
using FinPal.Expense.Api.Entities;
using Microsoft.EntityFrameworkCore.Query.Internal;
using FinPal.Expense.Api.Services.UserId;

namespace FinPal.Expense.Api.Services.Expense
{
    public class ExpenseService : IExpenseService
    {
        private readonly FinPalDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<ExpenseService> _logger;

        public ExpenseService(FinPalDbContext db, ICurrentUserService currentUser, ILogger<ExpenseService> logger)
        {
            _db = db;
            _currentUser = currentUser;
            _logger = logger;
        }

        //CreateExpense
        public async Task CreateAsync(CreateExpenseRequestDto request)
        {
            var userId = _currentUser.UserId;

            _logger.LogInformation("User {UserId} attempting to create expense of amount {Amount}", userId, request.Amount);

            var userExists = await _db.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserID == userId && u.IsActive);

            if (!userExists)
            {
                _logger.LogWarning("Invalid user {UserId} attempted to create expense", userId);

                throw new KeyNotFoundException("Invalid User");
            }

            var categoryExists = await _db.Categories
                .AsNoTracking()
                .AnyAsync(c => c.UserId == userId && c.CategoryId == request.CategoryId && c.IsActive);

            if (!categoryExists)
            {
                _logger.LogWarning("User {UserId} attempted to create expense for invalid category", userId);

                throw new KeyNotFoundException("Invalid category");
            }

            var expense = new Expenses
            {
                UserId = userId,
                CategoryId = request.CategoryId,
                Amount = request.Amount,
                Description = request.Description,
                ExpenseDate = request.ExpenseDate                
            };

            _db.Expenses.Add(expense);

            await _db.SaveChangesAsync();

            _logger.LogInformation("Expense {ExpenseId} created for user {UserId} successfully", expense.ExpenseId, userId);
        }

        //GetExpense by filters
        public async Task<List<ExpenseResponseDto>> FilterAsync(DateTime? startDate, DateTime? endDate)
        {
            var userId = _currentUser.UserId;

            _logger.LogInformation("User {UserId} attempting to fetch expenses with filters StartDate = {StartDate} and EndDate = {EndDate}", userId, startDate, endDate);

            var userExists = await _db.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserID == userId && u.IsActive);            

            if (!userExists)
            {
                _logger.LogWarning("Invalid user {UserId} attempted to fetch expenses", userId);

                throw new KeyNotFoundException("Invalid user");
            }

            var query = _db.Expenses
                .AsNoTracking()
                .Where(e => e.UserId == userId && !e.IsDeleted);

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                _logger.LogWarning("User {UserId} attempted to fetch expenses with invalid date range", userId);

                throw new ArgumentException("Invalid date range");
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate <= endDate.Value);
            }

            var expenses = await query
                .Select(e => new ExpenseResponseDto
                {
                    ExpenseId = e.ExpenseId,
                    CategoryName = e.Category.CategoryName,
                    Amount = e.Amount,
                    Description = e.Description,
                    ExpenseDate = e.ExpenseDate,
                })
                .OrderByDescending(e => e.ExpenseDate)
                .ThenByDescending(e => e.ExpenseId)
                .ToListAsync();

            _logger.LogInformation("User {UserId} fetched {Count} expenses", userId, expenses.Count);

            return expenses;            
        }

        //SoftDelete expenses
        public async Task DeleteAsync(int id)
        {
            var userId = _currentUser.UserId;

            _logger.LogInformation("User {UserId} attempting to delete expense {ExpenseId}", userId, id);

            var expense = await _db.Expenses
                .FirstOrDefaultAsync(e => e.ExpenseId == id && !e.IsDeleted);            

            if (expense == null)
            {
                _logger.LogWarning("User {UserId} attempted to delete invalid expense {ExpenseId}", userId, id);

                throw new KeyNotFoundException("Invalid expense");
            }

            expense.IsDeleted = true;

            await _db.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted expense {ExpenseId}", userId, id);
        }
    }
}
