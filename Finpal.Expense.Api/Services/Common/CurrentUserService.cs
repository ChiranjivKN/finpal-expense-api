using FinPal.Expense.Api.Services.UserId;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace FinPal.Expense.Api.Services.Common
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService (IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public int UserId
        {
            get
            {
                var userID = _httpContextAccessor
                    .HttpContext?
                    .User
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                return int.Parse(userID!);
            }
        }            
    }
}
