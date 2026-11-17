using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Surat;
using GWALib.Lib.Transaksi.Surat;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Persuratan
{
    /// <summary>Persuratan › Arsip Dokumen: tab Dokumen (t_document, berkas wajib maks. 20 MB) dan Kategori (m_document_category).</summary>
    public class DokumenVM
    {
        public class Index
        {
        }

        // ================= Dokumen
        public class ListDokumen
        {
            public List<DokumenRow> Rows { get; set; } = Document.GetDataList(jmldata: -1).Select(x => new DokumenRow
            {
                EncDecrKey = x.EncDecrKey,
                Title = x.Title,
                CategoryName = x.CategoryName,
                Description = x.Description,
                FileName = x.FileName,
                Ukuran = x.FileSize.HasValue ? (x.FileSize.Value >= 1048576 ? $"{x.FileSize.Value / 1048576d:0.0} MB" : $"{Math.Max(1, x.FileSize.Value / 1024)} KB") : "",
                Visibility = EnumHelper.Text(x.Visibility),
                InsDate = x.InsDate
            }).OrderByDescending(x => x.InsDate).ToList();
        }

        public class DokumenRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string CategoryName { get; set; } = "";
            public string? Description { get; set; }
            public string FileName { get; set; } = "";
            public string Ukuran { get; set; } = "";
            public string Visibility { get; set; } = "";
            public DateTime InsDate { get; set; }
        }

        public class AddDokumen
        {
            public NewDokumen Row { get; set; } = new();
            public List<GwaVM.OpsiItem> KategoriOpsi { get; set; } = DocumentCategory.GetDataList(FilterAktif.Aktif, jmldata: -1).Select(x => new GwaVM.OpsiItem(x.Id, x.Name)).ToList();
            public List<GwaVM.OpsiItem> VisibilitasOpsi { get; set; } = GwaVM.Opsi<DocumentVisibility>();
            public string? FileName { get; set; }
        }

        public class EditDokumen : AddDokumen
        {
            public EditDokumen(string encKey)
            {
                var obj = LibExtensions.Load<Document>(encKey, "Dokumen");
                FileName = obj.FileName;
                Row = new NewDokumen { CategoryId = obj.CategoryId, Title = obj.Title, Description = obj.Description, Visibility = (int)obj.Visibility };
            }
        }

        public class NewDokumen
        {
            [Required(ErrorMessage = "Pilih kategori")]
            [Display(Name = "Kategori")]
            public int? CategoryId { get; set; }

            [Required(ErrorMessage = "Judul harus diisi")]
            [StringLength(255, ErrorMessage = "Judul maksimal 255 karakter")]
            [Display(Name = "Judul")]
            public string? Title { get; set; }

            [StringLength(1000, ErrorMessage = "Keterangan maksimal 1000 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Display(Name = "Dapat dilihat oleh")]
            public int Visibility { get; set; } = (int)DocumentVisibility.Member;
        }

        // ================= Kategori
        public class ListKategori
        {
            public List<KategoriRow> Rows { get; set; } = DocumentCategory.GetDataList(FilterAktif.Semua, jmldata: -1).Select(x => new KategoriRow
            {
                EncDecrKey = x.EncDecrKey,
                Name = x.Name,
                Description = x.Description,
                Aktif = x.IsActive == ActiveStatus.Aktif
            }).OrderBy(x => x.Name).ToList();
        }

        public class KategoriRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Description { get; set; }
            public bool Aktif { get; set; }
        }

        public class AddKategori
        {
            public NewKategori Row { get; set; } = new();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
        }

        public class EditKategori : AddKategori
        {
            public EditKategori(string encKey)
            {
                var obj = LibExtensions.Load<DocumentCategory>(encKey, "Kategori dokumen");
                Row = new NewKategori { Name = obj.Name, Description = obj.Description, IsActive = (int)obj.IsActive };
            }
        }

        public class NewKategori
        {
            [Required(ErrorMessage = "Nama kategori harus diisi")]
            [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
            [Display(Name = "Nama Kategori")]
            public string? Name { get; set; }

            [StringLength(255, ErrorMessage = "Keterangan maksimal 255 karakter")]
            [Display(Name = "Keterangan")]
            public string? Description { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }

        public class Method
        {
            public static Document Load(string? key) => LibExtensions.Load<Document>(key, "Dokumen");
            public static DocumentCategory LoadKategori(string? key) => LibExtensions.Load<DocumentCategory>(key, "Kategori dokumen");

            public static void Simpan(NewDokumen input, byte[]? file, IFormFile? meta, string user)
            {
                if (file == null || meta == null)
                {
                    throw new ArgumentException("Pilih berkas dokumen.");
                }
                Document.Save(new Document.NewRow
                {
                    CategoryId = input.CategoryId ?? 0,
                    Title = input.Title ?? "",
                    Description = input.Description,
                    Visibility = (DocumentVisibility)input.Visibility,
                    FileData = file,
                    FileName = meta.FileName,
                    ContentType = meta.ContentType
                }, user).Ok();
            }

            public static void Update(string key, NewDokumen input, byte[]? file, IFormFile? meta, string user)
            {
                var obj = Load(key);
                obj.CategoryId = input.CategoryId ?? obj.CategoryId;
                obj.Title = input.Title ?? "";
                obj.Description = input.Description;
                obj.Visibility = (DocumentVisibility)input.Visibility;
                obj.Update(user).Ok();
                if (file != null)
                {
                    obj.UpdateFile(file, meta!.FileName, meta.ContentType, user).Ok();
                }
            }

            public static void SimpanKategori(NewKategori input, string user)
                => DocumentCategory.Save(new DocumentCategory.NewRow { Name = input.Name ?? "", Description = input.Description }, user).Ok();

            public static void UpdateKategori(string key, NewKategori input, string user)
            {
                var obj = LoadKategori(key);
                obj.Name = input.Name ?? "";
                obj.Description = input.Description;
                obj.IsActive = (ActiveStatus)input.IsActive;
                obj.Update(user).Ok();
            }
        }
    }
}
