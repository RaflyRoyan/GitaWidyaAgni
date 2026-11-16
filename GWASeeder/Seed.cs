using System.Text;
using GWAShared.Enums;
using GWAShared.Results;

namespace GWASeeder
{
    /// <summary>
    /// Data contoh untuk semua tabel, diisi lewat service GWALib (validasi ikut teruji). Urutan modul mengikuti dependensi.
    /// Semua aksi memakai user admin kecuali yang memang harus dilakukan user tertentu (penyetuju, pemilih, presensi).
    /// </summary>
    internal static partial class Seed
    {
        public static string Admin = null!;
        public static string WebRoot = null!;
        public const string DEFAULT_PASSWORD = "gwa12345";
        public const string DEFAULT_ADMIN_PASSWORD = "admin12345";

        private static int _step;

        public static T Ok<T>(Response<T> response, string what)
        {
            if (response.Status != ResponseStatus.Success)
            {
                throw new InvalidOperationException($"{what}: {response.Message}");
            }
            return response.Data!;
        }
        public static void Section(string name)
        {
            Console.WriteLine($"[{++_step:D2}] {name}");
        }
        public static byte[] Image(string relativePath)
        {
            return File.ReadAllBytes(Path.Combine(WebRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }
        public static byte[] Avatar(int i)
        {
            return Image($"assets/images/users/avatar-{(i % 10) + 1}.jpg");
        }
        public static byte[] Photo(int i)
        {
            return Image($"assets/images/small/img-{(i % 12) + 1}.jpg");
        }
        // PDF minimal satu halaman berisi teks (cukup untuk lampiran contoh).
        public static byte[] Pdf(string text)
        {
            var safe = text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
            var content = $"BT /F1 14 Tf 60 760 Td ({safe}) Tj ET";
            var objects = new[]
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
                $"<< /Length {content.Length} >>\nstream\n{content}\nendstream",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
            };
            var sb = new StringBuilder("%PDF-1.4\n");
            var offsets = new List<int>();
            for (var i = 0; i < objects.Length; i++)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(sb.ToString()));
                sb.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
            }
            var xref = Encoding.ASCII.GetByteCount(sb.ToString());
            sb.Append($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
            foreach (var offset in offsets)
            {
                sb.Append($"{offset:D10} 00000 n \n");
            }
            sb.Append($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
            return Encoding.ASCII.GetBytes(sb.ToString());
        }
        // .docx minimal (zip berisi content types, relasi, dan satu paragraf).
        public static byte[] Docx(string text)
        {
            using var ms = new MemoryStream();
            using (var zip = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                void Add(string name, string content)
                {
                    using var writer = new StreamWriter(zip.CreateEntry(name).Open(), new UTF8Encoding(false));
                    writer.Write(content);
                }
                Add("[Content_Types].xml", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/></Types>");
                Add("_rels/.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"word/document.xml\"/></Relationships>");
                Add("word/document.xml", $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body><w:p><w:r><w:t>{System.Security.SecurityElement.Escape(text)}</w:t></w:r></w:p></w:body></w:document>");
            }
            return ms.ToArray();
        }
        public static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
        public static DateOnly D(int year, int month, int day) => new(year, month, day);
    }
}
