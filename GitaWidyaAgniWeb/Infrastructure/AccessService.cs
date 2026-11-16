using GWALib.Lib.Master.Akses;
using GWALib.Lib.Master.Organization;
using Microsoft.Extensions.Caching.Memory;

namespace GitaWidyaAgniWeb.Infrastructure
{
    /// <summary>
    /// Cache hak akses dan menu per user (5 menit) serta identitas organisasi untuk layout.
    /// Clear(userId) dipanggil saat login / logout supaya perubahan role langsung terasa.
    /// </summary>
    public class AccessService
    {
        public const string DEFAULT_APP_NAME = "Gita Widya Agni";
        private static readonly TimeSpan Duration = TimeSpan.FromMinutes(5);
        private readonly IMemoryCache _cache;

        public AccessService(IMemoryCache cache)
        {
            _cache = cache;
        }

        // null = semua aksi (SUPERADMIN).
        public HashSet<string>? GetAllowedActions(int userId)
        {
            return _cache.GetOrCreate($"access:actions:{userId}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Duration;
                return UserAccess.GetAllowedActions(userId);
            });
        }
        public Dictionary<string, bool> GetRegisteredActions()
        {
            return _cache.GetOrCreate("access:registered", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Duration;
                return UserAccess.GetRegisteredActions();
            })!;
        }
        public List<UserAccess.MenuItem> GetMenu(int userId)
        {
            return _cache.GetOrCreate($"access:menu:{userId}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Duration;
                return UserAccess.GetMenu(userId);
            })!;
        }
        public UserAccess.UserProfile? GetProfile(int userId)
        {
            return _cache.GetOrCreate($"access:profile:{userId}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Duration;
                return UserAccess.GetProfile(userId);
            });
        }
        public string GetAppName()
        {
            return _cache.GetOrCreate("brand:name", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Duration;
                var name = OrganizationProfile.GetData()?.Name;
                return string.IsNullOrWhiteSpace(name) ? DEFAULT_APP_NAME : name;
            })!;
        }
        public void Clear(int userId)
        {
            _cache.Remove($"access:actions:{userId}");
            _cache.Remove($"access:menu:{userId}");
            _cache.Remove($"access:profile:{userId}");
        }
        public void ClearAll()
        {
            if (_cache is MemoryCache memory)
            {
                memory.Compact(1.0);
            }
        }
    }
}
