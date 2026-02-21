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

        //POST: api/Expenses
        [HttpPost]
        public async Task<IActionResult> Create(CreateExpenseRequestDto request)
        {
            //Check if User exists
            var userExists = await _db.Users
                .AnyAsync(u => u.UserID == request.UserId && u.IsActive);

            if (!userExists)
            {
                return BadRequest("Invalid user.");
            }

            //Check if User-Category composite exists
            var categoryExists = await _db.Categories
                .AnyAsync(c => c.CategoryId == request.CategoryId && c.UserId == request.UserId && c.IsActive);

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

        //GET: api/Expenses
        [HttpGet]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var expenses = await _db.Expenses
                .AsNoTracking()
                .Where(e => e.UserId == userId && !e.IsDeleted)
                .Select(e => new ExpenseResponseDto
                {
                    ExpenseId = e.ExpenseId,
                    Amount = e.Amount,
                    Description = e.Description,
                    ExpenseDate = e.ExpenseDate,
                    CategoryName = e.Category.CategoryName
                })
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();

            return Ok(expenses);            
        }

        //GET: api/Expenses/filter?startDate-endDate
        [HttpGet("filter")]
        public async Task<IActionResult> Filter(int userId, DateTime startDate, DateTime endDate)
        {
            //confirm endDate is greater than startDate
            if (startDate > endDate)
            {
                return BadRequest("Invalid date range");
            }

            var expenses = await _db.Expenses
                .AsNoTracking()
                .Where(e => e.UserId == userId && !e.IsDeleted && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
                .Select(e => new ExpenseResponseDto
                {
                    ExpenseId = e.ExpenseId,
                    Amount = e.Amount,
                    Description = e.Description,
                    ExpenseDate = e.ExpenseDate,
                    CategoryName = e.Category.CategoryName
                })
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();

            return Ok(expenses);
        }
    }
}
