using Microsoft.AspNetCore.Identity;

namespace FinPal.Expense.Api.Services.Security
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}
