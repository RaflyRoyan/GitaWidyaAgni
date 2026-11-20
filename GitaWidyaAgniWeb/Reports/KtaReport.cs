using DevExpress.Drawing;
using DevExpress.Drawing.Printing;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.BarCode;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;
using System.Drawing;
using System.Globalization;

namespace GitaWidyaAgniWeb.Reports
{
    /// <summary>
    /// Kartu Tanda Anggota (KTA), ukuran kartu ID-1 / CR80 (85,6 × 54 mm), dibuat lewat kode tanpa .repx.
    /// Depan: pita merah dengan logo + nama organisasi, foto, nama, NIA, section, jabatan / status, angkatan, masa berlaku.
    /// Belakang: QR verifikasi (URL publik /kta/{cardToken}, token acak bukan NIA), ketentuan singkat, tanggal terbit.
    /// Mode Kartu = satu halaman seukuran kartu per sisi (unduhan anggota); mode Lembar = A4, tiap baris depan | belakang
    /// bersebelahan (cetak massal, dipotong lalu dilipat / dilaminasi). Ukuran ditulis dalam 0,1 mm lalu dikonversi (M / R).
    /// </summary>
    public class KtaReport : XtraReport
    {
        public class Row
        {
            public string Fullname { get; set; } = "";
            public string MemberNumber { get; set; } = "";
            public string Section { get; set; } = "";
            public string Role { get; set; } = "";
            public string Angkatan { get; set; } = "";
            public string Study { get; set; } = "";
            public string Valid { get; set; } = "";
            public string Issued { get; set; } = "";
            public string Url { get; set; } = "";
            public string Initials { get; set; } = "";
            public byte[]? Photo { get; set; }
        }

        private const float W = 856;       // lebar kartu (0,1 mm)
        private const float H = 540;       // tinggi kartu
        private const float GAP = 80;      // jarak depan | belakang di mode lembar
        private const float ROW_GAP = 60;  // jarak antarbaris di mode lembar
        private const string FONT = "Arial";
        private static readonly Color Flame = Color.FromArgb(192, 33, 42);
        private static readonly Color FlameDark = Color.FromArgb(158, 23, 32);
        private static readonly Color Gold = Color.FromArgb(232, 181, 42);
        private static readonly Color Bronze = Color.FromArgb(154, 107, 0);
        private static readonly Color Ink = Color.FromArgb(43, 33, 24);
        private static readonly Color Muted = Color.FromArgb(122, 110, 96);
        private static readonly Color Ivory = Color.FromArgb(251, 248, 242);

        private static float M(float tenthMm) => tenthMm / 2.54f;
        private static RectangleF R(float x, float y, float w, float h) => new(M(x), M(y), M(w), M(h));


        public static List<Row> ToRows(IEnumerable<Member.CardData> cards, string baseUrl)
        {
            var id = CultureInfo.GetCultureInfo("id-ID");
            return cards.Select(c => new Row
            {
                Fullname = c.Fullname,
                MemberNumber = c.MemberNumber ?? "-",
                Section = c.Section ?? "-",
                Role = c.Status == MemberStatus.Alumni ? "Alumni" : c.Position ?? (c.MemberType == MemberType.Pengurus ? "Pengurus" : "Anggota"),
                Angkatan = c.Angkatan?.ToString() ?? "-",
                Study = string.Join(" · ", new[] { c.ProgramStudi, c.Fakultas }.Where(x => !string.IsNullOrWhiteSpace(x))),
                Valid = c.Status == MemberStatus.Alumni ? "Alumni" : c.ValidUntil != null ? c.ValidUntil.Value.ToString("d MMM yyyy", id) : "Selama aktif",
                Issued = c.IssuedAt.ToString("d MMM yyyy", id),
                Url = $"{baseUrl}/kta/{c.Token}",
                Initials = string.Concat(c.Fullname.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(x => char.ToUpperInvariant(x[0]))),
                Photo = c.Photo
            }).ToList();
        }

