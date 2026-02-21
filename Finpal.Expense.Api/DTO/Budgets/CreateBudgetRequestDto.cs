
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FinPal.Expense.Api.DTO.Budgets
{
    public class CreateBudgetRequestDto
    {
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BudgetAmount { get; set; }
    }
}
