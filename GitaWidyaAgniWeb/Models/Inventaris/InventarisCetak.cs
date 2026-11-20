using GitaWidyaAgniWeb.Reports;
using GWALib.Lib.Transaksi.Inventaris;

namespace GitaWidyaAgniWeb.Models.Inventaris
{
    /// <summary>
    /// Laporan inventaris dari Inventaris › Aset (tombol "Cetak Laporan Inventaris"): kondisi aset per kategori + riwayat perawatan
    /// dalam rentang tanggal (bawaan: awal periode Aktif / awal tahun sampai hari ini). Penanda tangan Sekretaris + Ketua Umum periode Aktif.
    /// </summary>
    public static class InventarisCetak
    {
        public static InventarisReport Cetak(DateTime? dari, DateTime? sampai, int? kategori, string webRoot)
        {
            var to = sampai.HasValue ? DateOnly.FromDateTime(sampai.Value) : DateOnly.FromDateTime(DateTime.Today);
            var from = dari.HasValue ? DateOnly.FromDateTime(dari.Value) : DariBawaan(to);
            var signers = new List<DocumentReport.Signer> { Penandatangan.Jabatan("Sekretaris"), Penandatangan.Jabatan("Ketua Umum") };
            return InventarisReport.Create(InventoryReport.GetInventory(from, to, kategori > 0 ? kategori : null), ReportBrand.Load(webRoot), signers);
        }

        /// <summary>Awal rentang perawatan bawaan: awal periode Aktif, else 1 Januari tahun ini.</summary>
        public static DateOnly DariBawaan(DateOnly to)
        {
            var active = GWALib.Lib.Master.Anggota.ManagementPeriod.GetActive();
            return active != null && active.StartDate <= to ? active.StartDate : new DateOnly(to.Year, 1, 1);
        }
    }
}
