using DevExpress.Drawing;
using DevExpress.Drawing.Printing;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.BarCode;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using GWALib.Lib.Transaksi.Prestasi;
using GWAShared.Enums;
using System.Drawing;
using System.Globalization;

namespace GitaWidyaAgniWeb.Reports
{
    /// <summary>
    /// Sertifikat anggota (DevExpress, A4 landscape, satu halaman): bingkai emas, logo + nama organisasi, judul, nomor,
    /// nama penerima + NIA, kalimat peran sesuai jenis (kegiatan / jabatan / prestasi), tanggal + kota, penanda tangan,
    /// dan QR ke halaman verifikasi publik. Sertifikat Dicabut diberi tanda air DICABUT.
    /// Ukuran dalam satuan bawaan report (1/100 inci).
    /// </summary>
    public class SertifikatReport : XtraReport
    {
        private const float PAGE_W = 1169 - 40;
        private const float PAGE_H = 827 - 40 - 7;
        private const string SERIF = "Georgia";
        private static readonly CultureInfo ID = new("id-ID");
        private static readonly Color Gold = Color.FromArgb(232, 181, 42);
        private static readonly Color Bronze = Color.FromArgb(154, 107, 0);
        private static readonly Color Flame = Color.FromArgb(200, 32, 42);
        private static readonly Color Ink = Color.FromArgb(40, 40, 40);
        private static readonly Color Muted = Color.FromArgb(100, 100, 100);

        private readonly DetailBand _detail;

        public SertifikatReport(Certificate.PrintData d, ReportBrand brand, string verifyUrl)
        {
            DisplayName = "Sertifikat " + d.Number + " " + d.MemberName;
            ExportOptions.PrintPreview.DefaultFileName = DisplayName;
            ExportOptions.Pdf.DocumentOptions.Title = DisplayName;
            PaperKind = DXPaperKind.A4;
            Landscape = true;
            Margins = new DXMargins(20, 20, 20, 20);
            Bands.Add(new TopMarginBand { HeightF = 20 });
            Bands.Add(new BottomMarginBand { HeightF = 20 });
            _detail = new DetailBand { HeightF = PAGE_H };
            Bands.Add(_detail);

            // Bingkai: garis emas tebal di luar, garis bronze tipis di dalam, aksen merah di sudut atas.
            Frame(10, 10, PAGE_W - 20, PAGE_H - 20, Gold, 5);
            Frame(26, 26, PAGE_W - 52, PAGE_H - 52, Bronze, 1);
            Bar(26, 26, 160, 6, Flame);
            Bar(PAGE_W - 186, PAGE_H - 32, 160, 6, Flame);

            // Kepala: logo + organisasi.
            if (brand.Logo is { Length: > 0 })
            {
                _detail.Controls.Add(new XRPictureBox
                {
                    BoundsF = new RectangleF(PAGE_W / 2 - 38, 46, 76, 76),
                    ImageSource = new ImageSource(DXImage.FromStream(new MemoryStream(brand.Logo))),
                    Sizing = ImageSizeMode.ZoomImage
                });
            }
            Text(brand.OrgName.ToUpper(ID), 126, 22, 13f, DXFontStyle.Bold, Bronze);
            Text(ReportBrand.INSTITUTION_LINE, 148, 18, 8.5f, DXFontStyle.Regular, Muted, "Arial");

            Text("SERTIFIKAT", 176, 62, 40f, DXFontStyle.Bold, Flame);
            if (d.Reference != null)
            {
                Text(d.Title, 238, 26, 14f, DXFontStyle.Italic, Ink);
            }
            Text("Nomor: " + d.Number, 266, 18, 9.5f, DXFontStyle.Regular, Muted, "Arial");

            Text("diberikan kepada", 300, 22, 13f, DXFontStyle.Regular, Ink);
            Text(d.MemberName, 324, 52, 28f, DXFontStyle.Bold, Ink);
            _detail.Controls.Add(new XRLine { BoundsF = new RectangleF(PAGE_W / 2 - 260, 376, 520, 4), ForeColor = Gold, LineWidth = 2 });
            if (!string.IsNullOrWhiteSpace(d.MemberNumber))
            {
                Text("NIA " + d.MemberNumber, 382, 20, 10.5f, DXFontStyle.Regular, Muted, "Arial");
            }

            Text(Phrase(d.Type), 412, 22, 13f, DXFontStyle.Regular, Ink);
            Text(d.Reference ?? d.Title, 436, 52, 17f, DXFontStyle.Bold, Bronze, wrap: true);
            if (!string.IsNullOrWhiteSpace(d.ReferenceDetail))
            {
                Text(d.ReferenceDetail, 488, 20, 11f, DXFontStyle.Italic, Muted);
            }

            // Tanda tangan.
            Text($"{brand.City}, {d.IssuedDate.ToString("d MMMM yyyy", ID)}", 522, 20, 11f, DXFontStyle.Regular, Ink);
            const float colW = 300;
            var gap = d.Signers.Count > 2 ? 20 : 120;
            var total = d.Signers.Count * colW + (d.Signers.Count - 1) * gap;
            var x = PAGE_W / 2 - total / 2;
            foreach (var s in d.Signers)
            {
                Text(s.Role, 546, 20, 11f, DXFontStyle.Regular, Ink, x: x, w: colW);
                Text(string.IsNullOrWhiteSpace(s.Name) ? "(..............................)" : s.Name, 636, 22, 11.5f,
                    string.IsNullOrWhiteSpace(s.Name) ? DXFontStyle.Regular : DXFontStyle.Bold | DXFontStyle.Underline, Ink, x: x, w: colW);
                if (!string.IsNullOrWhiteSpace(s.Number))
                {
                    Text("NIA " + s.Number, 658, 18, 9.5f, DXFontStyle.Regular, Muted, "Arial", x: x, w: colW);
                }
                x += colW + gap;
            }

            // QR verifikasi di kiri bawah.
            var qr = new XRBarCode
            {
                BoundsF = new RectangleF(52, PAGE_H - 190, 110, 110),
                Symbology = new QRCodeGenerator
                {
                    CompactionMode = QRCodeCompactionMode.Byte,
                    ErrorCorrectionLevel = QRCodeErrorCorrectionLevel.M,
                    Version = QRCodeVersion.AutoVersion
                },
                Text = verifyUrl,
                ShowText = false,
                AutoModule = true,
                Padding = new PaddingInfo(0, 0, 0, 0)
            };
            _detail.Controls.Add(qr);
            Text("Pindai untuk verifikasi\nKode " + d.VerificationCode, PAGE_H - 78, 30, 7.5f, DXFontStyle.Regular, Muted, "Arial", x: 32, w: 150, wrap: true);

            if (d.Status == CertificateStatus.Dicabut)
            {
                Watermark.Text = "DICABUT";
                Watermark.TextDirection = DirectionMode.ForwardDiagonal;
                Watermark.Font = new DXFont("Arial", 110f, DXFontStyle.Bold);
                Watermark.ForeColor = Flame;
                Watermark.TextTransparency = 190;
                Watermark.ShowBehind = false;
            }
        }

