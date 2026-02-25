using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;

namespace FinPal.Expense.Api.DTO.Categories
{
    public class CreateCategoryRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        [MinLength(2)]
        public string CategoryName { get; set; } = null!;
    }
}
