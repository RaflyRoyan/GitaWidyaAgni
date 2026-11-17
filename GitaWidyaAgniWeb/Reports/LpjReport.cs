using DevExpress.Drawing;
using DevExpress.Drawing.Printing;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using GWALib.Lib.Transaksi.Program;
using GWAShared.Enums;
using System.Drawing;
using System.Globalization;

namespace GitaWidyaAgniWeb.Reports
{
    /// <summary>
    /// LPJ Kegiatan / Kepengurusan (DevExpress Reports, dibuat lewat kode), mengikuti template LPJ MB Gita Widya Agni:
    /// sampul, bab bernomor (narasi dari t_lpj + data modul), tanda tangan, lalu lampiran (dokumentasi, realisasi anggaran /
    /// inventaris, susunan pengurus). Setiap blok adalah DetailReportBand dengan sumber data List&lt;Row&gt;, sehingga paragraf
    /// dan baris tabel dipotong rapi antar halaman; judul bab adalah GroupHeader WithFirstDetail agar tidak terpisah dari isinya.
    /// Satuan bawaan report 1/100 inci (A4 = 827 × 1169).
    /// </summary>
    public class LpjReport : XtraReport
    {
        /// <summary>Baris generik untuk semua blok (paragraf, tabel, foto).</summary>
        public class Row
        {
            public string? C1 { get; set; }
            public string? C2 { get; set; }
            public string? C3 { get; set; }
            public string? C4 { get; set; }
            public string? C5 { get; set; }
            public string? C6 { get; set; }
            public bool Bold { get; set; }
            public bool Shade { get; set; }
            public bool Bullet { get; set; }
            public byte[]? Img { get; set; }
            public byte[]? Img2 { get; set; }
        }

        public const string INSTITUTION = "UNIVERSITAS PEMBANGUNAN NASIONAL “VETERAN”\nJAWA TIMUR";
        private const float W = 611;
        private const string FONT = "Times New Roman";
        private const float SIZE = 12f;
        private const string INDENT = "  ";
        private static readonly Color SHADE = Color.FromArgb(242, 232, 207);
        private static readonly CultureInfo ID = new("id-ID");
        private static readonly TextAlignment L = TextAlignment.TopLeft;
        private static readonly TextAlignment C = TextAlignment.TopCenter;
        private static readonly TextAlignment R = TextAlignment.TopRight;
        private int _level;

        private LpjReport(string title)
        {
            DisplayName = title;
            ExportOptions.PrintPreview.DefaultFileName = title;
            ExportOptions.Pdf.DocumentOptions.Title = title;
            PaperKind = DXPaperKind.A4;
            Margins = new DXMargins(118, 98, 98, 79);
            Font = new DXFont(FONT, SIZE);

            Bands.Add(new TopMarginBand { HeightF = 98 });
            Bands.Add(new BottomMarginBand { HeightF = 79 });
            Bands.Add(new DetailBand { HeightF = 0 });
            var footer = new PageFooterBand { HeightF = 30, PrintOn = PrintOnPages.NotWithReportHeader };
            footer.Controls.Add(new XRPageInfo
            {
                BoundsF = new RectangleF(0, 8, W, 20),
                PageInfo = PageInfo.Number,
                TextAlignment = TextAlignment.MiddleCenter,
                Font = new DXFont(FONT, 10f)
            });
            Bands.Add(footer);
        }

        // ================================================================= LPJ KEGIATAN
        public static LpjReport Kegiatan(Lpj.KegiatanData d, byte[]? logo, string place)
        {
            var lpj = d.Lpj;
            var r = new LpjReport("LPJ " + lpj.Title);
            var org = d.Org.Name.ToUpper(ID);
            r.Cover(new[] { "LAPORAN PERTANGGUNGJAWABAN", lpj.Title.ToUpper(ID), org }, logo, d.Year);

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
                var rows = new List<Row>
                {
                    new() { C2 = "Tanggal", C3 = ":", C4 = s.Date },
                    new() { C2 = "Hari", C3 = ":", C4 = s.Day },
                    new() { C2 = "Jam", C3 = ":", C4 = s.Time },
                    new() { C2 = "Tempat", C3 = ":", C4 = string.IsNullOrWhiteSpace(s.Location) ? "-" : s.Location }
                };
                var sub = $"{INDENT}{(char)('a' + i % 26)}. {s.Name}";
                r.Table(null, new float[] { 48, 100, 15, W - 163 }, new[] { L, L, C, L }, rows, borders: false,
                    heading: i == 0 ? title + "\n" + sub : sub);
            }

