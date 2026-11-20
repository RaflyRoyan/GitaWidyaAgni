using DevExpress.Drawing;
using DevExpress.Drawing.Printing;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using System.Drawing;
using System.Globalization;

namespace GitaWidyaAgniWeb.Reports
{
    /// <summary>
    /// Dasar laporan dokumen GWA (DevExpress, dibuat lewat kode): A4, Times New Roman 12, nomor halaman di kaki.
    /// Isi disusun dari blok berurutan; setiap blok adalah DetailReportBand dengan sumber data List&lt;Row&gt;, sehingga
    /// paragraf dan baris tabel terpotong rapi antar halaman. Satuan bawaan 1/100 inci (A4 = 827 × 1169, landscape 1169 × 827).
    /// Dipakai LpjReport dan laporan Persuratan.
    /// </summary>
    public abstract class DocumentReport : XtraReport
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
            public string? C7 { get; set; }
            public string? C8 { get; set; }
            public bool Bold { get; set; }
            public bool Shade { get; set; }
            public bool Bullet { get; set; }
            public byte[]? Img { get; set; }
            public byte[]? Img2 { get; set; }
        }

        /// <summary>Penanda tangan; Signature = gambar tanda tangan (opsional); Wet = selalu tanda tangan basah (tanpa QR, mis. pihak luar).</summary>
        public record Signer(string Role, string? Name, string? Number, byte[]? Signature = null, bool Wet = false)
        {
            /// <summary>URL QR tanda tangan elektronik (diisi Signatures bila qrType diberikan); null = tanda tangan basah.</summary>
            public string? QrUrl { get; init; }
        }

        /// <summary>Keterangan dokumen untuk QR tanda tangan (nomor, filter, periode); tampil di halaman /ttd.</summary>
        protected string? QrReference { get; set; }

        protected const string FONT = "Times New Roman";
        protected const float SIZE = 12f;
        protected const string INDENT = "  ";
        protected static readonly Color SHADE = Color.FromArgb(242, 232, 207);
        protected static readonly CultureInfo ID = new("id-ID");
        protected static readonly TextAlignment L = TextAlignment.TopLeft;
        protected static readonly TextAlignment C = TextAlignment.TopCenter;
        protected static readonly TextAlignment R = TextAlignment.TopRight;

        /// <summary>Lebar area isi (lebar kertas dikurangi margin kiri/kanan).</summary>
        protected readonly float W;
        private int _level;

        protected DocumentReport(string title, bool landscape = false, bool totalPages = false)
        {
            DisplayName = title;
            ExportOptions.PrintPreview.DefaultFileName = title;
            ExportOptions.Pdf.DocumentOptions.Title = title;
            PaperKind = DXPaperKind.A4;
            Landscape = landscape;
            Margins = landscape ? new DXMargins(79, 79, 79, 79) : new DXMargins(118, 98, 98, 79);
            W = (landscape ? 1169 : 827) - Margins.Left - Margins.Right;
            Font = new DXFont(FONT, SIZE);

            Bands.Add(new TopMarginBand { HeightF = Margins.Top });
            Bands.Add(new BottomMarginBand { HeightF = Margins.Bottom });
            Bands.Add(new DetailBand { HeightF = 0 });
            var footer = new PageFooterBand { HeightF = 30, PrintOn = PrintOnPages.NotWithReportHeader };
            footer.Controls.Add(new XRPageInfo
            {
                BoundsF = new RectangleF(0, 8, W, 20),
                PageInfo = totalPages ? PageInfo.NumberOfTotal : PageInfo.Number,
                TextFormatString = totalPages ? "Halaman {0} dari {1}" : "{0}",
                TextAlignment = TextAlignment.MiddleCenter,
                Font = new DXFont(FONT, 10f)
            });
            Bands.Add(footer);
        }

        // ================================================================= DASAR
        protected DetailReportBand Block(List<Row> rows)
        {
            var block = new DetailReportBand { Level = _level++, DataSource = rows };
            Bands.Add(block);
            return block;
        }

        protected static XRLabel Label(RectangleF bounds, string? text = null, float size = SIZE, bool bold = false, TextAlignment? align = null)
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

        protected static XRPictureBox Picture(RectangleF bounds, byte[] data)
        {
            return new XRPictureBox
            {
                BoundsF = bounds,
                ImageSource = new ImageSource(DXImage.FromStream(new MemoryStream(data))),
                Sizing = ImageSizeMode.ZoomImage
            };
        }

        // Judul bab di atas blok: tercetak bersama baris pertama (tidak tertinggal sendirian di bawah halaman).
        // Di atas tabel memakai ReportHeader: dua GroupHeader tanpa field tercetak dengan judul kolom di atas judul bab.
        private void AddHeading(DetailReportBand block, string? heading, bool tableHeading)
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

        // ================================================================= BLOK
        /// <summary>
        /// Kop surat: logo kiri, nama organisasi, institusi, alamat dan kontak, lalu garis ganda.
        /// Hanya di halaman pertama (ReportHeader).
        /// </summary>
        protected void Letterhead(ReportBrand brand)
        {
            var band = new ReportHeaderBand { HeightF = 118 };
            if (brand.Logo != null && brand.Logo.Length > 0)
            {
                band.Controls.Add(Picture(new RectangleF(0, 0, 92, 92), brand.Logo));
            }
            var x = 100f;
            var width = W - x;
            band.Controls.Add(Label(new RectangleF(x, 0, width, 26), brand.OrgName.ToUpper(ID), 16f, true, TextAlignment.TopCenter));
            band.Controls.Add(Label(new RectangleF(x, 28, width, 20), ReportBrand.INSTITUTION_LINE, 11f, true, TextAlignment.TopCenter));
            var info = string.Join("\n", new[] { brand.Address, brand.Contact }.Where(s => !string.IsNullOrWhiteSpace(s)));
            if (info.Length > 0)
            {
                var detail = Label(new RectangleF(x, 50, width, 40), info, 9.5f, false, TextAlignment.TopCenter);
                detail.CanGrow = false;
                band.Controls.Add(detail);
            }
            band.Controls.Add(new XRLine { BoundsF = new RectangleF(0, 98, W, 6), LineWidth = 3 });
            band.Controls.Add(new XRLine { BoundsF = new RectangleF(0, 104, W, 4), LineWidth = 1 });
            Bands.Add(band);
        }

        /// <summary>Judul halaman (tengah, tebal); pageBreak = mulai di halaman baru.</summary>
        protected void TitleBlock(string[] lines, bool pageBreak, bool underlineFirst = false)
        {
            var block = Block(new List<Row> { new() });
            var header = new ReportHeaderBand { HeightF = lines.Length * 22 + 20, PageBreak = pageBreak ? PageBreak.BeforeBand : PageBreak.None };
            if (underlineFirst && lines.Length > 0)
            {
                var first = Label(new RectangleF(0, 0, W, 22), lines[0], 14f, true, TextAlignment.TopCenter);
                first.Font = new DXFont(FONT, 14f, DXFontStyle.Bold | DXFontStyle.Underline);
                header.Controls.Add(first);
                if (lines.Length > 1)
                {
                    header.Controls.Add(Label(new RectangleF(0, 24, W, (lines.Length - 1) * 22), string.Join("\n", lines.Skip(1)), SIZE, false, TextAlignment.TopCenter));
                }
            }
            else
            {
                header.Controls.Add(Label(new RectangleF(0, 0, W, lines.Length * 22), string.Join("\n", lines), SIZE, true, TextAlignment.TopCenter));
            }
            block.Bands.Add(header);
            block.Bands.Add(new DetailBand { HeightF = 0 });
        }

        protected void Spacer(float height = 14)
        {
            var block = Block(new List<Row> { new() });
            block.Bands.Add(new DetailBand { HeightF = height });
        }

        protected void Section(int number, string title, string? text)
        {
            Paragraphs(text, true, $"{number}. {title}");
        }

        /// <summary>
        /// Teks naratif: baris "- " / "* " / "• " jadi poin, baris lain jadi paragraf (indentasi baris pertama bila indent).
        /// left = jarak kiri isi (judul bab tetap di kiri).
        /// </summary>
        protected void Paragraphs(string? text, bool indent, string? heading = null, float left = 30)
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
            var para = Label(new RectangleF(left, 0, W - left, 20), align: TextAlignment.TopJustify);
            para.Padding = new PaddingInfo(0, 0, 2, 4);
            para.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[C1]"));
            para.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Visible", "Not [Bullet]"));
            var dot = Label(new RectangleF(left + 18, 0, 16, 20), "•");
            dot.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Visible", "[Bullet]"));
            var bullet = Label(new RectangleF(left + 36, 0, W - left - 36, 20), align: TextAlignment.TopJustify);
            bullet.Padding = new PaddingInfo(0, 0, 2, 4);
            bullet.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[C1]"));
            bullet.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Visible", "[Bullet]"));
            detail.Controls.AddRange(new XRControl[] { para, dot, bullet });
            block.Bands.Add(detail);
        }

        /// <summary>Isi HTML (mis. badan surat dari template) lewat XRRichText; terpotong antar halaman bila panjang.</summary>
        protected void Html(string html)
        {
            var block = Block(new List<Row> { new() });
            var detail = new DetailBand { HeightF = 30 };
            var rich = new XRRichText
            {
                BoundsF = new RectangleF(0, 0, W, 30),
                CanGrow = true,
                CanShrink = false
            };
            rich.Html = $"<div style=\"font-family:'{FONT}';font-size:12pt;text-align:justify;line-height:1.35\">{html}</div>";
            detail.Controls.Add(rich);
            block.Bands.Add(detail);
        }

        /// <summary>
        /// Tabel dengan kolom C1..C8. headers null = tanpa baris judul kolom (judul kolom diulang tiap halaman).
        /// imageColumn = indeks kolom berisi foto [Img]. heading = judul bab di atas tabel.
        /// </summary>
        protected void Table(string[]? headers, float[] widths, TextAlignment[] aligns, List<Row> rows, bool borders = true,
            int imageColumn = -1, string? heading = null, float fontSize = 11f, float rowHeight = 22f)
        {
            var block = Block(rows);
            AddHeading(block, heading, true);
            if (imageColumn >= 0)
            {
                rowHeight = Math.Max(rowHeight, 90f);
            }
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

        /// <summary>Daftar "label : nilai" tanpa garis (nomor surat, tanggal, dsb.).</summary>
        protected void Fields(IEnumerable<(string Label, string? Value)> fields, float labelWidth = 110, bool boldValue = false, float left = 0, string? heading = null)
        {
            var rows = fields.Select(x => new Row { C1 = x.Label, C2 = ":", C3 = string.IsNullOrWhiteSpace(x.Value) ? "-" : x.Value, Bold = boldValue }).ToList();
            var widths = left > 0 ? new float[] { left, labelWidth, 15, W - left - labelWidth - 15 } : new float[] { labelWidth, 15, W - labelWidth - 15 };
            if (left > 0)
            {
                rows = rows.Select(x => new Row { C2 = x.C1, C3 = x.C2, C4 = x.C3, Bold = x.Bold }).ToList();
            }
            var aligns = left > 0 ? new[] { L, L, C, L } : new[] { L, C, L };
            Table(null, widths, aligns, rows, borders: false, heading: heading, fontSize: SIZE, rowHeight: 20);
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
                    Padding = new PaddingInfo(borders ? 5 : 0, 5, borders ? 3 : 1, borders ? 3 : 1)
                };
                setup(cell, i);
                row.Cells.Add(cell);
            }
            table.Rows.Add(row);
            table.EndInit();
            return table;
        }

        /// <summary>
        /// Tanda tangan dengan baris tempat + tanggal di kanan.
        /// knownLast = penanda tangan terakhir di bawah tengah dengan "Mengetahui," (LPJ); selain itu semua sejajar
        /// (satu penanda tangan di kanan). Gambar tanda tangan dipasang di atas nama bila ada.
        /// </summary>
        /// gap = jarak kosong di atas baris tempat/tanggal (lebih kecil untuk dokumen satu halaman yang padat, mis. invoice).
        /// qrType = kode jenis dokumen (TandaTanganQr.Jenis): penanda tangan bernama (bukan Wet) mendapat QR tanda tangan elektronik
        /// di tempat tanda tangan basah; null = tanda tangan basah (mis. LPJ).
        protected void Signatures(string? place, DateTime? date, List<Signer> signers, bool knownLast, float gap = 30, string? qrType = null)
        {
            if (qrType != null)
            {
                var urls = TandaTanganQr.Terbitkan(qrType, DisplayName, QrReference, DateOnly.FromDateTime(date ?? DateTime.Today), signers);
                signers = signers.Select((s, i) => urls[i] == null ? s : s with { QrUrl = urls[i] }).ToList();
            }
            var block = Block(new List<Row> { new() });
            var top = knownLast && signers.Count > 1 ? signers.Take(signers.Count - 1).ToList() : signers;
            var height = (knownLast && signers.Count > 1 ? 400 : 230) - 30 + gap;
            var detail = new DetailBand { HeightF = height, KeepTogether = true };
            // place null = tanpa baris tempat/tanggal (surat: tanggal sudah di kepala surat).
            if (place != null)
            {
                var dateText = date == null ? $"{place}, ...................." : $"{place}, {date.Value.ToString("d MMMM yyyy", ID)}";
                detail.Controls.Add(Label(new RectangleF(W / 2, gap, W / 2, 20), dateText, align: TextAlignment.TopCenter));
            }
            if (top.Count == 1)
            {
                SignerBlock(detail, top[0], W / 2, gap + 30, W / 2, null);
            }
            else
            {
                var colWidth = W / Math.Max(top.Count, 1);
                for (var i = 0; i < top.Count; i++)
                {
                    SignerBlock(detail, top[i], i * colWidth, gap + 30, colWidth, null);
                }
            }
            if (knownLast && signers.Count > 1)
            {
                SignerBlock(detail, signers[^1], (W - 260) / 2, gap + 210, 260, "Mengetahui,");
            }
            block.Bands.Add(detail);
        }

        private static void SignerBlock(Band band, Signer s, float x, float y, float width, string? caption)
        {
            var role = caption == null ? s.Role : caption + "\n" + s.Role;
            band.Controls.Add(Label(new RectangleF(x, y, width, 40), role, align: TextAlignment.TopCenter));
            if (s.QrUrl != null)
            {
                // QR tanda tangan elektronik menggantikan tanda tangan basah / gambar tanda tangan.
                band.Controls.Add(new XRBarCode
                {
                    BoundsF = new RectangleF(x + (width - 64) / 2, y + 43, 64, 64),
                    Symbology = new DevExpress.XtraPrinting.BarCode.QRCodeGenerator
                    {
                        CompactionMode = DevExpress.XtraPrinting.BarCode.QRCodeCompactionMode.Byte,
                        ErrorCorrectionLevel = DevExpress.XtraPrinting.BarCode.QRCodeErrorCorrectionLevel.M,
                        Version = DevExpress.XtraPrinting.BarCode.QRCodeVersion.AutoVersion
                    },
                    Text = s.QrUrl,
                    ShowText = false,
                    AutoModule = true,
                    Padding = new PaddingInfo(0, 0, 0, 0)
                });
            }
            else if (s.Signature != null && s.Signature.Length > 0)
            {
                band.Controls.Add(Picture(new RectangleF(x + (width - 140) / 2, y + 42, 140, 64), s.Signature));
            }
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
            if (s.QrUrl != null)
            {
                var note = Label(new RectangleF(x, y + (string.IsNullOrWhiteSpace(s.Number) ? 132 : 152), width, 14), "Ditandatangani secara elektronik", 7.5f, false, TextAlignment.TopCenter);
                note.ForeColor = Color.FromArgb(95, 88, 73);
                band.Controls.Add(note);
            }
        }

        /// <summary>Tanda air miring di setiap halaman (mis. DRAFT untuk surat yang belum disetujui).</summary>
        protected void Stamp(string text)
        {
            Watermark.Text = text;
            Watermark.TextDirection = DirectionMode.ForwardDiagonal;
            Watermark.Font = new DXFont("Arial", 96f, DXFontStyle.Bold);
            Watermark.ForeColor = Color.FromArgb(200, 32, 42);
            Watermark.TextTransparency = 200;
            Watermark.ShowBehind = false;
        }

        // ================================================================= TEKS
        protected static string Rp(decimal value) => (value < 0 ? "-Rp " : "Rp ") + Math.Abs(value).ToString("N0", ID);
        protected static string Num(decimal value) => value.ToString("N0", ID);
        protected static string Date(DateOnly? value) => value == null ? "-" : value.Value.ToString("d MMMM yyyy", ID);
        protected static string Date(DateTime? value) => value == null ? "-" : value.Value.ToString("d MMMM yyyy", ID);

        /// <summary>Angka (dibulatkan ke rupiah) dalam kata bahasa Indonesia, mis. 150000 → "seratus lima puluh ribu".</summary>
        protected static string Terbilang(decimal value)
        {
            var n = (long)Math.Round(Math.Abs(value), MidpointRounding.AwayFromZero);
            var text = n == 0 ? "nol" : string.Join(" ", Kata(n).Split(' ', StringSplitOptions.RemoveEmptyEntries));
            return value < 0 ? "minus " + text : text;
        }

        private static readonly string[] Satuan = { "", "satu", "dua", "tiga", "empat", "lima", "enam", "tujuh", "delapan", "sembilan", "sepuluh", "sebelas" };

        private static string Kata(long n)
        {
            return n switch
            {
                < 12 => Satuan[n],
                < 20 => Kata(n - 10) + " belas",
                < 100 => Kata(n / 10) + " puluh " + Kata(n % 10),
                < 200 => "seratus " + Kata(n - 100),
                < 1000 => Kata(n / 100) + " ratus " + Kata(n % 100),
                < 2000 => "seribu " + Kata(n - 1000),
                < 1_000_000 => Kata(n / 1000) + " ribu " + Kata(n % 1000),
                < 1_000_000_000 => Kata(n / 1_000_000) + " juta " + Kata(n % 1_000_000),
                < 1_000_000_000_000 => Kata(n / 1_000_000_000) + " miliar " + Kata(n % 1_000_000_000),
                _ => Kata(n / 1_000_000_000_000) + " triliun " + Kata(n % 1_000_000_000_000)
            };
        }
    }
}
