using FinPal.Expense.Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FinPal.Expense.Api.DTO.Categories;
using Microsoft.EntityFrameworkCore;
using FinPal.Expense.Api.Entities;
using Microsoft.Identity.Client;

namespace FinPal.Expense.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly FinPalDbContext _db;

        public CategoriesController(FinPalDbContext db)
        {
            _db = db;
        }

        //POST: api/categories

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryRequestDto request)
        {

            //check if user exists in Users table
            var userExists = await _db.Users.AnyAsync(u => u.UserID == request.UserId && u.IsActive);

            if (!userExists)
            {
                return BadRequest("User does not exist!");
            }

            //check if the category already exists

            var categoryExists = await _db.Categories.AnyAsync(c => c.UserId == request.UserId && c.CategoryName == request.CategoryName);

            if (categoryExists)
            {
                return BadRequest($"Category {request.CategoryName} already exists for {request.UserId}");
            }

            //create new category

            var category = new Category
            {
                UserId = request.UserId,
                CategoryName = request.CategoryName,
                IsActive = true

            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            //Response Dto

            var response = new CategoryResponseDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName

            };

            return Ok(response);

            /*return CreatedAtAction(
            nameof(GetById),
            new { id = category.CategoryId },
            response);*/      
            
        }

        //GET: api/categories?userId=1

        [HttpGet]
        
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var categories = await _db.Categories
                .Where(c => c.UserId == userId && c.IsActive)
                .Select(c => new CategoryResponseDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                })
                .ToListAsync();

            return Ok(categories);
        } 

    }
}