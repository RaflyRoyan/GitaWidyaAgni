using DevExpress.Drawing;
using DevExpress.Drawing.Printing;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.BarCode;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using System.Drawing;

namespace GitaWidyaAgniWeb.Reports
{
    /// <summary>
    /// Label QR unit aset (DevExpress Reports, dibuat lewat kode tanpa .repx).
    /// A4 portrait, 4 kolom × 5 baris (label 46 × 53 mm, garis potong putus-putus).
    /// Isi QR = URL publik /aset/{qrToken} (token acak, detail alat tanpa login); kode unit hanya tercetak sebagai teks.
    /// Scanner internal memetakan token ke kode lewat AsetLabel.ResolveKode.
    /// Ukuran ditulis dalam 0,1 mm lalu dikonversi (M / R) ke satuan bawaan report, 1/100 inci: kontrol yang dibuat lewat kode
    /// memakai satuan bawaan walau ReportUnit diubah.
    /// </summary>
    public class LabelQrUnitReport : XtraReport
    {
        public class Row
        {
            public string UnitCode { get; set; } = "";
            // Isi QR: URL publik detail unit (AsetLabel.PublicUrl).
            public string Url { get; set; } = "";
            public string AssetName { get; set; } = "";
            public string? SerialNumber { get; set; }
        }

        private const float LABEL_WIDTH = 460;
        private const float LABEL_HEIGHT = 530;
        private const float COLUMN_SPACING = 20;
        private const string FONT = "Arial";

        private static float M(float tenthMm) => tenthMm / 2.54f;
        private static RectangleF R(float x, float y, float w, float h) => new(M(x), M(y), M(w), M(h));

        public LabelQrUnitReport(List<Row> rows, string orgName, byte[]? logo, string title)
        {
            DisplayName = title;
            ExportOptions.PrintPreview.DefaultFileName = title;
            ExportOptions.Pdf.DocumentOptions.Title = title;

            PaperKind = DXPaperKind.A4;
            Margins = new DXMargins(39, 39, 39, 39);
            DataSource = rows;

            var detail = new DetailBand { HeightF = M(LABEL_HEIGHT + COLUMN_SPACING) };
            detail.MultiColumn.Mode = MultiColumnMode.UseColumnCount;
            detail.MultiColumn.ColumnCount = 4;
            detail.MultiColumn.ColumnSpacing = M(COLUMN_SPACING);
            detail.MultiColumn.Layout = ColumnLayout.AcrossThenDown;
            Bands.Add(detail);
            Bands.Add(new TopMarginBand { HeightF = 39 });
            Bands.Add(new BottomMarginBand { HeightF = 39 });

            var panel = new XRPanel
            {
                BoundsF = R(0, 0, LABEL_WIDTH, LABEL_HEIGHT),
                Borders = BorderSide.All,
                BorderDashStyle = BorderDashStyle.Dash,
                BorderColor = Color.FromArgb(150, 150, 150),
                BorderWidth = 1,
                CanGrow = false
            };
            detail.Controls.Add(panel);

            // Kepala label: logo + nama organisasi.
            if (logo != null && logo.Length > 0)
            {
                panel.Controls.Add(new XRPictureBox
                {
                    BoundsF = R(15, 15, 70, 70),
                    ImageSource = new ImageSource(DXImage.FromStream(new MemoryStream(logo))),
                    Sizing = ImageSizeMode.ZoomImage
                });
            }
            panel.Controls.Add(new XRLabel
            {
                BoundsF = R(logo != null ? 95 : 15, 15, logo != null ? LABEL_WIDTH - 110 : LABEL_WIDTH - 30, 70),
                Text = orgName,
                Font = new DXFont(FONT, 7f, DXFontStyle.Bold),
                ForeColor = Color.FromArgb(154, 107, 0),
                TextAlignment = TextAlignment.MiddleLeft,
                WordWrap = true,
                CanGrow = false,
                Padding = new PaddingInfo(0, 0, 0, 0)
            });

            var qr = new XRBarCode
            {
                BoundsF = R(70, 95, 320, 320),
                Symbology = new QRCodeGenerator
                {
                    CompactionMode = QRCodeCompactionMode.Byte,
                    ErrorCorrectionLevel = QRCodeErrorCorrectionLevel.M,
                    Version = QRCodeVersion.AutoVersion
                },
                ShowText = false,
                AutoModule = true,
                Alignment = TextAlignment.MiddleCenter,
                Padding = new PaddingInfo(0, 0, 0, 0)
            };
            qr.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[Url]"));
            panel.Controls.Add(qr);

            var code = new XRLabel
            {
                BoundsF = R(10, 420, LABEL_WIDTH - 20, 55),
                Font = new DXFont(FONT, 12f, DXFontStyle.Bold),
                TextAlignment = TextAlignment.MiddleCenter,
                CanGrow = false,
                Padding = new PaddingInfo(0, 0, 0, 0)
            };
            code.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "[UnitCode]"));
            panel.Controls.Add(code);

            var name = new XRLabel
            {
                BoundsF = R(10, 475, LABEL_WIDTH - 20, 45),
                Font = new DXFont(FONT, 6.5f),
                ForeColor = Color.FromArgb(68, 68, 68),
                TextAlignment = TextAlignment.TopCenter,
                WordWrap = true,
                CanGrow = false,
                Padding = new PaddingInfo(0, 0, 0, 0)
            };
            name.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text",
                "Iif(IsNullOrEmpty([SerialNumber]), [AssetName], [AssetName] + ' · SN ' + [SerialNumber])"));
            panel.Controls.Add(name);
        }
    }
}
