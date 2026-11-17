using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Formulir;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Formulir
{
    /// <summary>
    /// Formulir › Aspirasi: aspirasi / pengaduan / saran anggota. Baru → Diproses → Selesai / Ditolak (Selesai / Ditolak wajib tanggapan).
    /// Aspirasi anonim tidak menyimpan identitas pengirim. Admin hanya bisa menghapus aspirasi yang Ditolak.
    /// </summary>
    public class AspirasiVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = Aspiration.GetDataList().Select(x => new Row
            {
                EncDecrKey = x.EncDecrKey,
                Title = x.Title,
                Kategori = EnumHelper.Text(x.Category),
                Pengirim = x.IsAnonymous == IdentityMode.Anonim ? "Anonim" : x.Username ?? "-",
                AttachmentFileName = x.AttachmentFileName,
                InsDate = x.InsDate,
                RespondedAt = x.RespondedAt,
                Status = (int)x.Status,
                StatusText = EnumHelper.Text(x.Status)
            }).OrderBy(x => x.Status > (int)AspirationStatus.Diproses).ThenByDescending(x => x.InsDate).ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string Kategori { get; set; } = "";
            public string Pengirim { get; set; } = "";
            public string? AttachmentFileName { get; set; }
            public DateTime InsDate { get; set; }
            public DateTime? RespondedAt { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
        }

        public class Detail
        {
            public Aspiration Data { get; set; }
            public bool BisaTanggapi { get; set; }
            public string Pengirim { get; set; }
            public Detail(string encKey)
            {
                Data = LibExtensions.Load<Aspiration>(encKey, "Aspirasi");
                BisaTanggapi = Data.Status == AspirationStatus.Baru || Data.Status == AspirationStatus.Diproses;
                Pengirim = Data.IsAnonymous == IdentityMode.Anonim ? "Anonim" : Data.Username ?? "-";
            }
        }

        public class Method
        {
            public static Aspiration Load(string? key) => LibExtensions.Load<Aspiration>(key, "Aspirasi");
        }
    }
}
