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

            var budget = new Budget
            {
                UserId = request.UserId,
                CategoryId = request.CategoryId,
                Month = request.Month,
                Year = request.Year,
                BudgetAmount = request.BudgetAmount
            };

            _db.Budgets.Add(budget);
            await _db.SaveChangesAsync();
            
            return Ok();
        }
    }
}
