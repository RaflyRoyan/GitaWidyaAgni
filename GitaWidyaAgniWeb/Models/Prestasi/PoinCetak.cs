using GitaWidyaAgniWeb.Reports;
using GWALib.Lib.Transaksi.Prestasi;

namespace GitaWidyaAgniWeb.Models.Prestasi
{
    /// <summary>
    /// Rekap poin keaktifan per periode dari Prestasi › Lomba &amp; Prestasi, tab Poin (tombol "Cetak Rekap Poin").
    /// Periode kosong = periode Aktif; section opsional. Penanda tangan Sekretaris + Ketua Umum periode itu.
    /// </summary>
    public static class PoinCetak
    {
        public static RekapPoinReport Cetak(int? periode, int? section, string webRoot)
        {
            var periodId = periode > 0 ? periode.Value : Opsi.PeriodeAktifId() ?? throw new ArgumentException("Pilih periode; belum ada periode aktif.");
            var signers = new List<DocumentReport.Signer>
            {
                Penandatangan.Jabatan("Sekretaris", periodId: periodId),
                Penandatangan.Jabatan("Ketua Umum", periodId: periodId)
            };
            return RekapPoinReport.Create(MemberPoint.GetRecap(periodId, section > 0 ? section : null), ReportBrand.Load(webRoot), signers);
        }
    }
}
