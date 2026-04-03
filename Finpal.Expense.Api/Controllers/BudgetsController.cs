using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FinPal.Expense.Api.Data;
using FinPal.Expense.Api.DTO.Budgets;
using Microsoft.EntityFrameworkCore;
using FinPal.Expense.Api.Entities;
using FinPal.Expense.Api.Services.Budgets;
using Microsoft.AspNetCore.Authorization;

namespace FinPal.Expense.Api.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BudgetsController : ControllerBase
    {
        private readonly IBudgetService _service;
        public BudgetsController(IBudgetService service)
        {
            _service = service;
        }

        //POST: api/Budgets
        [HttpPost]
        public async Task<IActionResult> Create(CreateBudgetRequestDto request)
        {
            await _service.CreateAsync(request);

            return Ok();
        }

        //GET: api/Budgets?month=1&year=2026
        [HttpGet("filter")]
        public async Task<IActionResult> Filter(int? month, int? year)
        {
            var response = await _service.FilterAsync(month, year); 

            return Ok(response);
        }
        
        //GET: api/budgets/summary?month=1&year=2026
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(int month, int year)
        {
            var response = await _service.GetSummaryAsync(month, year);    
            
            return Ok(response);
        }               
    }
}
