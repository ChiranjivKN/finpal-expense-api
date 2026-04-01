using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FinPal.Expense.Api.Data;
using FinPal.Expense.Api.DTO.Budgets;
using Microsoft.EntityFrameworkCore;
using FinPal.Expense.Api.Entities;
using FinPal.Expense.Api.Services.Budgets;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FinPal.Expense.Api.Services.UserId;

namespace FinPal.Expense.Api.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BudgetsController : ControllerBase
    {
        private readonly IBudgetService _service;
        private readonly ICurrentUserService _currentUser;
        public BudgetsController(IBudgetService service, ICurrentUserService currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        //POST: api/Budgets
        [HttpPost]
        public async Task<IActionResult> Create(CreateBudgetRequestDto request)
        {
            var userId = _currentUser.UserId;

            await _service.CreateAsync(userId, request);

            return Ok();
        }

        //GET: api/Budgets?month=1&year=2026
        [HttpGet("filter")]
        public async Task<IActionResult> Filter(int? month, int? year)
        {
            var userId = _currentUser.UserId;

            var response = await _service.FilterAsync(userId, month, year); 

            return Ok(response);
        }
        
        //GET: api/budgets/summary?month=1&year=2026
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(int month, int year)
        {
            var userId =_currentUser.UserId;

            var response = await _service.GetSummaryAsync(userId, month, year);    
            
            return Ok(response);
        }               
    }
}
