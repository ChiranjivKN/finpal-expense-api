namespace FinPal.Expense.Api.DTO.Budgets
{
    public class BudgetResponseDto
    {
        public int BudgetId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BudgetAmount { get; set; }           
    }
}
