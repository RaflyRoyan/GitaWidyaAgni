using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using GWALib.Lib.Transaksi.Program;
using GWAShared.Enums;
using System.Drawing;

namespace GitaWidyaAgniWeb.Reports
{
    /// <summary>
    /// LPJ Kegiatan / Kepengurusan, mengikuti template LPJ MB Gita Widya Agni: sampul, bab bernomor (narasi dari t_lpj +
    /// data modul), tanda tangan, lalu lampiran (dokumentasi, realisasi anggaran / inventaris, susunan pengurus).
    /// Blok generik ada di DocumentReport.
    /// </summary>
    public class LpjReport : DocumentReport
    {
        public const string INSTITUTION = "UNIVERSITAS PEMBANGUNAN NASIONAL “VETERAN”\nJAWA TIMUR";

        private LpjReport(string title) : base(title)
        {
        }

        // ================================================================= LPJ KEGIATAN
        public static LpjReport Kegiatan(Lpj.KegiatanData d, ReportBrand brand)
        {
            var lpj = d.Lpj;
            var r = new LpjReport("LPJ " + lpj.Title);
            var org = d.Org.Name.ToUpper(ID);
            r.Cover(new[] { "LAPORAN PERTANGGUNGJAWABAN", lpj.Title.ToUpper(ID), org }, brand.Logo, d.Year);

            r.TitleBlock(new[] { "LAPORAN PERTANGGUNGJAWABAN", lpj.Title.ToUpper(ID), org }, false);
            var n = 0;
            r.Section(++n, "PENDAHULUAN", lpj.Introduction);
            r.Section(++n, "LATAR BELAKANG", lpj.Background);
            r.Section(++n, "MACAM KEGIATAN", lpj.ActivityKind);
            r.Section(++n, "MAKSUD DAN TUJUAN", lpj.Purpose);
            r.Section(++n, "BENTUK KEGIATAN", lpj.ActivityForm);

            // Waktu & tempat: satu tabel tanpa garis per jadwal, judul bab ikut jadwal pertama.
            var title = $"{++n}. WAKTU DAN TEMPAT PELAKSANAAN";
            for (var i = 0; i < d.Schedules.Count; i++)
            {
                var s = d.Schedules[i];
                var sub = $"{INDENT}{(char)('a' + i % 26)}. {s.Name}";
                r.Fields(new (string, string?)[]
                {
                    ("Tanggal", s.Date), ("Hari", s.Day), ("Jam", s.Time), ("Tempat", s.Location)
                }, labelWidth: 100, left: 48, heading: i == 0 ? title + "\n" + sub : sub);
            }

            var results = d.Results.Count == 0 ? null : string.Join("\n", d.Results.Select((x, k) => $"{k + 1}. {x}"));
            r.Paragraphs(results, true, $"{++n}. HASIL KEGIATAN");
            r.Section(++n, "DOKUMENTASI", "Terlampir");
            r.Section(++n, "REALISASI ANGGARAN", "Terlampir");
            r.Section(++n, "PENUTUP", lpj.Closing);
            r.Signatures(brand.City, lpj.FinalizedAt ?? DateTime.Today, Signers(d.Signers), knownLast: true);

            // Lampiran
            r.TitleBlock(new[] { "DOKUMENTASI", lpj.Title.ToUpper(ID) }, true);
            r.Photos(d.Photos);

            r.TitleBlock(new[] { "REALISASI ANGGARAN", lpj.Title.ToUpper(ID), org }, true);
            r.BudgetTables(d.Budget);
            return r;
        }

