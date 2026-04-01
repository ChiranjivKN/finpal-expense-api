using System.ComponentModel.DataAnnotations;

namespace FinPal.Expense.Api.DTO.Expenses
{
    public class CreateExpenseRequestDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Range(0.01, 10000000)]
        public decimal Amount { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; }
    }
}


