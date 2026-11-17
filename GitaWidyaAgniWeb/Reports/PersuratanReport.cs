using DevExpress.XtraReports.UI;
using GWALib.Lib.Transaksi.Surat;
using GWAShared.Enums;
using System.Net;

namespace GitaWidyaAgniWeb.Reports
{
    /// <summary>
    /// Laporan Persuratan (DevExpress): cetak surat keluar berkop, pratinjau template surat, lembar disposisi,
    /// agenda surat masuk / keluar, dan daftar arsip dokumen. Blok generik ada di DocumentReport.
    /// </summary>
    public class PersuratanReport : DocumentReport
    {
        /// <summary>Data cetak surat keluar (diisi SuratVM.Method dari Letter + persetujuannya).</summary>
        public class SuratData
        {
            public string? Number { get; set; }
            public DateOnly? Date { get; set; }
            public string Subject { get; set; } = "";
            public string? Recipient { get; set; }
            public string? Attachment { get; set; }
            /// <summary>Badan surat HTML (dari template / editor); kosong = Summary sebagai paragraf.</summary>
            public string? Body { get; set; }
            public string? Summary { get; set; }
            public List<Signer> Signers { get; set; } = new();
            /// <summary>Tanda air (mis. DRAFT) untuk surat yang belum disetujui; null = tanpa tanda air.</summary>
            public string? Stamp { get; set; }
        }

        /// <summary>Baris disposisi pada lembar disposisi.</summary>
        public record DisposisiLine(string From, string To, string Instruction, DateOnly? DueDate, string Status, DateTime Date);

        private PersuratanReport(string title, bool landscape = false) : base(title, landscape, totalPages: true)
        {
        }

        // ================================================================= SURAT KELUAR
        public static PersuratanReport Surat(SuratData d, ReportBrand brand)
        {
            var r = new PersuratanReport("Surat " + (d.Number ?? d.Subject));
            r.Letterhead(brand);
            r.Spacer(6);
            r.Paragraphs($"{brand.City}, {(d.Date == null ? "...................." : Date(d.Date))}", false, left: r.W / 2);
            r.Fields(new (string, string?)[]
            {
                ("Nomor", d.Number ?? "(dibuat otomatis saat disetujui)"),
                ("Lampiran", d.Attachment),
                ("Perihal", d.Subject)
            }, labelWidth: 80);
            r.Spacer(34);
            r.Paragraphs("Kepada Yth.\n" + (string.IsNullOrWhiteSpace(d.Recipient) ? "...................." : d.Recipient.Trim()) + "\ndi Tempat", false, left: 0);
            r.Spacer(10);
            if (!string.IsNullOrWhiteSpace(d.Body))
            {
                r.Html(d.Body);
            }
            else
            {
                r.Paragraphs(d.Summary, true, left: 0);
            }
            r.Signatures(null, null, d.Signers.Count == 0 ? new List<Signer> { new("Ketua Umum", null, null) } : d.Signers, knownLast: d.Signers.Count > 2);
            if (d.Stamp != null)
            {
                r.Stamp(d.Stamp);
            }
            return r;
        }

        /// <summary>Contoh tampilan template surat di atas kop (nomor / tujuan dikosongkan, tanda air CONTOH).</summary>
        public static PersuratanReport Template(string name, string letterType, string? body, ReportBrand brand)
        {
            var r = Surat(new SuratData
            {
                Subject = name,
                Body = string.IsNullOrWhiteSpace(body) ? "<p><i>Template ini tidak punya isi HTML; isi surat ada di berkas .docx.</i></p>" : body,
                Signers = new List<Signer> { new("Sekretaris", null, null), new("Ketua Umum", null, null) },
                Stamp = "CONTOH"
            }, brand);
            r.DisplayName = "Template " + name + " (" + letterType + ")";
            return r;
        }

