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
        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }        

        //POST: api/categories
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryRequestDto request)
        {
            var response = await _service.CreateAsync(request);          

            return Ok(response);
        }

        //GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetByUser()
        {
            var response = await _service.GetByUserAsync();

            return Ok(response);
        }

        //PUT: api/categories?categoryId1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateCategoryRequestDto request)
        {
            await _service.UpdateAsync(id, request);

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