using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Information;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Informasi
{
    /// <summary>
    /// Informasi › Banner: slide hero landing page. Gambar wajib (JPEG / PNG / WebP, maks. 5 MB, disimpan apa adanya).
    /// Jadwal kosong = tanpa batas; teks tombol wajib tautan. Hapus permanen.
    /// </summary>
    public class BannerVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = Banner.GetDataList(FilterAktif.Semua).Select(x => new Row
            {
                EncDecrKey = x.EncDecrKey,
                Title = x.Title,
                Subtitle = x.Subtitle,
                ButtonText = x.ButtonText,
                LinkUrl = x.LinkUrl,
                Seq = x.Seq,
                StartAt = x.StartAt,
                EndAt = x.EndAt,
                Aktif = x.IsActive == ActiveStatus.Aktif,
                Tayang = x.IsActive == ActiveStatus.Aktif && (x.StartAt == null || x.StartAt <= DateTime.Now) && (x.EndAt == null || x.EndAt >= DateTime.Now)
            }).OrderBy(x => x.Seq).ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string? Subtitle { get; set; }
            public string? ButtonText { get; set; }
            public string? LinkUrl { get; set; }
            public int Seq { get; set; }
            public DateTime? StartAt { get; set; }
            public DateTime? EndAt { get; set; }
            public bool Aktif { get; set; }
            public bool Tayang { get; set; }
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
            public string? Key { get; set; }
        }

        public class Edit : Add
        {
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<Banner>(encKey, "Banner");
                Key = obj.EncDecrKey;
                Row = new NewRow
                {
                    Title = obj.Title,
                    Subtitle = obj.Subtitle,
                    ButtonText = obj.ButtonText,
                    LinkUrl = obj.LinkUrl,
                    Seq = obj.Seq,
                    StartAt = obj.StartAt,
                    EndAt = obj.EndAt,
                    IsActive = (int)obj.IsActive
                };
            }
        }

        public class NewRow
        {
            [Required(ErrorMessage = "Judul harus diisi")]
            [StringLength(150, ErrorMessage = "Judul maksimal 150 karakter")]
            [Display(Name = "Judul")]
            public string? Title { get; set; }

            [StringLength(255, ErrorMessage = "Subjudul maksimal 255 karakter")]
            [Display(Name = "Subjudul")]
            public string? Subtitle { get; set; }

            [StringLength(50, ErrorMessage = "Teks tombol maksimal 50 karakter")]
            [Display(Name = "Teks Tombol")]
            public string? ButtonText { get; set; }

            [StringLength(255, ErrorMessage = "Tautan maksimal 255 karakter")]
            [Display(Name = "Tautan (https://... atau /path)")]
            public string? LinkUrl { get; set; }

            [Range(0, 999, ErrorMessage = "Urutan 0 - 999")]
            [Display(Name = "Urutan (0 = paling akhir)")]
            public int Seq { get; set; }

            [Display(Name = "Tayang Mulai")]
            public DateTime? StartAt { get; set; }

            [Display(Name = "Tayang Sampai")]
            public DateTime? EndAt { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        public class Method
        {
            public static Banner Load(string? key) => LibExtensions.Load<Banner>(key, "Banner");

            public static void Simpan(NewRow input, byte[]? image, string user)
            {
                if (image == null)
                {
                    throw new ArgumentException("Pilih gambar banner.");
                }
                Banner.Save(new Banner.NewRow
                {
                    Title = input.Title ?? "",
                    Subtitle = input.Subtitle,
                    ButtonText = input.ButtonText,
                    LinkUrl = input.LinkUrl,
                    Seq = input.Seq,
                    StartAt = input.StartAt,
                    EndAt = input.EndAt,
                    IsActive = (ActiveStatus)input.IsActive,
                    Image = image
                }, user).Ok();
            }

            public static void Update(string key, NewRow input, byte[]? image, string user)
            {
                var obj = Load(key);
                obj.Title = input.Title ?? "";
                obj.Subtitle = input.Subtitle;
                obj.ButtonText = input.ButtonText;
                obj.LinkUrl = input.LinkUrl;
                obj.Seq = input.Seq;
                obj.StartAt = input.StartAt;
                obj.EndAt = input.EndAt;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
                if (image != null)
                {
                    obj.UpdateImage(image, user).Ok();
                }
            }
        }
    }
}
