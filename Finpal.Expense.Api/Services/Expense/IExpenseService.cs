using FinPal.Expense.Api.DTO.Expenses;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace FinPal.Expense.Api.Services.Expense
{
    public interface IExpenseService
    {
        Task CreateAsync(int userId, CreateExpenseRequestDto request);
        Task<List<ExpenseResponseDto>> FilterAsync(int userId, DateTime? startDate, DateTime? endDate);
        Task DeleteAsync(int id);
    }
}
