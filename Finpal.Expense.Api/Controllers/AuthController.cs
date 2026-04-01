using FinPal.Expense.Api.DTO.Auth;
using FinPal.Expense.Api.DTO.Users;
using FinPal.Expense.Api.Services.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinPal.Expense.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController (IAuthService service)
        {
            _service = service;
        }

        //POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserRequestDto request)
        {
            await _service.RegisterAsync(request);
            return Ok();
        }

        //POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var token = await _service.LoginAsync(request);
            return Ok(new
            {
                token
            });
        }
    }
}
