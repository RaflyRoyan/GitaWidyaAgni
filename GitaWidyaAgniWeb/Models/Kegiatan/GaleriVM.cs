using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Transaksi.Program;
using GWAShared.Enums;
using GWAShared.Extensions;

namespace GitaWidyaAgniWeb.Models.Kegiatan
{
    /// <summary>
    /// Kegiatan › Galeri (t_gallery + t_gallery_photo). Draft → Terbit (butuh minimal satu foto) → Diarsipkan.
    /// Foto pertama otomatis jadi sampul; galeri Terbit + Publik tampil di landing page.
    /// </summary>
    public class GaleriVM
    {
        public class Index
        {
        }

        public class List
        {
            public List<Row> Rows { get; set; } = Method.GetDataList().ToList();
        }

        public class Row
        {
            public string EncDecrKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string? Kegiatan { get; set; }
            public DateTime? EventDate { get; set; }
            public bool Publik { get; set; }
            public bool Disorot { get; set; }
            public int JumlahFoto { get; set; }
            public int? CoverId { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public DateTime? PublishedAt { get; set; }
        }

        public class Add
        {
            public NewRow Row { get; set; } = new();
            public List<GwaVM.OpsiItem> KegiatanOpsi { get; set; } = Opsi.Kegiatan();
            public List<GwaVM.OpsiItem> PublikOpsi { get; set; } = GwaVM.Opsi<PublicVisibility>();
            public List<GwaVM.OpsiItem> SorotOpsi { get; set; } = GwaVM.Opsi<FeaturedStatus>();
        }

        public class Edit : Add
        {
            public Edit(string encKey)
            {
                var obj = LibExtensions.Load<Gallery>(encKey, "Galeri");
                Row = new NewRow
                {
                    ActivityId = obj.ActivityId,
                    Title = obj.Title,
                    Description = obj.Description,
                    EventDate = Opsi.KeDateTime(obj.EventDate),
                    IsPublic = (int)obj.IsPublic,
                    IsFeatured = (int)obj.IsFeatured
                };
            }
        }

        public class NewRow
        {
            [Display(Name = "Kegiatan")]
            public int? ActivityId { get; set; }

            [Required(ErrorMessage = "Judul galeri harus diisi")]
            [StringLength(200, ErrorMessage = "Judul maksimal 200 karakter")]
            [Display(Name = "Judul Galeri")]
            public string? Title { get; set; }

            [Display(Name = "Deskripsi")]
            public string? Description { get; set; }

            [Display(Name = "Tanggal Acara")]
            public DateTime? EventDate { get; set; }

            [Display(Name = "Visibilitas")]
            public int IsPublic { get; set; } = (int)PublicVisibility.Public;

            [Display(Name = "Sorotan")]
            public int IsFeatured { get; set; } = (int)FeaturedStatus.Biasa;
        }

        // Modal foto.
        public class Foto
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public List<GalleryPhoto.Info> Items { get; set; }

            public Foto(string encKey)
            {
                var obj = LibExtensions.Load<Gallery>(encKey, "Galeri");
                Key = obj.EncDecrKey;
                Judul = obj.Title;
                Items = obj.GetPhotos(true).OrderBy(x => x.Seq).ToList();
            }
        }

        public class FotoInput
        {
            public string Key { get; set; } = "";
            public int Id { get; set; }
            public string? Caption { get; set; }
        }

        public class Method
        {
            public static IEnumerable<Row> GetDataList()
            {
                var kegiatan = Activity.GetDataList().ToDictionary(x => x.Id, x => x.Title);
                return Gallery.GetDataList().Select(x =>
                {
                    var photos = x.GetPhotos(true);
                    return new Row
                    {
                        EncDecrKey = x.EncDecrKey,
                        Title = x.Title,
                        Kegiatan = x.ActivityId.HasValue && kegiatan.TryGetValue(x.ActivityId.Value, out var k) ? k : null,
                        EventDate = Opsi.KeDateTime(x.EventDate),
                        Publik = x.IsPublic == PublicVisibility.Public,
                        Disorot = x.IsFeatured == FeaturedStatus.Disorot,
                        JumlahFoto = photos.Count,
                        CoverId = photos.FirstOrDefault(p => p.IsCover == GalleryPhotoRole.Sampul)?.Id ?? photos.FirstOrDefault()?.Id,
                        Status = (int)x.Status,
                        StatusText = EnumHelper.Text(x.Status),
                        PublishedAt = x.PublishedAt
                    };
                }).OrderByDescending(x => x.EventDate ?? x.PublishedAt);
            }

            public static void Simpan(NewRow input, string user)
                => Gallery.Save(new Gallery.NewRow
                {
                    ActivityId = input.ActivityId,
                    Title = input.Title ?? "",
                    Description = input.Description,
                    EventDate = Opsi.Tanggal(input.EventDate),
                    IsPublic = (PublicVisibility)input.IsPublic,
                    IsFeatured = (FeaturedStatus)input.IsFeatured
                }, user).Ok();

            public static void Update(string key, NewRow input, string user)
            {
                var obj = Load(key);
                obj.ActivityId = input.ActivityId;
                obj.Title = input.Title ?? "";
                obj.Description = input.Description;
                obj.EventDate = Opsi.Tanggal(input.EventDate);
                obj.IsPublic = (PublicVisibility)input.IsPublic;
                obj.IsFeatured = (FeaturedStatus)input.IsFeatured;
                obj.Update(user).Ok();
            }

            public static Gallery Load(string? key) => LibExtensions.Load<Gallery>(key, "Galeri");

            public static void TambahFoto(string? key, List<(byte[] Data, string Caption)> files, string user)
            {
                var obj = Load(key);
                if (files.Count == 0)
                {
                    throw new ArgumentException("Pilih foto terlebih dahulu.");
                }
                foreach (var f in files)
                {
                    GalleryPhoto.Save(new GalleryPhoto.NewRow { GalleryId = obj.Id, Data = f.Data, Caption = f.Caption }, user).Ok();
                }
            }

            // Foto milik galeri (cek kepemilikan lewat daftar info galeri).
            public static GalleryPhoto GetFoto(string? key, int id)
            {
                var obj = Load(key);
                if (!obj.GetPhotos(true).Any(x => x.Id == id))
                {
                    throw new ArgumentException("Foto tidak ditemukan.");
                }
                return new GalleryPhoto(id.ToString().EncryptString<GalleryPhoto>());
            }

            public static void Sampul(FotoInput input, string user) => GetFoto(input.Key, input.Id).SetCover(user).Ok();
            public static void HapusFoto(FotoInput input, string user) => GetFoto(input.Key, input.Id).Delete(user).Ok();

            public static void Keterangan(FotoInput input, string user)
            {
                var foto = GetFoto(input.Key, input.Id);
                foto.Caption = input.Caption;
                foto.Update(user).Ok();
            }
        }
    }
}
