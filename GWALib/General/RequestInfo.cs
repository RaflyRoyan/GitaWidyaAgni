namespace GWALib.General
{
    /// <summary>
    /// Info request HTTP yang sedang berjalan (alamat IP, user agent), diisi aplikasi web per request (middleware).
    /// Dipakai jejak audit (ElectionAuditEvent, ActivityLog) bila pemanggil tidak memberi nilainya sendiri.
    /// AsyncLocal: nilainya mengikuti alur async request itu saja; di luar request (job, seeder) bernilai null.
    /// </summary>
    public static class RequestInfo
    {
        private static readonly AsyncLocal<string?> _ipAddress = new();
        private static readonly AsyncLocal<string?> _userAgent = new();

        public static string? IpAddress
        {
            get => _ipAddress.Value;
            set => _ipAddress.Value = value;
        }
        public static string? UserAgent
        {
            get => _userAgent.Value;
            set => _userAgent.Value = value;
        }
    }
}
