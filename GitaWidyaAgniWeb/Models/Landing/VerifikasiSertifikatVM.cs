using GWALib.Lib.Transaksi.Prestasi;

namespace GitaWidyaAgniWeb.Models.Landing
{
    // Halaman publik /sertifikat/verifikasi?kode=...: cek keaslian sertifikat dari kode verifikasi.
    public class VerifikasiSertifikatVM
    {
        public string? Kode { get; set; }
        public bool Dicari => !string.IsNullOrWhiteSpace(Kode);
        public Certificate? Sertifikat { get; set; }
    }
}
