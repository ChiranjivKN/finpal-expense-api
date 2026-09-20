using FinPal.Expense.Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinPal.Expense.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly FinPalDbContext _db;

        public HealthController(FinPalDbContext db)
        {
            _db = db;
        }

        [HttpGet("db")]

        public async Task<IActionResult> CheckDatabase()
        {            
            var canConnect = await _db.Database.CanConnectAsync();
            return Ok(new
            {
                database = canConnect ? "Connected" : "Not Connected"
            });            
            
        }

    }
}
