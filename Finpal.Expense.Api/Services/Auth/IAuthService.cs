using FinPal.Expense.Api.DTO.Users;

namespace FinPal.Expense.Api.Services.Auth
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterUserRequestDto request);
    }
}
