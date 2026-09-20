namespace FinPal.Expense.Api.DTO.Expenses
{
    public class ExpenseResponseDto
    {        
        public int ExpenseId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string? CategoryName { get; set; }

    }
}
