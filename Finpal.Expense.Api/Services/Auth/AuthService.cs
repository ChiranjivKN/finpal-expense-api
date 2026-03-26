using FinPal.Expense.Api.DTO.Users;
using FinPal.Expense.Api.Data;
using Microsoft.EntityFrameworkCore;
using FinPal.Expense.Api.Entities;
using FinPal.Expense.Api.Services.Security;

namespace FinPal.Expense.Api.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly FinPalDbContext _db;
        private readonly IPasswordService _passwordService;

        public AuthService (FinPalDbContext db, IPasswordService passwordService)
        {
            _db = db;
            _passwordService = passwordService;
        }

        public async Task RegisterAsync(RegisterUserRequestDto request)
        {
            var emailExists = await _db.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == request.Email);

            if (emailExists)
            {
                throw new Exception("Email already exists");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = _passwordService.HashPassword(request.Password),
                IsActive = true
            };

            _db.Users.Add(user);

            await _db.SaveChangesAsync();
        }
    }
}
