using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_activity.audience_type | penentu siapa yang dicatat ALPA
    /// </summary>
    public enum AudienceType
    {
        [Description("Semua Anggota")] SemuaAnggota = 1,
        [Description("Pengurus")] Pengurus = 2,
        [Description("Peserta Terdaftar")] PesertaTerdaftar = 3,
        [Description("Divisi")] Divisi = 4,
        [Description("Tim Pelaksana")] TimPelaksana = 5
    }
}