        // ================================================================= LPJ KEPENGURUSAN
        public static LpjReport Kepengurusan(Lpj.KepengurusanData d, ReportBrand brand)
        {
            var lpj = d.Lpj;
            var r = new LpjReport("LPJ " + lpj.Title);
            var org = d.Org.Name.ToUpper(ID);
            var periode = Periode(d.PeriodName);
            r.Cover(new[] { "LAPORAN PERTANGGUNGJAWABAN", "KEPENGURUSAN " + org, periode }, brand.Logo, d.EndDate.Year);

            var n = 0;
            r.Section(++n, "PENDAHULUAN", lpj.Introduction);
            r.Section(++n, "VISI DAN MISI", lpj.VisionMission);
            r.Section(++n, "TUJUAN", lpj.Purpose);

            var rows = new List<Row>();
            foreach (var division in d.Programs.GroupBy(x => x.Division))
            {
                rows.Add(new Row { C2 = "BIDANG " + division.Key.ToUpper(ID), Bold = true, Shade = true });
                foreach (var term in division.GroupBy(x => x.Term))
                {
                    if (term.Key != null || division.Any(x => x.Term != null))
                    {
                        rows.Add(new Row { C2 = term.Key == null ? "Lainnya" : Infrastructure.EnumHelper.Text(term.Key.Value), Bold = true });
                    }
                    var k = 0;
                    rows.AddRange(term.Select(x => new Row { C1 = (++k).ToString(), C2 = x.Title, C3 = ProgramText(x.Status) }));
                }
            }
            if (rows.Count == 0)
            {
                rows.Add(new Row { C2 = "Belum ada program kerja pada periode ini" });
            }
            r.Table(new[] { "NO", "PROGRAM KERJA", "KETERANGAN" }, new float[] { 40, r.W - 200, 160 }, new[] { C, L, C }, rows,
                heading: $"{++n}. EVALUASI PROGRAM KERJA {periode}");

            r.Spacer();
            var activities = d.Activities.Select((x, k) => new Row
            {
                C1 = (k + 1).ToString(),
                C2 = x.Title,
                C3 = x.StartAt.ToString("d MMM yyyy", ID),
                C4 = ActivityText(x.Status)
            }).ToList();
            if (activities.Count == 0)
            {
                activities.Add(new Row { C2 = "Belum ada kegiatan pada periode ini" });
            }
            r.Table(new[] { "NO", "NAMA KEGIATAN", "TANGGAL", "KETERANGAN" }, new float[] { 40, r.W - 320, 120, 160 }, new[] { C, L, C, C }, activities);

            r.Section(++n, "DATA INVENTARIS", "Terlampir");
            r.Section(++n, "SUSUNAN PENGURUS " + periode, "Terlampir");
            r.Section(++n, "DOKUMENTASI SELAMA KEPENGURUSAN", "Terlampir");
            r.Section(++n, "PENUTUP", lpj.Closing);
            r.Signatures(brand.City, lpj.FinalizedAt ?? DateTime.Today, Signers(d.Signers), knownLast: true);

            // Lampiran: inventaris
            r.TitleBlock(new[] { "DAFTAR INVENTARIS SARANA DAN PRASARANA", org }, true);
            var assets = new List<Row>();
            var c = 0;
            foreach (var category in d.Assets.GroupBy(x => x.Category))
            {
                assets.Add(new Row { C1 = (++c).ToString(), C2 = category.Key.ToUpper(ID), Bold = true, Shade = true });
                assets.AddRange(category.Select(x => new Row { C2 = x.Name, C3 = $"{x.Quantity} {x.Unit}", C4 = x.Note }));
            }
            if (assets.Count == 0)
            {
                assets.Add(new Row { C2 = "Belum ada data inventaris" });
            }
            r.Table(new[] { "NO", "NAMA ALAT", "JUMLAH", "KETERANGAN" }, new float[] { 40, r.W - 340, 100, 200 }, new[] { C, L, C, L }, assets);

            // Lampiran: susunan pengurus (tanpa alamat / telepon)
            r.TitleBlock(new[] { "SUSUNAN PENGURUS", org, periode }, true);
            var officers = d.Officers.Select(x => new Row
            {
                C2 = string.IsNullOrWhiteSpace(x.Division) ? x.Position : $"{x.Position}\n{x.Division}",
                C3 = x.Name,
                C4 = string.IsNullOrWhiteSpace(x.Number) ? "-" : x.Number,
                C5 = string.IsNullOrWhiteSpace(x.Faculty) ? "-" : x.Faculty,
                Img = x.Photo
            }).ToList();
            if (officers.Count == 0)
            {
                officers.Add(new Row { C2 = "Belum ada data kepengurusan" });
            }
            r.Table(new[] { "FOTO", "JABATAN", "NAMA", "NIA", "FAKULTAS / JURUSAN" }, new float[] { 75, 130, 145, 100, r.W - 450 },
                new[] { C, L, L, C, L }, officers, imageColumn: 0);

            r.TitleBlock(new[] { "DOKUMENTASI SELAMA KEPENGURUSAN", periode }, true);
            r.Photos(d.Photos);
            return r;
        }

