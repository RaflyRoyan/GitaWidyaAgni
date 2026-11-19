using GWALib.Lib.Transaksi.Presensi;

namespace GitaWidyaAgniWeb.Reports
{
    /// <summary>
    /// Rekap kehadiran anggota (DevExpress): satu baris per anggota dari sesi presensi yang sudah ditutup pada rentang tanggal,
    /// opsional per kegiatan / section. Data disusun Models/Kegiatan/PresensiCetak.
    /// </summary>
    public class RekapPresensiReport : DocumentReport
    {
        public class Data
        {
            public DateOnly From { get; set; }
            public DateOnly To { get; set; }
            public string Activity { get; set; } = "Semua kegiatan";
            public string Section { get; set; } = "Semua section";
            public int Sessions { get; set; }
            public List<AttendanceSession.RecapRow> Rows { get; set; } = new();
            public Signer Signer { get; set; } = new("Sekretaris", null, null);
        }

        private RekapPresensiReport(string title) : base(title, totalPages: true)
        {
        }

        public static RekapPresensiReport Create(Data d, ReportBrand brand)
        {
            var range = d.From == d.To ? Date(d.From) : $"{Date(d.From)} s.d. {Date(d.To)}";
            var r = new RekapPresensiReport("Rekap Kehadiran " + range);
            r.Letterhead(brand);
            r.TitleBlock(new[] { "REKAP KEHADIRAN ANGGOTA", range }, false, underlineFirst: true);

            var totalHadir = d.Rows.Sum(x => x.Hadir + x.Terlambat);
            var totalCatatan = d.Rows.Sum(x => x.Sessions);
            r.Fields(new (string, string?)[]
            {
                ("Kegiatan", d.Activity),
                ("Section", d.Section),
                ("Sesi ditutup", d.Sessions + " sesi"),
                ("Anggota", d.Rows.Count + " orang"),
                ("Rata-rata hadir", totalCatatan == 0 ? "-" : Math.Round(100m * totalHadir / totalCatatan, 1).ToString("0.#", ID) + "%")
            }, labelWidth: 130);

            var rows = d.Rows.Select((x, k) => new Row
            {
                C1 = (k + 1).ToString(),
                C2 = x.Name + (string.IsNullOrWhiteSpace(x.MemberNumber) ? "" : "\n" + x.MemberNumber),
                C3 = x.Section ?? "-",
                C4 = x.Sessions.ToString(),
                C5 = (x.Hadir + x.Terlambat) + (x.Terlambat > 0 ? $" ({x.Terlambat})" : ""),
                C6 = (x.Izin + x.Sakit).ToString(),
                C7 = x.Alpa.ToString(),
                C8 = x.Percent.ToString("0.#", ID) + "%",
                Shade = x.Percent < 75
            }).ToList();
            if (rows.Count == 0)
            {
                rows.Add(new Row { C2 = "Tidak ada sesi ditutup pada rentang ini" });
            }
            r.Table(new[] { "NO", "NAMA / NIA", "SECTION", "SESI", "HADIR (TELAT)", "IZIN / SAKIT", "ALPA", "% HADIR" },
                new float[] { 34, r.W - 494, 110, 50, 90, 80, 55, 75 }, new[] { C, L, L, C, C, C, C, C }, rows, heading: "KEHADIRAN PER ANGGOTA", fontSize: 10.5f);
            r.Paragraphs("- Hadir termasuk terlambat (angka dalam kurung).\n- Baris berlatar menandai kehadiran di bawah 75%.", false, "CATATAN", left: 0);

            r.Signatures(brand.City, DateTime.Today, new List<Signer> { d.Signer }, knownLast: false, gap: 10);
            return r;
        }
    }
}