        // ================================================================= LEMBAR DISPOSISI
        public static PersuratanReport Disposisi(Letter letter, List<DisposisiLine> lines, ReportBrand brand)
        {
            var r = new PersuratanReport("Lembar Disposisi " + (letter.LetterNumber ?? letter.Subject));
            r.Letterhead(brand);
            r.TitleBlock(new[] { "LEMBAR DISPOSISI" }, false, underlineFirst: true);

            var incoming = letter.Direction == LetterDirection.In;
            var info = new List<Row>
            {
                new() { C1 = incoming ? "Surat dari" : "Ditujukan kepada", C2 = (incoming ? letter.Sender : letter.Recipient) ?? "-" },
                new() { C1 = "Nomor surat", C2 = letter.LetterNumber ?? "-" },
                new() { C1 = "Tanggal surat", C2 = Date(letter.LetterDate) },
                new() { C1 = "Diterima tanggal", C2 = incoming ? Date(letter.ReceivedDate) : "-" },
                new() { C1 = "Jenis surat", C2 = letter.LetterType ?? "-" },
                new() { C1 = "Perihal", C2 = letter.Subject, Bold = true },
                new() { C1 = "Status", C2 = Infrastructure.EnumHelper.Text(letter.Status) }
            };
            r.Table(null, new float[] { 140, r.W - 140 }, new[] { L, L }, info, fontSize: 11f);
            if (!string.IsNullOrWhiteSpace(letter.Summary))
            {
                r.Paragraphs(letter.Summary, false, "Ringkasan isi", left: 0);
            }

            var rows = lines.Select((x, k) => new Row
            {
                C1 = (k + 1).ToString(),
                C2 = x.Date.ToString("d MMM yyyy", ID) + "\n" + x.From,
                C3 = x.To,
                C4 = x.Instruction,
                C5 = x.DueDate == null ? "-" : x.DueDate.Value.ToString("d MMM yyyy", ID),
                C6 = x.Status,
                C7 = ""
            }).ToList();
            // Baris kosong untuk disposisi tulisan tangan.
            var target = Math.Max(5, rows.Count + 2);
            for (var i = rows.Count; i < target; i++)
            {
                rows.Add(new Row { C1 = (i + 1).ToString() });
            }
            r.Table(new[] { "NO", "TANGGAL / DARI", "DITERUSKAN KEPADA", "ISI DISPOSISI / INSTRUKSI", "BATAS WAKTU", "STATUS", "PARAF" },
                new float[] { 36, 105, 115, r.W - 461, 80, 65, 60 }, new[] { C, L, L, L, C, C, C }, rows, heading: "DISPOSISI", fontSize: 10f, rowHeight: 34);

            // Kotak catatan kosong untuk ditulis tangan.
            r.Table(null, new float[] { r.W }, new[] { L }, new List<Row> { new(), new(), new() }, heading: "CATATAN", rowHeight: 28);
            return r;
        }

