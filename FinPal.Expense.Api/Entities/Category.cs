namespace FinPal.Expense.Api.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public int UserId { get; set; } 
        public string CategoryName { get; set; } = null!;
        public bool IsActive { get; set; }

        public User User { get; set; } = null!;

        public ICollection<Expenses> Expenses { get; set; } = new List<Expenses>();
        public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    }
}
