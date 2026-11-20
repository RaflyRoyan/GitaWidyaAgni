using GitaWidyaAgniWeb.Reports;
using GWALib.Lib.Master.Keuangan;
using GWALib.Lib.Transaksi.Keuangan;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Keuangan
{
    /// <summary>
    /// Laporan keuangan dari Keuangan › Transaksi (tombol "Cetak Laporan Keuangan"): buku kas, arus kas, realisasi anggaran, rekap iuran.
    /// Data dari GWALib FinancialReport; penanda tangan Bendahara + Ketua Umum periode Aktif.
    /// </summary>
    public static class KeuanganCetak
    {
        public const string BUKU_KAS = "bukukas";
        public const string ARUS_KAS = "aruskas";
        public const string REALISASI = "realisasi";
        public const string IURAN = "iuran";

        /// <summary>Pilihan untuk modal _Laporan.</summary>
        public class Form
        {
            public List<GwaVM.OpsiItem> Akun { get; set; } = Opsi.Akun();
            public List<GwaVM.OpsiItem> Anggaran { get; set; } = Budget.GetDataList()
                .OrderByDescending(x => x.Status == BudgetStatus.Approved).ThenByDescending(x => x.Id)
                .Select(x => new GwaVM.OpsiItem(x.Id, $"{x.Name} · {x.PeriodName} ({GWAShared.Extensions.EnumExtensions.GetDescription(x.Status)})")).ToList();
            public List<GwaVM.OpsiItem> Iuran { get; set; } = DuesSetting.GetDataList(FilterAktif.Semua)
                .OrderByDescending(x => x.IsActive == ActiveStatus.Aktif).ThenByDescending(x => x.Id)
                .Select(x => new GwaVM.OpsiItem(x.Id, x.Name + (x.Period == null ? "" : " · " + x.Period.Name))).ToList();
            public List<GwaVM.OpsiItem> Section { get; set; } = Opsi.Section();
            /// <summary>Rentang bawaan = periode Aktif sampai hari ini, atau awal tahun ini.</summary>
            public DateOnly Dari { get; set; }
            public DateOnly Sampai { get; set; } = DateOnly.FromDateTime(DateTime.Today);

            public Form()
            {
                var active = GWALib.Lib.Master.Anggota.ManagementPeriod.GetActive();
                Dari = active != null && active.StartDate <= Sampai ? active.StartDate : new DateOnly(Sampai.Year, 1, 1);
            }
        }

        public static KeuanganReport Cetak(string? jenis, DateTime? dari, DateTime? sampai, int? akun, int? anggaran, int? iuran, int? section, string webRoot)
        {
            var brand = ReportBrand.Load(webRoot);
            var signers = new List<DocumentReport.Signer> { Penandatangan.Jabatan("Bendahara"), Penandatangan.Jabatan("Ketua Umum") };
            var to = sampai.HasValue ? DateOnly.FromDateTime(sampai.Value) : DateOnly.FromDateTime(DateTime.Today);
            var from = dari.HasValue ? DateOnly.FromDateTime(dari.Value) : new DateOnly(to.Year, to.Month, 1);
            var accountId = akun > 0 ? akun : null;
            switch (jenis)
            {
                case BUKU_KAS:
                    return KeuanganReport.BukuKas(FinancialReport.GetCashBook(from, to, accountId), brand, signers);
                case ARUS_KAS:
                    return KeuanganReport.ArusKas(FinancialReport.GetCashFlow(from, to, accountId), brand, signers);
                case REALISASI:
                    if (!(anggaran > 0))
                    {
                        throw new ArgumentException("Pilih anggaran.");
                    }
                    return KeuanganReport.RealisasiAnggaran(FinancialReport.GetBudgetRealization(anggaran.Value), brand, signers);
                case IURAN:
                    if (!(iuran > 0))
                    {
                        throw new ArgumentException("Pilih pengaturan iuran.");
                    }
                    var sectionId = section > 0 ? section : null;
                    var sectionName = sectionId == null ? "Semua section" : Opsi.Section().FirstOrDefault(x => x.Value == sectionId)?.Text ?? "-";
                    return KeuanganReport.RekapIuran(FinancialReport.GetDuesRecap(iuran.Value, sectionId), sectionName, brand, signers);
                default:
                    throw new ArgumentException("Jenis laporan tidak dikenal.");
            }
        }
    }
}
