using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_message_outbox.status
    /// </summary>
    public enum MessageOutboxStatus
    {
        [Description("Menunggu")] Menunggu = 1,
        [Description("Terkirim")] Terkirim = 2,
        [Description("Gagal")] Gagal = 3,
        [Description("Dibatalkan")] Dibatalkan = 4
    }
}
