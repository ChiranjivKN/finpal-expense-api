using FinPal.Expense.Api.Data;
using FinPal.Expense.Api.DTO.Categories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FinPal.Expense.Api.DTO.Expenses;
using FinPal.Expense.Api.Entities;
using Microsoft.EntityFrameworkCore;
using FinPal.Expense.Api.Services.Expense;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FinPal.Expense.Api.Services.UserId;

namespace FinPal.Expense.Api.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _service;
        private readonly ICurrentUserService _currentUser;

        public ExpensesController(IExpenseService service, ICurrentUserService currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        //POST: api/expenses
        [HttpPost]
        public async Task<IActionResult> Create(CreateExpenseRequestDto request)
        {
            var userId = _currentUser.UserId;

            await _service.CreateAsync(userId, request);

            return Ok();            
        }

        //GET: api/Expenses/filter?startDate=2026-01-01&endDate=2026-01-31
        [HttpGet("filter")]
        public async Task<IActionResult> Filter(DateTime? startDate, DateTime? endDate)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var response = await _service.FilterAsync(userId, startDate, endDate);

            return Ok(response);
        }

        //DELETE: api/expenses?id1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
