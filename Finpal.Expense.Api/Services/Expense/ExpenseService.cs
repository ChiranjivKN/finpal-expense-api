using FinPal.Expense.Api.Data;
using FinPal.Expense.Api.DTO.Expenses;
using Microsoft.EntityFrameworkCore;
using FinPal.Expense.Api.Entities;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace FinPal.Expense.Api.Services.Expense
{
    public class ExpenseService : IExpenseService
    {
        private readonly FinPalDbContext _db;

        public ExpenseService(FinPalDbContext db)
        {
            _db = db;
        }

        //CreateExpense
        public async Task CreateAsync(CreateExpenseRequestDto request)
        {
            var userExists = await _db.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserID == request.UserId && u.IsActive);

            if (!userExists)
            {
                throw new KeyNotFoundException("Invalid User");
            }

            var categoryExists = await _db.Categories
                .AsNoTracking()
                .AnyAsync(c => c.UserId == request.UserId && c.CategoryId == request.CategoryId && c.IsActive);

            if (!categoryExists)
            {
                throw new KeyNotFoundException("Invalid category");
            }

            var expense = new Expenses
            {
                UserId = request.UserId,
                CategoryId = request.CategoryId,
                Amount = request.Amount,
                Description = request.Description,
                ExpenseDate = request.ExpenseDate
            };

            _db.Expenses.Add(expense);

            await _db.SaveChangesAsync();
        }

        //GetExpense by filters
        public async Task<List<ExpenseResponseDto>> FilterAsync(int userId, DateTime? startDate, DateTime? endDate)
        {
            var userExists = await _db.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserID == userId && u.IsActive);            

            if (!userExists)
            {
                throw new KeyNotFoundException("Invalid user");
            }

            var query = _db.Expenses
                .AsNoTracking()
                .Where(e => e.UserId == userId && !e.IsDeleted);

            if (startDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.ExpenseDate <= endDate.Value);
            }

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                throw new ArgumentException("Invalid date range");
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

            return expenses;
        }

        //SoftDelete expenses
        public async Task DeleteAsync(int id)
        {
            var expense = await _db.Expenses
                .FirstOrDefaultAsync(e => e.ExpenseId == id && !e.IsDeleted);

            if (expense == null)
            {
                throw new KeyNotFoundException("Invalid expense");
            }

            expense.IsDeleted = true;

            await _db.SaveChangesAsync();
        }
    }
}
