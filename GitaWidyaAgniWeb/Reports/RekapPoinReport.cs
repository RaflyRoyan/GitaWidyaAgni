using GWALib.Lib.Transaksi.Prestasi;

namespace GitaWidyaAgniWeb.Reports
{
    /// <summary>
    /// Rekap poin keaktifan satu periode (DevExpress, A4 landscape) dari GWALib MemberPoint.GetRecap: peringkat, anggota (NIA, section),
    /// poin per sumber dan total. Data dan penanda tangan: Models/Prestasi/PoinCetak.
    /// </summary>
    public class RekapPoinReport : DocumentReport
    {
        private RekapPoinReport(string title) : base(title, landscape: true, totalPages: true)
        {
        }

        public static RekapPoinReport Create(MemberPoint.PointRecap d, ReportBrand brand, List<Signer> signers)
        {
            var r = new RekapPoinReport("Rekap Poin " + d.PeriodName);
            r.Letterhead(brand);
            r.TitleBlock(new[] { "REKAP POIN KEAKTIFAN", d.PeriodName }, false, underlineFirst: true);
            r.Fields(new (string, string?)[]
            {
                ("Section", d.Section ?? "Semua section"),
                ("Anggota", d.Section == null ? $"{d.Rows.Count} anggota punya poin" : $"{d.Rows.Count} dari {d.RankedMembers} anggota yang punya poin"),
                ("Total poin", Num(d.Total)),
                ("Rata-rata", d.Rows.Count == 0 ? "-" : (d.Total / (decimal)d.Rows.Count).ToString("0.#", ID) + " poin per anggota")
            }, labelWidth: 110);

            var n = 95f;
            var widths = new[] { 110, r.W - 110 - 6 * n, n, n, n, n, n, n };
            var rows = d.Rows.Select(x => new Row
            {
                C1 = x.Rank.ToString(),
                C2 = x.Name + "\n" + string.Join(" · ", new[] { x.MemberNumber, x.Section }.Where(s => !string.IsNullOrWhiteSpace(s))),
                C3 = Num(x.Hadir),
                C4 = Num(x.Panitia),
                C5 = Num(x.Pengurus),
                C6 = Num(x.Prestasi),
                C7 = Num(x.Pemateri + x.Lainnya),
                C8 = Num(x.Total),
                Shade = x.Rank <= 3
            }).ToList();
            if (rows.Count == 0)
            {
                rows.Add(new Row { C2 = "Belum ada poin di periode ini" });
            }
            else
            {
                rows.Add(new Row
                {
                    C2 = "Jumlah", C3 = Num(d.Rows.Sum(x => x.Hadir)), C4 = Num(d.Rows.Sum(x => x.Panitia)), C5 = Num(d.Rows.Sum(x => x.Pengurus)),
                    C6 = Num(d.Rows.Sum(x => x.Prestasi)), C7 = Num(d.Rows.Sum(x => x.Pemateri + x.Lainnya)), C8 = Num(d.Total), Bold = true, Shade = true
                });
            }
            r.Table(new[] { "PERINGKAT", "ANGGOTA", "HADIR", "PANITIA", "PENGURUS", "PRESTASI", "LAINNYA", "TOTAL" }, widths,
                new[] { C, L, R, R, R, R, R, R }, rows, fontSize: 10f, rowHeight: 34f);

            r.Paragraphs("- Peringkat dihitung di antara semua anggota yang punya poin di periode ini; poin sama = peringkat sama. Baris berlatar = tiga besar.\n"
                + "- Section = section anggota saat ini. Lainnya termasuk poin pemateri.", false, "CATATAN", left: 0);
            r.Signatures(brand.City, DateTime.Today, signers, knownLast: false, gap: 16);
            return r;
        }
    }
}
