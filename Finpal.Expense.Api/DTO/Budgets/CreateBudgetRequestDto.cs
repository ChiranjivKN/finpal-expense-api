
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace FinPal.Expense.Api.DTO.Budgets
{
    public class CreateBudgetRequestDto
    {
        [Required]
        public int CategoryId { get; set; }
        
        [Range(1, 12)]
        public int Month { get; set; }

        [Range(2000, 2100)]
        public int Year { get; set; }

        [Range(0.01, 10000000)]
        public decimal BudgetAmount { get; set; }
    }
}
