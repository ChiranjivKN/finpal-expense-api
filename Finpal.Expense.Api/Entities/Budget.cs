namespace FinPal.Expense.Api.Entities
{
    public class Budget
    {
        public int BudgetId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BudgetAmount { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
        public Category Category { get; set; } = null!;

    }
}
