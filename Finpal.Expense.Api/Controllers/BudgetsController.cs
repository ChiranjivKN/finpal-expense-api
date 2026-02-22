using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FinPal.Expense.Api.Data;
using FinPal.Expense.Api.DTO.Budgets;
using Microsoft.EntityFrameworkCore;
using FinPal.Expense.Api.Entities;

namespace FinPal.Expense.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetsController : ControllerBase
    {
        private readonly FinPalDbContext _db;

        public BudgetsController(FinPalDbContext db)
        {
            _db = db;
        }

        //POST: api/Budgets
        [HttpPost]
        public async Task<IActionResult> Create(CreateBudgetRequestDto request)
        {
            //Validate category ownership
            var categoryExists = await _db.Categories
                .AnyAsync(c => c.CategoryId == request.CategoryId && c.UserId == request.UserId && c.IsActive);

            if (!categoryExists)
            {
                return BadRequest("Invalid category");
            }

            //Prevent duplicate budget
            var budgetExists = await _db.Budgets
                .AnyAsync(b => b.UserId == request.UserId && b.CategoryId == request.CategoryId && b.Month == request.Month && b.Year == request.Year);

            if (budgetExists)
            {
                return BadRequest("Budget already exists for this month");
            }

            var budgets = new Budget
            {
                UserId = request.UserId,
                CategoryId = request.CategoryId,
                Month = request.Month,
                Year = request.Year,
                BudgetAmount = request.BudgetAmount
            };

            _db.Budgets.Add(budgets);
            await _db.SaveChangesAsync();
            
            return Ok();
        }

        //GET: api/Budgets?userId=1&month=1&year=2026
        [HttpGet("filter")]
        public async Task<IActionResult> Filter(int userId, int? month, int? year)
        {            
            var query = _db.Budgets
                .AsNoTracking()
                .Where(b => b.UserId == userId);

            if (month.HasValue)
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest("Invalid month");
                }

                query = query.Where(b => b.Month == month.Value);
            }

            if (year.HasValue)
            {
                query = query.Where(b => b.Year == year.Value);
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

            return Ok(budgets);
        }
        
        //GET: api/budgets/summary?userId1&month=1&year=2026
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(int userId, int month, int year)
        {            
            //Validate month
            if (month < 1 || month > 12)
            {
                return BadRequest("Invalid month");
            }

            //Validate user
            var userExists = await _db.Users.AnyAsync(u => u.UserID == userId && u.IsActive);

            if (!userExists)
            {
                return NotFound("User not found");
            }

            //Calculate total expenses
            var expenseTotals = await _db.Expenses
                .AsNoTracking()
                .Where(e => e.UserId == userId && !e.IsDeleted && e.ExpenseDate.Month == month && e.ExpenseDate.Year == year)
                .GroupBy(e => e.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    TotalSpent = g.Sum(e => e.Amount)
                })
                .ToListAsync();

            //Convert to dictionary lookup for fast access
            var expenseLookup = expenseTotals.ToDictionary(
                x => x.CategoryId,
                x => x.TotalSpent);

            //Fetch budgets
            var budgets = await _db.Budgets
                .AsNoTracking()
                .Where(b => b.UserId == userId && b.Month == month && b.Year == year)
                .Select(b => new 
                {
                    b.CategoryId,                    
                    b.BudgetAmount,
                    categoryName = b.Category.CategoryName                    
                })
                .ToListAsync();

            //Get TotalSpent
            var summary = budgets.Select(b =>
            {
                expenseLookup.TryGetValue(b.CategoryId, out var totalSpent);

                return new BudgetSummaryDto
                {
                    CategoryName = b.categoryName,
                    BudgetAmount = b.BudgetAmount,
                    TotalSpent = totalSpent
                };
            })
                .ToList();

            //Calculate remaining amount and status
            foreach (var item in summary)
            {
                item.Remaining = item.BudgetAmount - item.TotalSpent;

                item.Status = item.TotalSpent > item.BudgetAmount ? "Overspent" : "Within Budget";
            }

            return Ok(summary);
        }               
    }
}
