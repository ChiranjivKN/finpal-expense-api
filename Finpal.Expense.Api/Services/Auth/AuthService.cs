using FinPal.Expense.Api.DTO.Users;
using FinPal.Expense.Api.Data;
using Microsoft.EntityFrameworkCore;
using FinPal.Expense.Api.Entities;
using FinPal.Expense.Api.Services.Security;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using FinPal.Expense.Api.DTO.Auth;
using System.Security.Authentication;
using System.IdentityModel.Tokens.Jwt;

namespace FinPal.Expense.Api.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly FinPalDbContext _db;
        private readonly IConfiguration _config;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<AuthService> _logger;

        public AuthService (FinPalDbContext db, IPasswordService passwordService, IConfiguration config, ILogger<AuthService> logger)
        {
            _db = db;
            _config = config;
            _passwordService = passwordService;
            _logger = logger;
        }

        public async Task RegisterAsync(RegisterUserRequestDto request)
        {
            _logger.LogInformation("Registration attempt for email {Email}", request.Email);

            var emailExists = await _db.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == request.Email);

            if (emailExists)
            {
                _logger.LogWarning("Failed registration attempt for email {Email}", request.Email);

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

            _logger.LogInformation("New user {UserId} created successfully", user.UserID);
        }

        public async Task<string> LoginAsync(LoginRequestDto request)
        {
            _logger.LogInformation("Login attempt for email {Email}", request.Email);

            var user = await _db.Users                
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("Failed login attempt for email {Email}", request.Email);

                throw new AuthenticationException("Invalid email or password");
            }

            var verifyPassword = _passwordService.VerifyPassword(request.Password, user.Password);

            if (!verifyPassword)
            {
                _logger.LogWarning("Failed login attempt for email {Email}", request.Email);

                throw new AuthenticationException("Invalid email or password");
            }

            _logger.LogInformation("User {UserId} logged in successfully", user.UserID);

            return GenerateToken(user);
        }

        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(_config["JwtSettings:key"]!));
                
            var creds = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["JwtSettings:ExpiryMinutes"]!)),
                signingCredentials: creds
                );             

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
