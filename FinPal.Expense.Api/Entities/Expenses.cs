using Microsoft.Identity.Client;

namespace FinPal.Expense.Api.Entities
{
    public class Expenses
    {
        public int ExpenseId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime ExpenseDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public User User { get; set; } = null!;
        public Category Category { get; set; } = null!;
        
    }
}
