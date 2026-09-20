using FinPal.Expense.Api.DTO.Budgets;

namespace FinPal.Expense.Api.Services.Budgets
{
    public interface IBudgetService
    {
        Task CreateAsync(CreateBudgetRequestDto request);
        Task<List<BudgetResponseDto>> FilterAsync(int? Month, int? Year);
        Task<List<BudgetSummaryDto>> GetSummaryAsync(int Month, int Year);
    }
}
