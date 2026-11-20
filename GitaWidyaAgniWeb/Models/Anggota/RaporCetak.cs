using GitaWidyaAgniWeb.Reports;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Anggota
{
    /// <summary>
    /// Rapor anggota per periode (RaporAnggotaReport): pengurus dari Data Anggota, anggota dari Profil Saya.
    /// Periode kosong = periode Aktif. Penanda tangan Sekretaris + Ketua Umum periode rapor.
    /// </summary>
    public static class RaporCetak
    {
        public static RaporAnggotaReport Pengurus(int memberId, int? periode, string webRoot)
        {
            var periodId = PeriodeAtauAktif(periode);
            return Buat(Member.GetReportCard(memberId, periodId), periodId, webRoot);
        }

        public static RaporAnggotaReport Saya(string user, int? periode, string webRoot)
        {
            var periodId = PeriodeAtauAktif(periode);
            return Buat(Member.GetMyReportCard(user, periodId), periodId, webRoot);
        }

        /// <summary>Periode yang bisa dipilih untuk rapor: Aktif dan Ditutup, terbaru dulu.</summary>
        public static List<GwaVM.OpsiItem> PeriodeOpsi()
        {
            return ManagementPeriod.GetDataList()
                .Where(x => x.Status != ManagementPeriodStatus.Draft)
                .OrderByDescending(x => x.StartDate)
                .Select(x => new GwaVM.OpsiItem(x.Id, x.Name + (x.Status == ManagementPeriodStatus.Active ? " (aktif)" : "")))
                .ToList();
        }

        private static int PeriodeAtauAktif(int? periode)
        {
            return periode > 0 ? periode.Value : ManagementPeriod.GetActive()?.Id ?? throw new ArgumentException("Pilih periode; belum ada periode aktif.");
        }

        private static RaporAnggotaReport Buat(Member.ReportCard card, int periodId, string webRoot)
        {
            var signers = new List<DocumentReport.Signer>
            {
                Penandatangan.Jabatan("Sekretaris", periodId: periodId),
                Penandatangan.Jabatan("Ketua Umum", periodId: periodId)
            };
            return RaporAnggotaReport.Create(card, ReportBrand.Load(webRoot), signers);
        }
    }
}
