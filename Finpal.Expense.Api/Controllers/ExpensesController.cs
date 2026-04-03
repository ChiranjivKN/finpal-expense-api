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

        public ExpensesController(IExpenseService service)
        {
            _service = service;            
        }

        //POST: api/expenses
        [HttpPost]
        public async Task<IActionResult> Create(CreateExpenseRequestDto request)
        {        
            await _service.CreateAsync(request);

            return Ok();            
        }

        //GET: api/Expenses/filter?startDate=2026-01-01&endDate=2026-01-31
        [HttpGet("filter")]
        public async Task<IActionResult> Filter(DateTime? startDate, DateTime? endDate)
        {
            var response = await _service.FilterAsync(startDate, endDate);

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
