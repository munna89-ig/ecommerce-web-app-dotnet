using System.Security.Claims;

namespace Pearl_Poetry.Helpers
{
    public static class UserRoleHelper
    {
        public static bool IsGuest(ClaimsPrincipal user)
        {
            return user.Identity?.IsAuthenticated != true;
        }

        public static bool IsCustomer(ClaimsPrincipal user)
        {
            return user.IsInRole("Customer");
        }

        public static bool IsSeller(ClaimsPrincipal user)
        {
            return user.IsInRole("Seller");
        }

        public static bool IsAdmin(ClaimsPrincipal user)
        {
            return user.IsInRole("Admin");
        }
    }
}
