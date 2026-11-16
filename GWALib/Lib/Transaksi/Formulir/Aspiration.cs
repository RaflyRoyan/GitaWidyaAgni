using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Formulir
{
    /// <summary>
    /// Aspirasi / pengaduan / saran (t_aspiration). Kirim butuh login; aspirasi anonim tidak menyimpan identitas (user_id dan ins_by NULL),
    /// sehingga tidak muncul di GetMyList dan tidak bisa diubah pengirim. Lampiran maksimal 5 MB, tidak dimuat di daftar (GetAttachment).
    /// Alur: Baru → Diproses → Selesai / Ditolak, dan Baru → Selesai / Ditolak langsung (Respond). Selesai / Ditolak wajib tanggapan.
    /// Ubah hanya oleh pengirim selama Baru. Hapus hanya saat Baru (oleh pengirim) atau Ditolak.
    /// </summary>
    public partial class Aspiration : BaseLib
    {
        public class NewRow
        {
            public IdentityMode IsAnonymous { get; set; } = IdentityMode.Teridentifikasi;
            public AspirationCategory Category { get; set; } = AspirationCategory.Aspirasi;
            public string Title { get; set; } = null!;
            public string Content { get; set; } = null!;
            public byte[]? Attachment { get; set; }
            public string? AttachmentFileName { get; set; }
        }
        public record FileContent(byte[] Data, string FileName);

        public const int MAX_ATTACHMENT_BYTES = 5 * 1024 * 1024;

        public int Id { get; set; }
        // NULL bila anonim.
        public int? UserId { get; private set; }
        public string? Username { get; private set; }
        public IdentityMode IsAnonymous { get; private set; }
        public AspirationCategory Category { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? AttachmentFileName { get; private set; }
        // Hanya-baca: diubah lewat Respond.
        public AspirationStatus Status { get; private set; }
        public string? Response { get; private set; }
        public int? RespondedBy { get; private set; }
        public string? RespondedByName { get; private set; }
        public DateTime? RespondedAt { get; private set; }
        public DateTime InsDate { get; private set; }

        // Kolom lampiran (blob) tidak ikut di-select.
        private static IQueryable<EF.TAspiration> _Querys
        {
            get
            {
                return DBClass.GetContext().TAspirations.Select(x => new EF.TAspiration
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    User = x.UserId == null ? null : new EF.MUser { Id = x.User!.Id, Username = x.User.Username },
                    IsAnonymous = x.IsAnonymous,
                    Category = x.Category,
                    Title = x.Title,
                    Content = x.Content,
                    AttachmentFileName = x.AttachmentFileName,
                    Status = x.Status,
                    Response = x.Response,
                    RespondedBy = x.RespondedBy,
                    RespondedByNavigation = x.RespondedBy == null ? null
                        : new EF.MUser { Id = x.RespondedByNavigation!.Id, Username = x.RespondedByNavigation.Username },
                    RespondedAt = x.RespondedAt,
                    InsDate = x.InsDate
                });
            }
        }
        public Aspiration()
        {
        }
        public Aspiration(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Aspiration>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Aspiration(EF.TAspiration row)
        {
            SetData(row);
        }
        private void SetData(EF.TAspiration row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Aspiration>();
            UserId = row.UserId;
            Username = row.User?.Username;
            IsAnonymous = (IdentityMode)row.IsAnonymous;
            Category = (AspirationCategory)row.Category;
            Title = row.Title;
            Content = row.Content;
            AttachmentFileName = row.AttachmentFileName;
            Status = (AspirationStatus)row.Status;
            Response = row.Response;
            RespondedBy = row.RespondedBy;
            RespondedByName = row.RespondedByNavigation?.Username;
            RespondedAt = row.RespondedAt;
            InsDate = row.InsDate;
        }
    }
}
