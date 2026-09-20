namespace FinPal.Expense.Api.DTO.Budgets
{
    public class BudgetSummaryDto
    {
        public string CategoryName { get; set; } = null!;
        public decimal BudgetAmount { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal Remaining { get; set; }
        public string Status { get; set; } = null!;
    }
}