        /// <param name="sheet">true = A4 berisi pasangan depan | belakang; false = halaman seukuran kartu.</param>
        public KtaReport(List<Row> rows, ReportBrand brand, string title, bool sheet)
        {
            DisplayName = title;
            ExportOptions.PrintPreview.DefaultFileName = title;
            ExportOptions.Pdf.DocumentOptions.Title = title;
            DataSource = rows;

            DetailBand detail;
            if (sheet)
            {
                PaperKind = DXPaperKind.A4;
                Margins = new DXMargins(59, 59, 59, 59);
                detail = new DetailBand { HeightF = M(H + ROW_GAP) };
                Bands.Add(detail);
                Bands.Add(new TopMarginBand { HeightF = 59 });
                Bands.Add(new BottomMarginBand { HeightF = 59 });
                var left = (M(2100) - 118 - M(W * 2 + GAP)) / 2;
                detail.Controls.Add(Front(brand, left, 0));
                detail.Controls.Add(Back(brand, left + M(W + GAP), 0));
            }
            else
            {
                PaperKind = DXPaperKind.Custom;
                PageWidth = (int)Math.Ceiling(M(W));
                PageHeight = (int)Math.Ceiling(M(H));
                Margins = new DXMargins(0, 0, 0, 0);
                detail = new DetailBand { HeightF = M(H * 2) };
                Bands.Add(detail);
                Bands.Add(new TopMarginBand { HeightF = 0 });
                Bands.Add(new BottomMarginBand { HeightF = 0 });
                detail.Controls.Add(Front(brand, 0, 0));
                detail.Controls.Add(new XRPageBreak { LocationF = new PointF(0, M(H)) });
                detail.Controls.Add(Back(brand, 0, M(H)));
            }
        }

        private Row Current => (Row)GetCurrentRow();

        private XRPanel Card(float x, float y, Color back) => new()
        {
            BoundsF = new RectangleF(x, y, M(W), M(H)),
            BackColor = back,
            Borders = BorderSide.All,
            BorderColor = Color.FromArgb(220, 210, 190),
            BorderWidth = 1,
            CanGrow = false
        };

        private static XRLabel Lbl(float x, float y, float w, float h, float size, DXFontStyle style, Color color, string? text = null,
            TextAlignment align = TextAlignment.MiddleLeft) => new()
            {
                BoundsF = R(x, y, w, h),
                Text = text ?? "",
                Font = new DXFont(FONT, size, style),
                ForeColor = color,
                TextAlignment = align,
                WordWrap = true,
                Multiline = true,
                CanGrow = false,
                Borders = BorderSide.None,
                Padding = new PaddingInfo(0, 0, 0, 0)
            };

        // Pita warna (panel, bukan label: label yang bertumpuk menutupi teks di atasnya). Isinya diposisikan relatif ke pita.
        private static XRPanel Strip(float x, float y, float w, float h, Color color) => new()
        {
            BoundsF = R(x, y, w, h),
            BackColor = color,
            Borders = BorderSide.None,
            CanGrow = false
        };

        private static XRLabel Bound(XRLabel label, string field)
        {
            label.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", $"[{field}]"));
            return label;
        }

