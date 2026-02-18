using Microsoft.Extensions.Primitives;

namespace FinPal.Expense.Api.DTO.Categories
{
    public class CreateCategoryRequestDto
    {
        public int UserId { get; set; }
        public string CategoryName { get; set; } = null!;

    }
}
