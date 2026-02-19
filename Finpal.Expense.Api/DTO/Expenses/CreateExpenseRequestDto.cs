namespace FinPal.Expense.Api.DTO.Expenses
{
    public class CreateExpenseRequestDto
    {
        
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime ExpenseDate { get; set; }

    }
}