        // ================================================================= BLOK KHUSUS LPJ
        private void Cover(string[] titles, byte[]? logo, int year)
        {
            var band = new ReportHeaderBand { HeightF = 980, PageBreak = PageBreak.AfterBand };
            var y = 60f;
            foreach (var t in titles)
            {
                band.Controls.Add(Label(new RectangleF(0, y, W, 30), t, 14f, true, TextAlignment.TopCenter));
                y += 34;
            }
            if (logo != null && logo.Length > 0)
            {
                band.Controls.Add(Picture(new RectangleF((W - 260) / 2, 330, 260, 260), logo));
            }
            band.Controls.Add(Label(new RectangleF(0, 820, W, 60), INSTITUTION, 14f, true, TextAlignment.TopCenter));
            band.Controls.Add(Label(new RectangleF(0, 890, W, 30), year.ToString(), 14f, true, TextAlignment.TopCenter));
            Bands.Add(band);
        }

        // Dokumentasi: 2 foto per baris dengan keterangan.
        private void Photos(List<Lpj.PhotoLine> photos)
        {
            if (photos.Count == 0)
            {
                Paragraphs("Belum ada foto dokumentasi di galeri.", false);
                return;
            }
            var rows = new List<Row>();
            for (var i = 0; i < photos.Count; i += 2)
            {
                var second = i + 1 < photos.Count ? photos[i + 1] : null;
                rows.Add(new Row { Img = photos[i].Data, C1 = photos[i].Caption, Img2 = second?.Data, C2 = second?.Caption });
            }
            var block = Block(rows);
            var half = (W - 20) / 2;
            var detail = new DetailBand { HeightF = 250 };
            for (var k = 0; k < 2; k++)
            {
                var x = k * (half + 20);
                var pic = new XRPictureBox { BoundsF = new RectangleF(x, 6, half, 200), Sizing = ImageSizeMode.ZoomImage };
                pic.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "ImageSource", k == 0 ? "[Img]" : "[Img2]"));
                var caption = Label(new RectangleF(x, 210, half, 34), size: 10f, align: TextAlignment.TopCenter);
                caption.CanGrow = false;
                caption.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", k == 0 ? "'(' + [C1] + ')'" : "Iif(IsNullOrEmpty([C2]), '', '(' + [C2] + ')')"));
                detail.Controls.AddRange(new XRControl[] { pic, caption });
            }
            block.Bands.Add(detail);
        }

        /// <summary>Realisasi anggaran: pemasukan, pengeluaran (RAB qty × harga berdampingan dengan realisasi), lalu rekapitulasi.</summary>
        private void BudgetTables(List<Lpj.BudgetLine> lines)
        {
            var income = lines.Where(x => x.Type == CashFlowType.Income).ToList();
            var rows = income.Select((x, k) => new Row { C1 = (k + 1).ToString(), C2 = x.Description, C3 = x.Category, C4 = Rp(x.Realized) }).ToList();
            if (rows.Count == 0)
            {
                rows.Add(new Row { C2 = "Tidak ada pemasukan tercatat" });
            }
            rows.Add(new Row { C2 = "JUMLAH PEMASUKAN", C4 = Rp(income.Sum(x => x.Realized)), Bold = true, Shade = true });
            Table(new[] { "NO", "URAIAN", "SUMBER", "JUMLAH" }, new float[] { 35, W - 305, 140, 130 }, new[] { C, L, L, R }, rows, heading: "PEMASUKAN");

            // Kolom angka pengeluaran tanpa "Rp" (sudah di judul kolom) agar muat.
            var expense = lines.Where(x => x.Type == CashFlowType.Expense).ToList();
            rows = new List<Row>();
            var c = 0;
            foreach (var category in expense.GroupBy(x => x.Category))
            {
                rows.Add(new Row { C1 = (++c).ToString(), C2 = category.Key.ToUpper(ID), Bold = true, Shade = true });
                rows.AddRange(category.Select(x => new Row
                {
                    C2 = x.Description,
                    C3 = x.Detail ?? "-",
                    C4 = x.FromBudget ? Num(x.Budget) : "-",
                    C5 = Num(x.Realized),
                    C6 = x.FromBudget ? Num(x.Budget - x.Realized) : "-"
                }));
                rows.Add(new Row
                {
                    C2 = "Jumlah " + category.Key,
                    C4 = Num(category.Sum(x => x.Budget)),
                    C5 = Num(category.Sum(x => x.Realized)),
                    C6 = Num(category.Where(x => x.FromBudget).Sum(x => x.Budget - x.Realized)),
                    Bold = true
                });
            }
            if (rows.Count == 0)
            {
                rows.Add(new Row { C2 = "Tidak ada pengeluaran tercatat" });
            }
            rows.Add(new Row
            {
                C2 = "TOTAL PENGELUARAN",
                C4 = Num(expense.Sum(x => x.Budget)),
                C5 = Num(expense.Sum(x => x.Realized)),
                C6 = Num(expense.Where(x => x.FromBudget).Sum(x => x.Budget - x.Realized)),
                Bold = true,
                Shade = true
            });
            Table(new[] { "NO", "URAIAN", "RINCIAN RAB", "ANGGARAN (Rp)", "REALISASI (Rp)", "SELISIH (Rp)" }, new float[] { 34, W - 439, 120, 95, 95, 95 },
                new[] { C, L, L, R, R, R }, rows, heading: "PENGELUARAN", fontSize: 10f);

            var totalIncome = income.Sum(x => x.Realized);
            var totalExpense = expense.Sum(x => x.Realized);
            var balance = totalIncome - totalExpense;
            Table(null, new float[] { W - 180, 180 }, new[] { L, R }, new List<Row>
            {
                new() { C1 = "Total Pemasukan", C2 = Rp(totalIncome) },
                new() { C1 = "Total Pengeluaran (realisasi)", C2 = Rp(totalExpense) },
                new() { C1 = balance < 0 ? "KEKURANGAN" : "SISA DANA", C2 = Rp(Math.Abs(balance)), Bold = true, Shade = true }
            }, heading: "REKAPITULASI");
        }

        // ================================================================= TEKS
        private static List<Signer> Signers(List<Lpj.SignerLine> lines) => lines.Select(x => new Signer(x.Role, x.Name, x.Number)).ToList();

        // Nama periode yang sudah diawali "Periode" tidak diberi awalan lagi.
        private static string Periode(string name)
        {
            var upper = name.Trim().ToUpper(ID);
            return upper.StartsWith("PERIODE") ? upper : "PERIODE " + upper;
        }

        private static string ProgramText(ProgramStatus status) => status switch
        {
            ProgramStatus.Done => "Terealisasi",
            ProgramStatus.Cancelled => "Tidak Terealisasi",
            ProgramStatus.Ongoing => "Berjalan",
            _ => "Belum Terealisasi"
        };

        private static string ActivityText(ActivityStatus status) => status switch
        {
            ActivityStatus.Done => "Terlaksana",
            ActivityStatus.Cancelled => "Tidak Terlaksana",
            ActivityStatus.Ongoing => "Berlangsung",
            _ => "Belum Terlaksana"
        };
    }
}
