using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// Filter list berdasarkan is_active. Parameter query, bukan enum kolom (enum kolomnya <see cref="ActiveStatus"/>).
    /// </summary>
    public enum FilterAktif
    {
        [Description("Tidak Aktif")] TidakAktif = 0,
        [Description("Aktif")] Aktif = 1,
        [Description("Semua")] Semua = 2
    }
}