            var results = d.Results.Count == 0 ? null : string.Join("\n", d.Results.Select((x, k) => $"{k + 1}. {x}"));
            r.Paragraphs(results, true, $"{++n}. HASIL KEGIATAN");
            r.Section(++n, "DOKUMENTASI", "Terlampir");
            r.Section(++n, "REALISASI ANGGARAN", "Terlampir");
            r.Section(++n, "PENUTUP", lpj.Closing);
            r.Signatures(place, lpj.FinalizedAt ?? DateTime.Today, d.Signers);

            // Lampiran
            r.TitleBlock(new[] { "DOKUMENTASI", lpj.Title.ToUpper(ID) }, true);
            r.Photos(d.Photos);

            r.TitleBlock(new[] { "REALISASI ANGGARAN", lpj.Title.ToUpper(ID), org }, true);
            r.BudgetTables(d.Budget);
            return r;
        }

        // ================================================================= LPJ KEPENGURUSAN
        public static LpjReport Kepengurusan(Lpj.KepengurusanData d, byte[]? logo, string place)
        {
            var lpj = d.Lpj;
            var r = new LpjReport("LPJ " + lpj.Title);
            var org = d.Org.Name.ToUpper(ID);
            var periode = Periode(d.PeriodName);
            r.Cover(new[] { "LAPORAN PERTANGGUNGJAWABAN", "KEPENGURUSAN " + org, periode }, logo, d.EndDate.Year);

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
            r.Table(new[] { "NO", "PROGRAM KERJA", "KETERANGAN" }, new float[] { 40, W - 200, 160 }, new[] { C, L, C }, rows,
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
            r.Table(new[] { "NO", "NAMA KEGIATAN", "TANGGAL", "KETERANGAN" }, new float[] { 40, W - 320, 120, 160 }, new[] { C, L, C, C }, activities);

            r.Section(++n, "DATA INVENTARIS", "Terlampir");
            r.Section(++n, "SUSUNAN PENGURUS " + periode, "Terlampir");
            r.Section(++n, "DOKUMENTASI SELAMA KEPENGURUSAN", "Terlampir");
            r.Section(++n, "PENUTUP", lpj.Closing);
            r.Signatures(place, lpj.FinalizedAt ?? DateTime.Today, d.Signers);

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
            r.Table(new[] { "NO", "NAMA ALAT", "JUMLAH", "KETERANGAN" }, new float[] { 40, W - 340, 100, 200 }, new[] { C, L, C, L }, assets);

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
            r.Table(new[] { "FOTO", "JABATAN", "NAMA", "NIA", "FAKULTAS / JURUSAN" }, new float[] { 75, 130, 145, 100, W - 450 },
                new[] { C, L, L, C, L }, officers, imageColumn: 0);

            r.TitleBlock(new[] { "DOKUMENTASI SELAMA KEPENGURUSAN", periode }, true);
            r.Photos(d.Photos);
            return r;
        }

        // ================================================================= BLOK
        private DetailReportBand Block(List<Row> rows)
        {
            var block = new DetailReportBand { Level = _level++, DataSource = rows };
            Bands.Add(block);
            return block;
        }

        private static XRLabel Label(RectangleF bounds, string? text = null, float size = SIZE, bool bold = false, TextAlignment? align = null)
        {
            return new XRLabel
            {
                BoundsF = bounds,
                Text = text ?? "",
                Font = new DXFont(FONT, size, bold ? DXFontStyle.Bold : DXFontStyle.Regular),
                TextAlignment = align ?? TextAlignment.TopLeft,
                WordWrap = true,
                Multiline = true,
                CanGrow = true,
                CanShrink = false,
                Padding = new PaddingInfo(0, 0, 0, 0)
            };
        }

        // Judul bab di atas blok: tercetak bersama baris pertama (tidak tertinggal sendirian di bawah halaman).
        // Di atas tabel memakai ReportHeader: dua GroupHeader tanpa field tercetak dengan judul kolom di atas judul bab.
        private static void AddHeading(DetailReportBand block, string? heading, bool tableHeading)
        {
            if (heading == null)
            {
                return;
            }
            var lines = heading.Split('\n').Length;
            var label = Label(new RectangleF(0, 10, W, lines * 20), heading, SIZE, true);
            Band band = tableHeading
                ? new ReportHeaderBand { HeightF = 12 + lines * 20 }
                : new GroupHeaderBand { HeightF = 12 + lines * 20, GroupUnion = GroupUnion.WithFirstDetail };
            band.Controls.Add(label);
            block.Bands.Add(band);
        }

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
                band.Controls.Add(new XRPictureBox
                {
                    BoundsF = new RectangleF((W - 260) / 2, 330, 260, 260),
                    ImageSource = new ImageSource(DXImage.FromStream(new MemoryStream(logo))),
                    Sizing = ImageSizeMode.ZoomImage
                });
            }
            band.Controls.Add(Label(new RectangleF(0, 820, W, 60), INSTITUTION, 14f, true, TextAlignment.TopCenter));
            band.Controls.Add(Label(new RectangleF(0, 890, W, 30), year.ToString(), 14f, true, TextAlignment.TopCenter));
            Bands.Add(band);
        }

        // Judul halaman (tengah, tebal); pageBreak = mulai di halaman baru (lampiran).
        private void TitleBlock(string[] lines, bool pageBreak)
        {
            var block = Block(new List<Row> { new() });
            var header = new ReportHeaderBand { HeightF = lines.Length * 22 + 20, PageBreak = pageBreak ? PageBreak.BeforeBand : PageBreak.None };
            header.Controls.Add(Label(new RectangleF(0, 0, W, lines.Length * 22), string.Join("\n", lines), SIZE, true, TextAlignment.TopCenter));
            block.Bands.Add(header);
            block.Bands.Add(new DetailBand { HeightF = 0 });
        }

        private void Spacer()
        {
            var block = Block(new List<Row> { new() });
            block.Bands.Add(new DetailBand { HeightF = 14 });
        }

        private void Section(int number, string title, string? text)
        {
            Paragraphs(text, true, $"{number}. {title}");
        }

        /// <summary>Teks naratif: baris "- " / "* " / "• " jadi poin, baris lain jadi paragraf (indentasi baris pertama).</summary>
        private void Paragraphs(string? text, bool indent, string? heading = null)
        {
            var rows = new List<Row>();
            foreach (var raw in (text ?? "").Replace("\r", "").Split('\n'))
            {
                var line = raw.Trim();
                if (line.Length == 0)
                {
                    continue;
                }
                if (line.StartsWith("- ") || line.StartsWith("* ") || line.StartsWith("• "))
                {
                    rows.Add(new Row { C1 = line[2..].Trim(), Bullet = true });
                }
                else
                {
                    rows.Add(new Row { C1 = (indent ? INDENT : "") + line });
                }
            }
            if (rows.Count == 0)
            {
                rows.Add(new Row { C1 = (indent ? INDENT : "") + "-" });
            }

            var block = Block(rows);
            AddHeading(block, heading, false);
            var detail = new DetailBand { HeightF = 24 };
            var para = Label(new RectangleF(30, 0, W - 30, 20), align: TextAlignment.TopJustify);
            para.Padding = new PaddingInfo(0, 0, 2, 4);
            para.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[C1]"));
            para.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Visible", "Not [Bullet]"));
            var dot = Label(new RectangleF(48, 0, 16, 20), "•");
            dot.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Visible", "[Bullet]"));
            var bullet = Label(new RectangleF(66, 0, W - 66, 20), align: TextAlignment.TopJustify);
            bullet.Padding = new PaddingInfo(0, 0, 2, 4);
            bullet.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[C1]"));
            bullet.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Visible", "[Bullet]"));
            detail.Controls.AddRange(new XRControl[] { para, dot, bullet });
            block.Bands.Add(detail);
        }

        /// <summary>
        /// Tabel dengan kolom C1..C6. headers null = tanpa baris judul kolom (judul kolom diulang tiap halaman).
        /// imageColumn = indeks kolom berisi foto [Img]. heading = judul bab di atas tabel.
        /// </summary>
        private void Table(string[]? headers, float[] widths, TextAlignment[] aligns, List<Row> rows, bool borders = true,
            int imageColumn = -1, string? heading = null, float fontSize = 11f)
        {
            var block = Block(rows);
            AddHeading(block, heading, true);
            var rowHeight = imageColumn >= 0 ? 90f : 22f;
            if (headers != null)
            {
                var head = new GroupHeaderBand { HeightF = 26, RepeatEveryPage = true, GroupUnion = GroupUnion.WithFirstDetail };
                head.Controls.Add(NewTable(widths, aligns, borders, 26, fontSize, (cell, i) =>
                {
                    cell.Text = headers[i];
                    cell.Font = new DXFont(FONT, fontSize, DXFontStyle.Bold);
                    cell.BackColor = SHADE;
                    cell.TextAlignment = TextAlignment.MiddleCenter;
                }));
                block.Bands.Add(head);
            }
            var detail = new DetailBand { HeightF = rowHeight };
            detail.Controls.Add(NewTable(widths, aligns, borders, rowHeight, fontSize, (cell, i) =>
            {
                if (i == imageColumn)
                {
                    cell.CanGrow = false;
                    var pic = new XRPictureBox
                    {
                        BoundsF = new RectangleF(4, 4, widths[i] - 8, rowHeight - 8),
                        Sizing = ImageSizeMode.ZoomImage,
                        Borders = BorderSide.None
                    };
                    pic.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "ImageSource", "[Img]"));
                    cell.Controls.Add(pic);
                    return;
                }
                cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", $"[C{i + 1}]"));
                cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Font.Bold", "[Bold]"));
                if (borders)
                {
                    cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "BackColor", $"Iif([Shade], '{ColorTranslator.ToHtml(SHADE)}', 'White')"));
                }
            }));
            block.Bands.Add(detail);
        }

        private static XRTable NewTable(float[] widths, TextAlignment[] aligns, bool borders, float height, float fontSize, Action<XRTableCell, int> setup)
        {
            var table = new XRTable
            {
                BoundsF = new RectangleF(0, 0, widths.Sum(), height),
                Borders = borders ? BorderSide.All : BorderSide.None,
                BorderColor = Color.Black,
                BorderWidth = 1,
                Font = new DXFont(FONT, fontSize)
            };
            table.BeginInit();
            var row = new XRTableRow { HeightF = height };
            for (var i = 0; i < widths.Length; i++)
            {
                var cell = new XRTableCell
                {
                    WidthF = widths[i],
                    Weight = widths[i],
                    TextAlignment = aligns[i],
                    WordWrap = true,
                    Multiline = true,
                    CanGrow = true,
                    Padding = new PaddingInfo(borders ? 5 : 0, 5, 3, 3)
                };
                setup(cell, i);
                row.Cells.Add(cell);
            }
            table.Rows.Add(row);
            table.EndInit();
            return table;
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

        /// <summary>Tanda tangan: penanda tangan selain yang terakhir sejajar; yang terakhir di bawah dengan "Mengetahui,".</summary>
        private void Signatures(string place, DateTime date, List<Lpj.SignerLine> signers)
        {
            var block = Block(new List<Row> { new() });
            var detail = new DetailBand { HeightF = 400, KeepTogether = true };
            detail.Controls.Add(Label(new RectangleF(W / 2, 30, W / 2, 20), $"{place}, {date.ToString("d MMMM yyyy", ID)}", align: TextAlignment.TopCenter));
            var top = signers.Count > 1 ? signers.Take(signers.Count - 1).ToList() : signers;
            var colWidth = W / Math.Max(top.Count, 1);
            for (var i = 0; i < top.Count; i++)
            {
                Signer(detail, top[i], i * colWidth, 60, colWidth, null);
            }
            if (signers.Count > 1)
            {
                Signer(detail, signers[^1], (W - 260) / 2, 240, 260, "Mengetahui,");
            }
            block.Bands.Add(detail);
        }

        private static void Signer(Band band, Lpj.SignerLine s, float x, float y, float width, string? caption)
        {
            var role = caption == null ? s.Role : caption + "\n" + s.Role;
            band.Controls.Add(Label(new RectangleF(x, y, width, 40), role, align: TextAlignment.TopCenter));
            var name = Label(new RectangleF(x, y + 110, width, 20), string.IsNullOrWhiteSpace(s.Name) ? "(..............................)" : s.Name, SIZE, true, TextAlignment.TopCenter);
            if (!string.IsNullOrWhiteSpace(s.Name))
            {
                name.Font = new DXFont(FONT, SIZE, DXFontStyle.Bold | DXFontStyle.Underline);
            }
            band.Controls.Add(name);
            if (!string.IsNullOrWhiteSpace(s.Number))
            {
                band.Controls.Add(Label(new RectangleF(x, y + 132, width, 20), "NIA. " + s.Number, align: TextAlignment.TopCenter));
            }
        }

        // ================================================================= TEKS
        private static string Rp(decimal value) => (value < 0 ? "-Rp " : "Rp ") + Math.Abs(value).ToString("N0", ID);
        private static string Num(decimal value) => value.ToString("N0", ID);

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
