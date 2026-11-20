using GitaWidyaAgniWeb.Infrastructure;
using GitaWidyaAgniWeb.Reports;
using GWALib.Lib.Master.Anggota;

namespace GitaWidyaAgniWeb.Models.Anggota
{
    /// <summary>
    /// Kartu Tanda Anggota (Reports/KtaReport): satu anggota (seukuran kartu, depan lalu belakang) atau massal (A4, depan | belakang
    /// per baris). Mencetak / mengunduh membuat token QR bila belum ada (Member.GetCard / GetCards).
    /// </summary>
    public static class KtaCetak
    {
        public static KtaReport Satu(Member member, string user, string webRoot, string baseUrl)
        {
            var card = member.GetCard(user).Ok();
            return new KtaReport(KtaReport.ToRows(new[] { card }, baseUrl), ReportBrand.Load(webRoot), "KTA " + (card.MemberNumber ?? card.Fullname), false);
        }

        public static KtaReport Massal(int? sectionId, string user, string webRoot, string baseUrl)
        {
            var cards = Member.GetCards(null, sectionId, user).Ok();
            return new KtaReport(KtaReport.ToRows(cards, baseUrl), ReportBrand.Load(webRoot), "KTA Anggota Aktif", true);
        }
    }
}