        private XRPanel Front(ReportBrand brand, float x, float y)
        {
            var card = Card(x, y, Color.White);

            // Pita atas: logo + judul.
            var head = Strip(0, 0, W, 130, Flame);
            card.Controls.Add(head);
            card.Controls.Add(Strip(0, 130, W, 12, Gold));
            var textLeft = 30f;
            if (brand.Logo != null && brand.Logo.Length > 0)
            {
                head.Controls.Add(new XRPictureBox
                {
                    BoundsF = R(25, 15, 100, 100),
                    ImageSource = new ImageSource(DXImage.FromStream(new MemoryStream(brand.Logo))),
                    Sizing = ImageSizeMode.ZoomImage,
                    Borders = BorderSide.None
                });
                textLeft = 140;
            }
            head.Controls.Add(Lbl(textLeft, 22, W - textLeft - 25, 34, 6.5f, DXFontStyle.Bold, Gold, "KARTU TANDA ANGGOTA"));
            head.Controls.Add(Lbl(textLeft, 54, W - textLeft - 25, 64, 9f, DXFontStyle.Bold, Color.White, brand.OrgName.ToUpperInvariant()));

            // Foto (atau inisial bila tidak ada foto).
            var frame = new XRPanel
            {
                BoundsF = R(35, 170, 230, 290),
                BackColor = Ivory,
                Borders = BorderSide.All,
                BorderColor = Gold,
                BorderWidth = 2,
                CanGrow = false
            };
            var initials = Bound(Lbl(0, 0, 230, 290, 22f, DXFontStyle.Bold, Bronze, null, TextAlignment.MiddleCenter), nameof(Row.Initials));
            frame.Controls.Add(initials);
            var photo = new XRPictureBox { BoundsF = R(4, 4, 222, 282), Sizing = ImageSizeMode.Squeeze, Borders = BorderSide.None };
            photo.BeforePrint += (_, _) =>
            {
                var bytes = Current.Photo;
                photo.ImageSource = bytes != null && bytes.Length > 0 ? new ImageSource(DXImage.FromStream(new MemoryStream(bytes))) : null;
                initials.Visible = bytes == null || bytes.Length == 0;
            };
            frame.Controls.Add(photo);
            card.Controls.Add(frame);

            // Identitas.
            const float L = 295, VW = W - L - 30;
            card.Controls.Add(Bound(Lbl(L, 165, VW, 80, 10f, DXFontStyle.Bold, Ink), nameof(Row.Fullname)));
            card.Controls.Add(Bound(Lbl(L, 245, VW, 40, 8f, DXFontStyle.Bold, Flame), nameof(Row.MemberNumber)));
            float top = 295;
            foreach (var (label, field) in new[] { ("Section", nameof(Row.Section)), ("Jabatan", nameof(Row.Role)), ("Angkatan", nameof(Row.Angkatan)) })
            {
                card.Controls.Add(Lbl(L, top, 150, 34, 5.5f, DXFontStyle.Regular, Muted, label));
                card.Controls.Add(Bound(Lbl(L + 150, top, VW - 150, 34, 6f, DXFontStyle.Bold, Ink), field));
                top += 38;
            }
            card.Controls.Add(Bound(Lbl(L, top, VW, 34, 5.5f, DXFontStyle.Regular, Muted), nameof(Row.Study)));

            // Pita bawah: masa berlaku.
            var foot = Strip(0, H - 50, W, 50, FlameDark);
            card.Controls.Add(foot);
            var valid = Lbl(30, 0, W - 60, 50, 5.5f, DXFontStyle.Bold, Color.White, null, TextAlignment.MiddleRight);
            valid.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "'Berlaku s.d. ' + [Valid]"));
            foot.Controls.Add(valid);
            return card;
        }

        private XRPanel Back(ReportBrand brand, float x, float y)
        {
            var card = Card(x, y, Ivory);
            card.Controls.Add(Strip(0, 0, W, 16, Gold));

            card.Controls.Add(Lbl(35, 45, 500, 40, 7f, DXFontStyle.Bold, Flame, "KETENTUAN"));
            var rules = string.Join("\n", new[]
            {
                "1. Kartu ini identitas resmi anggota " + brand.OrgName + " dan tidak dapat dipindahtangankan.",
                "2. Tunjukkan kartu saat kegiatan, peminjaman inventaris, dan pemilihan.",
                "3. Pindai QR untuk memeriksa keaslian dan status keanggotaan terkini.",
                "4. Kartu hilang? Lapor pengurus agar QR lama dinonaktifkan."
            });
            card.Controls.Add(Lbl(35, 90, 500, 300, 5.2f, DXFontStyle.Regular, Ink, rules, TextAlignment.TopLeft));

            var qr = new XRBarCode
            {
                BoundsF = R(570, 60, 250, 250),
                Symbology = new QRCodeGenerator
                {
                    CompactionMode = QRCodeCompactionMode.Byte,
                    ErrorCorrectionLevel = QRCodeErrorCorrectionLevel.M,
                    Version = QRCodeVersion.AutoVersion
                },
                ShowText = false,
                AutoModule = true,
                BackColor = Color.White,
                Borders = BorderSide.None,
                Alignment = TextAlignment.MiddleCenter,
                Padding = new PaddingInfo(0, 0, 0, 0)
            };
            qr.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[Url]"));
            card.Controls.Add(qr);
            card.Controls.Add(Lbl(560, 315, 270, 30, 5f, DXFontStyle.Bold, Bronze, "PINDAI UNTUK VERIFIKASI", TextAlignment.MiddleCenter));

            var issued = Lbl(35, 400, 500, 30, 5f, DXFontStyle.Regular, Muted);
            issued.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "'Diterbitkan ' + [Issued] + ' · ' + [MemberNumber]"));
            card.Controls.Add(issued);
            if (!string.IsNullOrWhiteSpace(brand.Contact))
            {
                card.Controls.Add(Lbl(35, 430, W - 70, 30, 5f, DXFontStyle.Regular, Muted, brand.Contact));
            }

            var back = Strip(0, H - 50, W, 50, Flame);
            card.Controls.Add(back);
            back.Controls.Add(Lbl(30, 0, W - 60, 50, 6f, DXFontStyle.Bold, Color.White, brand.OrgName, TextAlignment.MiddleCenter));
            return card;
        }
    }
}