        // ================================================================= AGENDA SURAT
        /// <summary>Buku agenda surat masuk (incoming) atau keluar, landscape. dispositions = tujuan disposisi per surat (masuk).</summary>
        public static PersuratanReport Agenda(bool incoming, List<Letter> letters, Dictionary<int, string> dispositions, string range, ReportBrand brand)
        {
            var title = incoming ? "AGENDA SURAT MASUK" : "AGENDA SURAT KELUAR";
            var r = new PersuratanReport(incoming ? "Agenda Surat Masuk" : "Agenda Surat Keluar", landscape: true);
            r.Letterhead(brand);
            r.TitleBlock(new[] { title, range }, false, underlineFirst: true);
            List<Row> rows;
            if (incoming)
            {
                rows = letters.Select((x, k) => new Row
                {
                    C1 = (k + 1).ToString(),
                    C2 = Short(x.ReceivedDate),
                    C3 = x.LetterNumber ?? "-",
                    C4 = Short(x.LetterDate),
                    C5 = x.Sender ?? "-",
                    C6 = x.Subject,
                    C7 = dispositions.TryGetValue(x.Id, out var to) ? to : "-",
                    C8 = Infrastructure.EnumHelper.Text(x.Status)
                }).ToList();
                if (rows.Count == 0)
                {
                    rows.Add(new Row { C6 = "Tidak ada surat masuk pada rentang ini" });
                }
                r.Table(new[] { "NO", "DITERIMA", "NOMOR SURAT", "TGL SURAT", "PENGIRIM", "PERIHAL", "DISPOSISI KE", "STATUS" },
                    new float[] { 35, 92, 155, 88, 160, r.W - 765, 145, 90 }, new[] { C, C, L, C, L, L, L, C }, rows, fontSize: 10f);
            }
            else
            {
                rows = letters.Select((x, k) => new Row
                {
                    C1 = (k + 1).ToString(),
                    C2 = x.LetterNumber ?? "(belum bernomor)",
                    C3 = Short(x.LetterDate),
                    C4 = x.Recipient ?? "-",
                    C5 = x.Subject,
                    C6 = x.LetterType ?? "-",
                    C7 = Infrastructure.EnumHelper.Text(x.Status)
                }).ToList();
                if (rows.Count == 0)
                {
                    rows.Add(new Row { C5 = "Tidak ada surat keluar pada rentang ini" });
                }
                r.Table(new[] { "NO", "NOMOR SURAT", "TANGGAL", "TUJUAN", "PERIHAL", "JENIS", "STATUS" },
                    new float[] { 35, 170, 85, 200, r.W - 710, 110, 110 }, new[] { C, L, C, L, L, C, C }, rows, fontSize: 10f);
            }
            r.Paragraphs($"Jumlah: {letters.Count} surat · dicetak {DateTime.Now.ToString("d MMMM yyyy HH:mm", ID)}", false, left: 0);
            return r;
        }

        // ================================================================= ARSIP DOKUMEN
        public static PersuratanReport Arsip(List<Document> documents, ReportBrand brand)
        {
            var r = new PersuratanReport("Daftar Arsip Dokumen", landscape: true);
            r.Letterhead(brand);
            r.TitleBlock(new[] { "DAFTAR ARSIP DOKUMEN", "Per " + DateTime.Today.ToString("d MMMM yyyy", ID) }, false, underlineFirst: true);
            var rows = new List<Row>();
            var c = 0;
            foreach (var category in documents.GroupBy(x => x.CategoryName).OrderBy(x => x.Key))
            {
                rows.Add(new Row { C1 = (++c).ToString(), C2 = category.Key.ToUpper(ID), Bold = true, Shade = true });
                rows.AddRange(category.OrderBy(x => x.Title).Select(x => new Row
                {
                    C2 = x.Title,
                    C3 = x.Description ?? "-",
                    C4 = x.FileName,
                    C5 = Size(x.FileSize),
                    C6 = Infrastructure.EnumHelper.Text(x.Visibility),
                    C7 = x.InsDate.ToString("d MMM yyyy", ID)
                }));
            }
            if (rows.Count == 0)
            {
                rows.Add(new Row { C2 = "Belum ada dokumen" });
            }
            r.Table(new[] { "NO", "JUDUL", "KETERANGAN", "NAMA BERKAS", "UKURAN", "AKSES", "DIUNGGAH" },
                new float[] { 35, 220, r.W - 715, 200, 80, 90, 90 }, new[] { C, L, L, L, R, C, C }, rows, fontSize: 10f);
            r.Paragraphs($"Jumlah: {documents.Count} dokumen dalam {documents.Select(x => x.CategoryName).Distinct().Count()} kategori", false, left: 0);
            return r;
        }

        // ================================================================= TEKS
        private static string Short(DateOnly? value) => value == null ? "-" : value.Value.ToString("d MMM yyyy", ID);

        private static string Size(int? bytes)
        {
            if (bytes == null)
            {
                return "-";
            }
            return bytes < 1024 * 1024 ? $"{Math.Max(1, bytes.Value / 1024)} KB" : $"{bytes.Value / 1024d / 1024d:0.0} MB";
        }

        /// <summary>Teks biasa (bukan HTML) dari editor → HTML aman untuk XRRichText.</summary>
        public static string PlainToHtml(string text) =>
            string.Join("", text.Replace("\r", "").Split("\n\n").Select(p => "<p>" + WebUtility.HtmlEncode(p).Replace("\n", "<br/>") + "</p>"));
    }
}
