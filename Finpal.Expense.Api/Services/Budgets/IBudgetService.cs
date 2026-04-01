using FinPal.Expense.Api.DTO.Budgets;

namespace FinPal.Expense.Api.Services.Budgets
{
    public interface IBudgetService
    {
        Task CreateAsync(int userId, CreateBudgetRequestDto request);
        Task<List<BudgetResponseDto>> FilterAsync(int userID, int? Month, int? Year);
        Task<List<BudgetSummaryDto>> GetSummaryAsync(int userID, int Month, int Year);
    }
}
