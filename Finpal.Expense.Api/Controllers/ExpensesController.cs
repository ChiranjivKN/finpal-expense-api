using FinPal.Expense.Api.Data;
using FinPal.Expense.Api.DTO.Categories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FinPal.Expense.Api.DTO.Expenses;
using FinPal.Expense.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinPal.Expense.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly FinPalDbContext _db;

        public ExpensesController(FinPalDbContext db)
        {
            _db = db;
        }

        //POST: api/expenses
        [HttpPost]
        public async Task<IActionResult> Create(CreateExpenseRequestDto request)
        {
            //Check if User exists
            var userExists = await _db.Users.AnyAsync(u => u.UserID == request.UserId && u.IsActive);

            if (!userExists)
            {
                return BadRequest("Invalid user.");
            }

            //Check if User-Category composite exists
            var categoryExists = await _db.Categories.AnyAsync(c => c.CategoryId == request.CategoryId && c.UserId == request.UserId && c.IsActive);

            if(!categoryExists)
            {
                return BadRequest("Invalid category.");
            }

            var expenses = new Expenses
            {
                UserId = request.UserId,
                CategoryId = request.CategoryId,
                Amount = request.Amount,
                Description = request.Description,
                ExpenseDate = request.ExpenseDate
            };

            _db.Expenses.Add(expenses);
            await _db.SaveChangesAsync();

            return Ok();            
        }
    }
}
