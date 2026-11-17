using System.ComponentModel.DataAnnotations;
using GitaWidyaAgniWeb.Infrastructure;
using GWALib.Lib.Master.Surat;
using GWALib.Lib.Transaksi.Surat;
using GWAShared.Enums;

namespace GitaWidyaAgniWeb.Models.Persuratan
{
    /// <summary>
    /// Persuratan › Surat: tab Surat Keluar (Draft → Menunggu Persetujuan → Disetujui / Ditolak → Terkirim → Diarsipkan, alur LETTER;
    /// nomor dibuat saat disetujui), Surat Masuk (Diterima → Diarsipkan), dan Template Surat. Disposisi untuk surat masuk / keluar yang sudah disetujui.
    /// </summary>
    public partial class SuratVM
    {
        public class Index
        {
        }

        // ================= Surat
        public class ListSurat
        {
            public int Arah { get; set; }
            public List<SuratRow> Rows { get; set; }
            public ListSurat(int arah)
            {
                Arah = arah;
                Rows = Letter.GetDataList((LetterDirection)arah).Select(x => new SuratRow
                {
                    EncDecrKey = x.EncDecrKey,
                    LetterNumber = x.LetterNumber,
                    LetterType = x.LetterType,
                    Subject = x.Subject,
                    LetterDate = Opsi.KeDateTime(x.LetterDate),
                    ReceivedDate = Opsi.KeDateTime(x.ReceivedDate),
                    Pihak = arah == (int)LetterDirection.In ? x.Sender : x.Recipient,
                    FileName = x.FileName,
                    Status = (int)x.Status,
                    StatusText = EnumHelper.Text(x.Status),
                    InsDate = x.InsDate,
                    Masuk = x.Direction == LetterDirection.In
                }).OrderByDescending(x => x.InsDate).ToList();
            }
        }

        public class SuratRow
        {
            public string EncDecrKey { get; set; } = "";
            public string? LetterNumber { get; set; }
            public string LetterType { get; set; } = "";
            public string Subject { get; set; } = "";
            public DateTime? LetterDate { get; set; }
            public DateTime? ReceivedDate { get; set; }
            public string? Pihak { get; set; }
            public string? FileName { get; set; }
            public int Status { get; set; }
            public string StatusText { get; set; } = "";
            public DateTime InsDate { get; set; }
            public bool Masuk { get; set; }
        }

        public class AddSurat
        {
            public NewSurat Row { get; set; } = new();
            public bool Masuk => Row.Direction == (int)LetterDirection.In;
            public List<GwaVM.OpsiItem> TemplateOpsi { get; set; } = LetterTemplate.GetDataList(FilterAktif.Aktif).Select(x => new GwaVM.OpsiItem(x.Id, x.Name + " · " + x.LetterType)).ToList();
            public List<string> JenisOpsi { get; set; } = LetterTemplate.GetLetterTypes();
            public List<GwaVM.OpsiItem> PeriodeOpsi { get; set; } = Opsi.Periode(true);
            public string? FileName { get; set; }

            public AddSurat() : this((int)LetterDirection.Out) { }
            public AddSurat(int arah)
            {
                Row.Direction = arah;
                if (arah == (int)LetterDirection.In)
                {
                    Row.ReceivedDate = DateTime.Today;
                }
            }
        }

        public class EditSurat : AddSurat
        {
            public string StatusText { get; set; } = "";
            public EditSurat(string encKey) : base()
            {
                var obj = LibExtensions.Load<Letter>(encKey, "Surat");
                StatusText = EnumHelper.Text(obj.Status);
                FileName = obj.FileName;
                Row = new NewSurat
                {
                    Direction = (int)obj.Direction,
                    PeriodId = obj.PeriodId,
                    TemplateId = obj.TemplateId,
                    LetterType = obj.LetterType,
                    LetterNumber = obj.LetterNumber,
                    Subject = obj.Subject,
                    LetterDate = Opsi.KeDateTime(obj.LetterDate),
                    ReceivedDate = Opsi.KeDateTime(obj.ReceivedDate),
                    Sender = obj.Sender,
                    Recipient = obj.Recipient,
                    Summary = obj.Summary,
                    Body = obj.Body
                };
            }
        }

        public class NewSurat
        {
            public int Direction { get; set; } = (int)LetterDirection.Out;

            [Display(Name = "Template")]
            public int? TemplateId { get; set; }

            [StringLength(50, ErrorMessage = "Jenis maksimal 50 karakter")]
            [Display(Name = "Jenis Surat (kode)")]
            public string? LetterType { get; set; }

            [StringLength(100, ErrorMessage = "Nomor maksimal 100 karakter")]
            [Display(Name = "Nomor Surat")]
            public string? LetterNumber { get; set; }

            [Required(ErrorMessage = "Perihal harus diisi")]
            [StringLength(255, ErrorMessage = "Perihal maksimal 255 karakter")]
            [Display(Name = "Perihal")]
            public string? Subject { get; set; }

