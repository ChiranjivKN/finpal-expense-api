namespace FinPal.Expense.Api.DTO.Users
{
    public class RegisterUserRequestDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
    }
}
