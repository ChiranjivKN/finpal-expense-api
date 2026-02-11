using Microsoft.Extensions.Primitives;

namespace FinPal.Expense.Api.Entities
{
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
