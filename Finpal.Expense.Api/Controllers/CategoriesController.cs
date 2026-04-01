using FinPal.Expense.Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FinPal.Expense.Api.DTO.Categories;
using Microsoft.EntityFrameworkCore;
using FinPal.Expense.Api.Entities;
using Microsoft.Identity.Client;
using FinPal.Expense.Api.Services.Categories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FinPal.Expense.Api.Services.UserId;

namespace FinPal.Expense.Api.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;
        private readonly ICurrentUserService _currentUser;

        public CategoriesController(ICategoryService service, ICurrentUserService currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }        

        //POST: api/categories
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryRequestDto request)
        {
            var userId = _currentUser.UserId;

            var response = await _service.CreateAsync(userId, request);          

            return Ok(response);
        }

        //GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetByUser()
        {
            var userId = _currentUser.UserId;

            var response = await _service.GetByUserAsync(userId);

            return Ok(response);
        }

        //PUT: api/categories?categoryId1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateCategoryRequestDto request)
        {
            var userId = _currentUser.UserId;

            await _service.UpdateAsync(id, userId, request);

            return NoContent();
        }

        //DELETE: api/categories?categoryId1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);

            return NoContent();
        }
    }
}