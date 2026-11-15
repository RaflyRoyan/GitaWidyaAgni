using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_certificate.certificate_type
    /// </summary>
    public enum CertificateType
    {
        [Description("Peserta")] Peserta = 1,
        [Description("Panitia / Tim Pelaksana")] Panitia = 2,
        [Description("Pemateri")] Pemateri = 3,
        [Description("Pengurus")] Pengurus = 4,
        [Description("Prestasi")] Prestasi = 5,
        [Description("Lainnya")] Lainnya = 6
    }
}