        private static string Phrase(CertificateType type) => type switch
        {
            CertificateType.Peserta => "atas partisipasinya sebagai Peserta dalam",
            CertificateType.Panitia => "atas dedikasinya sebagai Panitia / Tim Pelaksana dalam",
            CertificateType.Pemateri => "atas kesediaannya menjadi Pemateri dalam",
            CertificateType.Pengurus => "atas pengabdiannya sebagai",
            CertificateType.Prestasi => "atas prestasinya meraih",
            _ => "sebagai penghargaan atas"
        };

        private void Text(string text, float y, float h, float size, DXFontStyle style, Color color, string font = SERIF,
            float? x = null, float? w = null, bool wrap = false)
        {
            _detail.Controls.Add(new XRLabel
            {
                BoundsF = new RectangleF(x ?? 60, y, w ?? PAGE_W - 120, h),
                Text = text,
                Font = new DXFont(font, size, style),
                ForeColor = color,
                TextAlignment = wrap ? TextAlignment.TopCenter : TextAlignment.MiddleCenter,
                WordWrap = wrap,
                Multiline = wrap,
                CanGrow = false,
                CanShrink = false,
                Padding = new PaddingInfo(0, 0, 0, 0)
            });
        }

        private void Frame(float x, float y, float w, float h, Color color, int width)
        {
            _detail.Controls.Add(new XRPanel
            {
                BoundsF = new RectangleF(x, y, w, h),
                Borders = BorderSide.All,
                BorderColor = color,
                BorderWidth = width,
                CanGrow = false
            });
        }

        private void Bar(float x, float y, float w, float h, Color color)
        {
            _detail.Controls.Add(new XRLabel { BoundsF = new RectangleF(x, y, w, h), BackColor = color, Text = "", CanGrow = false, Borders = BorderSide.None });
        }
    }
}