            [Display(Name = "Tanggal Surat")]
            public DateTime? LetterDate { get; set; }

            [Display(Name = "Tanggal Diterima")]
            public DateTime? ReceivedDate { get; set; }

            [StringLength(150, ErrorMessage = "Pengirim maksimal 150 karakter")]
            [Display(Name = "Pengirim")]
            public string? Sender { get; set; }

            [StringLength(150, ErrorMessage = "Tujuan maksimal 150 karakter")]
            [Display(Name = "Tujuan")]
            public string? Recipient { get; set; }

            [StringLength(1000, ErrorMessage = "Ringkasan maksimal 1000 karakter")]
            [Display(Name = "Ringkasan")]
            public string? Summary { get; set; }

            [Display(Name = "Isi Surat")]
            public string? Body { get; set; }

            [Display(Name = "Periode")]
            public int? PeriodId { get; set; }
        }

        // Detail surat: persetujuan + disposisi.
        public class Detail
        {
            public string Key { get; set; } = "";
            public string Judul { get; set; } = "";
            public string Ringkasan { get; set; } = "";
            public bool BisaDisposisi { get; set; }
            public string? Body { get; set; }
            public List<GwaVM.OpsiItem> AnggotaOpsi { get; set; } = Opsi.Anggota();
            public List<LetterApproval> Persetujuan { get; set; }
            public List<LetterDisposition> Disposisi { get; set; }

            public Detail(string encKey)
            {
                var obj = LibExtensions.Load<Letter>(encKey, "Surat");
                Key = obj.EncDecrKey;
                Judul = (obj.LetterNumber ?? "(belum bernomor)") + " · " + obj.Subject;
                Ringkasan = EnumHelper.Text(obj.Direction) + " · " + obj.LetterType + " · " + EnumHelper.Text(obj.Status)
                    + (obj.Direction == LetterDirection.In ? " · dari " + obj.Sender : " · kepada " + obj.Recipient);
                Body = obj.Body ?? obj.Summary;
                BisaDisposisi = obj.Direction == LetterDirection.In
                    ? obj.Status == LetterStatus.Received || obj.Status == LetterStatus.Archived
                    : obj.Status == LetterStatus.Approved || obj.Status == LetterStatus.Sent || obj.Status == LetterStatus.Archived;
                Persetujuan = obj.GetApprovals(true).OrderBy(x => x.ApprovalLevel).ToList();
                Disposisi = obj.GetDispositions(true);
            }
        }

        public class DisposisiInput
        {
            public string Key { get; set; } = "";
            public int ToMemberId { get; set; }
            public string? Instruction { get; set; }
            public DateTime? DueDate { get; set; }
        }

        // ================= Template
        public class ListTemplate
        {
            public List<TemplateRow> Rows { get; set; } = LetterTemplate.GetDataList(FilterAktif.Semua).Select(x => new TemplateRow
            {
                EncDecrKey = x.EncDecrKey,
                Name = x.Name,
                LetterType = x.LetterType,
                AdaIsi = !string.IsNullOrWhiteSpace(x.Body),
                FileName = x.FileName,
                Aktif = x.IsActive == ActiveStatus.Aktif
            }).OrderBy(x => x.LetterType).ThenBy(x => x.Name).ToList();
        }

        public class TemplateRow
        {
            public string EncDecrKey { get; set; } = "";
            public string Name { get; set; } = "";
            public string LetterType { get; set; } = "";
            public bool AdaIsi { get; set; }
            public string? FileName { get; set; }
            public bool Aktif { get; set; }
        }

        public class AddTemplate
        {
            public NewTemplate Row { get; set; } = new();
            public List<GwaVM.OpsiItem> AktifList { get; set; } = GwaVM.OpsiAktif;
            public string? FileName { get; set; }
        }

        public class EditTemplate : AddTemplate
        {
            public EditTemplate(string encKey)
            {
                var obj = LibExtensions.Load<LetterTemplate>(encKey, "Template surat");
                FileName = obj.FileName;
                Row = new NewTemplate { Name = obj.Name, LetterType = obj.LetterType, Body = obj.Body, IsActive = (int)obj.IsActive };
            }
        }

        public class NewTemplate
        {
            [Required(ErrorMessage = "Nama template harus diisi")]
            [StringLength(150, ErrorMessage = "Nama maksimal 150 karakter")]
            [Display(Name = "Nama Template")]
            public string? Name { get; set; }

            [Required(ErrorMessage = "Jenis surat harus diisi")]
            [StringLength(50, ErrorMessage = "Jenis maksimal 50 karakter")]
            [Display(Name = "Jenis Surat (kode, mis. UNDANGAN)")]
            public string? LetterType { get; set; }

            [Display(Name = "Isi Template (HTML / teks)")]
            public string? Body { get; set; }

            [Display(Name = "Status")]
            public int IsActive { get; set; } = (int)ActiveStatus.Aktif;
        }
    }
}
