using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;
using GWAShared.Extensions;

namespace GitaWidyaAgniWeb.Reports
{
    /// <summary>
    /// Rapor anggota per periode kepengurusan (DevExpress, A4 portrait) dari GWALib Member.GetReportCard: identitas + foto,
    /// kehadiran per bulan, poin dan peringkat, jabatan / divisi, kepanitiaan, prestasi, sertifikat. Data: Models/Anggota/RaporCetak.
    /// </summary>
    public class RaporAnggotaReport : DocumentReport
    {
        private const decimal LOW_PERCENT = 75;

        private RaporAnggotaReport(string title) : base(title, totalPages: true)
        {
        }

        public static RaporAnggotaReport Create(Member.ReportCard d, ReportBrand brand, List<Signer> signers)
        {
            var r = new RaporAnggotaReport($"Rapor {d.Fullname} {d.PeriodName}");
            r.Letterhead(brand);
            r.TitleBlock(new[] { "RAPOR ANGGOTA", d.PeriodName }, false, underlineFirst: true);

            var identity = new (string Label, string? Value)[]
            {
                ("Nama", d.Fullname),
                ("NIA", d.MemberNumber),
                ("NIM", d.Nim),
                ("Fakultas / Prodi", string.Join(" / ", new[] { d.Fakultas, d.ProgramStudi }.Where(x => !string.IsNullOrWhiteSpace(x)))),
                ("Angkatan", d.Angkatan?.ToString()),
                ("Section", d.Section),
                ("Status", d.Status.GetDescription()),
                ("Bergabung", d.JoinedAt == null ? null : Date(d.JoinedAt) + " (" + Member.TenureText(d.JoinedAt.Value, d.Until) + ")")
            };
            if (d.Photo != null && d.Photo.Length > 0)
            {
                // Foto kiri, label dan nilai di kanan (satu baris per data; nilai dibatasi satu baris agar sejajar dengan label).
                r.Table(null, new float[] { 100, 120, 12, r.W - 232 }, new[] { L, L, L, L }, new List<Row>
                {
                    new()
                    {
                        Img = d.Photo,
                        C2 = string.Join("\n", identity.Select(x => x.Label)),
                        C3 = string.Join("\n", identity.Select(_ => ":")),
                        C4 = string.Join("\n", identity.Select(x => Short(x.Value)))
                    }
                }, borders: false, imageColumn: 0, fontSize: 11.5f, rowHeight: 130);
            }
            else
            {
                r.Fields(identity, labelWidth: 130);
            }

            // A. Kehadiran
            var range = d.Until < d.PeriodStart ? "periode belum berjalan" : $"{Date(d.PeriodStart)} s.d. {Date(d.Until)}";
            r.Fields(new (string, string?)[]
            {
                ("Rentang", range),
                ("Sesi tercatat", d.Sessions + " sesi"),
                ("Hadir", (d.Hadir + d.Terlambat) + (d.Terlambat > 0 ? $" (terlambat {d.Terlambat})" : "")),
                ("Izin / sakit", (d.Izin + d.Sakit).ToString()),
                ("Alpa", d.Alpa.ToString()),
                ("Kehadiran", d.Sessions == 0 ? "-" : Pct(d.Percent) + (d.Percent < LOW_PERCENT ? " (di bawah 75%)" : ""))
            }, labelWidth: 130, heading: "A. KEHADIRAN");
            var months = d.Attendance.Where(x => x.Sessions > 0).Select(x => new Row
            {
                C1 = new DateTime(x.Year, x.Month, 1).ToString("MMMM yyyy", ID),
                C2 = x.Sessions.ToString(),
                C3 = x.Hadir.ToString(),
                C4 = x.Terlambat.ToString(),
                C5 = x.Izin.ToString(),
                C6 = x.Sakit.ToString(),
                C7 = x.Alpa.ToString(),
                C8 = Pct(x.Percent),
                Shade = x.Percent < LOW_PERCENT
            }).ToList();
            if (months.Count > 0)
            {
                var n = (r.W - 140 - 85) / 6;
                r.Table(new[] { "BULAN", "SESI", "HADIR", "TELAT", "IZIN", "SAKIT", "ALPA", "% HADIR" },
                    new[] { 140, n, n, n, n, n, n, 85f }, new[] { L, C, C, C, C, C, C, C }, months, fontSize: 10.5f);
            }

            // B. Poin
            var points = d.Points.Select((x, k) => new Row { C1 = (k + 1).ToString(), C2 = x.Source, C3 = x.Count.ToString(), C4 = x.Points.ToString("N0", ID) }).ToList();
            if (points.Count == 0)
            {
                points.Add(new Row { C2 = "Belum ada poin di periode ini" });
            }
            points.Add(new Row
            {
                C2 = "Total" + (d.Rank > 0 ? $" — peringkat {d.Rank} dari {d.RankedMembers} anggota" : ""),
                C4 = d.TotalPoints.ToString("N0", ID), Bold = true, Shade = true
            });
            r.Table(new[] { "NO", "SUMBER POIN", "CATATAN", "POIN" }, new[] { 34, r.W - 34 - 90 - 90, 90f, 90f }, new[] { C, L, C, R }, points,
                heading: "B. POIN KEAKTIFAN", fontSize: 10.5f);

            // C–F. Daftar kegiatan
            r.Items("C. JABATAN & DIVISI", new[] { "NO", "MULAI", "JABATAN / DIVISI", "KETERANGAN" },
                d.Positions.Concat(d.Divisions.Select(x => x with { Title = "Divisi " + x.Title })).ToList(), "Tidak memegang jabatan atau divisi");
            r.Items("D. KEPANITIAAN", new[] { "NO", "TANGGAL", "KEGIATAN", "PERAN" }, d.Committees, "Tidak ada tugas kepanitiaan");
            r.Items("E. PRESTASI", new[] { "NO", "TANGGAL", "PRESTASI", "KETERANGAN" }, d.Achievements, "Tidak ada prestasi tercatat");
            r.Items("F. SERTIFIKAT", new[] { "NO", "TERBIT", "JUDUL", "JENIS / NOMOR" }, d.Certificates, "Tidak ada sertifikat");

            r.Paragraphs("- Kehadiran dihitung dari sesi presensi yang sudah ditutup; hadir termasuk terlambat. Baris berlatar = kehadiran di bawah 75%.\n"
                + "- Poin, jabatan, kepanitiaan, prestasi, dan sertifikat dibatasi pada " + d.PeriodName + ".", false, "CATATAN", left: 0);
            r.Signatures(brand.City, DateTime.Today, signers, knownLast: false, gap: 16, qrType: "RAPOR_ANGGOTA");
            return r;
        }

        private void Items(string heading, string[] headers, List<Member.ReportItem> items, string empty)
        {
            var rows = items.Select((x, k) => new Row
            {
                C1 = (k + 1).ToString(),
                C2 = x.Date?.ToString("dd/MM/yyyy") ?? "-",
                C3 = x.Title,
                C4 = string.IsNullOrWhiteSpace(x.Detail) ? "-" : x.Detail
            }).ToList();
            if (rows.Count == 0)
            {
                rows.Add(new Row { C3 = empty });
            }
            Table(headers, new[] { 34, 95, (W - 129) * 0.55f, (W - 129) * 0.45f }, new[] { C, C, L, L }, rows, heading: heading, fontSize: 10.5f);
        }

        private static string Pct(decimal value) => value.ToString("0.#", ID) + "%";
        private static string Short(string? value)
        {
            var text = string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
            return text.Length > 48 ? text[..47] + "…" : text;
        }
    }
}
