using System.Security.Claims;

namespace GitaWidyaAgniWeb.Infrastructure
{
    public static class ClaimsExtensions
    {
        // Id user (m_user.id) dari cookie login; 0 bila belum login.
        public static int GetUserId(this ClaimsPrincipal user)
        {
            return int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        }
        // Nilai parameter "user" untuk method GWALib (id user sebagai string).
        public static string GetLibUser(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }
        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        }
    }
}
