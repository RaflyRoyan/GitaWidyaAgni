using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_point_rule.source_type, t_member_point.source_type
    /// </summary>
    public enum PointSourceType
    {
        [Description("Hadir Kegiatan")] HadirKegiatan = 1,
        [Description("Panitia / Tim Pelaksana")] Panitia = 2,
        [Description("Pemateri")] Pemateri = 3,
        [Description("Pengurus")] Pengurus = 4,
        [Description("Prestasi")] Prestasi = 5,
        [Description("Lainnya")] Lainnya = 6
    }
}
