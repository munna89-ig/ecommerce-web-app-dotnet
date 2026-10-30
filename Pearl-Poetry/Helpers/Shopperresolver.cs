using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Data.Entities;

namespace Pearl_Poetry.Helpers
{
    // Identifies who is shopping: a logged-in user, or a guest (tracked by cookie).
    public record Shopper(int? UserId, int? GuestUserId)
    {
        public bool IsAuthenticated => UserId.HasValue;

        public bool HasIdentity => UserId.HasValue || GuestUserId.HasValue;
    }

    public interface IShopperResolver
    {
        Task<Shopper> GetAsync(bool createGuest = false);
    }

    public class ShopperResolver : IShopperResolver
    {
        public const string GuestCookie = "pp_guest";

        private readonly EcommerceSystemContext _context;
        private readonly IHttpContextAccessor _http;

        public ShopperResolver(
            EcommerceSystemContext context,
            IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        public async Task<Shopper> GetAsync(bool createGuest = false)
        {
            var http = _http.HttpContext
                ?? throw new InvalidOperationException("No active HttpContext.");

            // Logged-in user
            int? userId = null;

            if (http.User.Identity?.IsAuthenticated == true &&
                int.TryParse(
                    http.User.FindFirstValue(ClaimTypes.NameIdentifier),
                    out int parsedId))
            {
                userId = parsedId;
            }

            // Guest (cookie)
            int? guestId = null;

            string? sessionId = http.Request.Cookies[GuestCookie];

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                guestId = await _context.GuestUsers
                    .AsNoTracking()
                    .Where(g => g.SessionId == sessionId)
                    .Select(g => (int?)g.Id)
                    .FirstOrDefaultAsync();
            }

            // Create a guest only when we really need one
            if (createGuest && userId == null && guestId == null)
            {
                sessionId = Guid.NewGuid().ToString("N");

                var guest = new GuestUser
                {
                    SessionId = sessionId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.GuestUsers.Add(guest);
                await _context.SaveChangesAsync();

                guestId = guest.Id;

                http.Response.Cookies.Append(
                    GuestCookie,
                    sessionId,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax,
                        Secure = http.Request.IsHttps,
                        Expires = DateTimeOffset.UtcNow.AddDays(30)
                    });
            }

            return new Shopper(userId, guestId);
        }
    }
}