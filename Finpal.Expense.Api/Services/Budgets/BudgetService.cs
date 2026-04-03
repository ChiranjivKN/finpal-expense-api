using FinPal.Expense.Api.Data;
using FinPal.Expense.Api.DTO.Budgets;
using Microsoft.EntityFrameworkCore;
using FinPal.Expense.Api.Entities;
using FinPal.Expense.Api.Services.UserId;

namespace FinPal.Expense.Api.Services.Budgets
{
    public class BudgetService : IBudgetService
    {
        private readonly FinPalDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public BudgetService(FinPalDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        private int UserId => _currentUser.UserId;

        public async Task CreateAsync(CreateBudgetRequestDto request)
        {
            //Verify category ownership
            var categoryExists = await _db.Categories
                .AsNoTracking()
                .AnyAsync(c => c.CategoryId == request.CategoryId && c.UserId == UserId && c.IsActive);

            if (!categoryExists)
            {
                throw new KeyNotFoundException("Invalid category");
            }

            //prevent duplicate budget
            var budgetExists = await _db.Budgets
                .AsNoTracking()
                .AnyAsync(b => b.CategoryId == request.CategoryId && b.UserId == UserId && b.Month == request.Month && b.Year == request.Year);

            if (budgetExists)
            {
                throw new InvalidOperationException("Budget already exists for this month");
            }

            var budgets = new Budget
            {
                UserId = UserId,
                CategoryId = request.CategoryId,
                Month = request.Month,
                Year = request.Year,
                BudgetAmount = request.BudgetAmount
            };

            _db.Budgets.Add(budgets);

            await _db.SaveChangesAsync();
        }

        public async Task <List<BudgetResponseDto>> FilterAsync(int? Month, int? Year)
        {
            var userExists = await _db.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserID == UserId && u.IsActive);

            if (!userExists)
            {
                throw new KeyNotFoundException("Invalid user");
            }

            var query = _db.Budgets
                .AsNoTracking()
                .Where(b => b.UserId == UserId);

            if (Month.HasValue)
            {
                if (Month.Value < 1 && Month.Value > 12)
                {
                    throw new ArgumentException("Invalid month");
                }
                query = query.Where(b => b.Month == Month.Value);
            }

            if (Year.HasValue)
            {
                query = query.Where(b => b.Year == Year.Value);
            }

            var budgets = await query
                .Select(b => new BudgetResponseDto
                {
                    BudgetId = b.BudgetId,
                    CategoryName = b.Category.CategoryName,
                    Month = b.Month, 
                    Year = b.Year,
                    BudgetAmount = b.BudgetAmount
                })
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .ToListAsync();

            return budgets;
        }

        public async Task<List<BudgetSummaryDto>> GetSummaryAsync(int month, int year)
        {
            var userExists = await _db.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserID == UserId && u.IsActive);

            if (!userExists)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (month < 1 || month > 12)
            {
                throw new ArgumentException("Invalid month");
            }

            //Calculate total expenses
            var totalExpenses = await _db.Expenses
                .AsNoTracking()
                .Where(e => e.UserId == UserId && e.ExpenseDate.Month == month && e.ExpenseDate.Year == year && !e.IsDeleted)
                .GroupBy(e => e.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    TotalSpent = g.Sum(e => e.Amount)
                })
                .ToListAsync();

            //convert to dictionary lookup for fast access
            var expenseLookup = totalExpenses.ToDictionary(
                x => x.CategoryId,
                x => x.TotalSpent);

            //fetch budgets
            var budgets = await _db.Budgets
                .AsNoTracking()
                .Where(b => b.UserId == UserId && b.Month == month && b.Year == year)
                .Select(b => new
                {
                    b.CategoryId,
                    b.BudgetAmount,
                    categoryName = b.Category.CategoryName
                })
                .ToListAsync();

            //summary
            var summary = budgets.Select(b =>
            {
                expenseLookup.TryGetValue(b.CategoryId, out var totalSpent);

                return new BudgetSummaryDto
                {
                    CategoryName = b.categoryName,
                    BudgetAmount = b.BudgetAmount,
                    TotalSpent = totalSpent,
                };
            })
                .ToList();

            //calculate remaining and status
            foreach (var item in summary)
            {
                item.Remaining = item.BudgetAmount - item.TotalSpent;
                item.Status = item.TotalSpent > item.BudgetAmount ? "Overspent" : "Within budget";
            }

            return(summary);
        }
    }
}
