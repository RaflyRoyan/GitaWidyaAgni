using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Surat
{
    /// <summary>
    /// Surat masuk / keluar (t_letter). Jenis surat dinormalisasi menjadi kode (ToTypeCode); template (opsional) mengisi jenis dan isi bila kosong.
    /// Surat keluar: Draft → WaitingApproval → Approved / Rejected (Rejected diperbaiki lalu diajukan lagi) → Sent → Archived. Persetujuan mengikuti
    /// alur aktif m_approval_flow LETTER (lihat LetterApproval); tanpa alur, satu persetujuan oleh penandatangan (jabatan opsional).
    /// Nomor dibuat saat disetujui dari format jenis surat, lalu format LETTER, atau SRT/yyyyMMdd/id. Surat masuk: langsung Received
    /// (nomor dari pengirim, tanggal terima wajib) → Archived. Nomor unik per arah. File (maksimal 10 MB) tidak dimuat di daftar (GetFile / UpdateFile).
    /// </summary>
    public partial class Letter : BaseLib
    {
        public class NewRow
        {
            public LetterDirection Direction { get; set; } = LetterDirection.Out;
            // Kosong = periode kepengurusan aktif.
            public int? PeriodId { get; set; }
            public int? TemplateId { get; set; }
            // Kosong = jenis dari template.
            public string? LetterType { get; set; }
            // Surat masuk: nomor dari pengirim. Surat keluar: diisi saat disetujui.
            public string? LetterNumber { get; set; }
            public string Subject { get; set; } = null!;
            public DateOnly? LetterDate { get; set; }
            public DateOnly? ReceivedDate { get; set; }
            public string? Sender { get; set; }
            public string? Recipient { get; set; }
            public string? Summary { get; set; }
            // Kosong = isi dari template.
            public string? Body { get; set; }
            public byte[]? FileData { get; set; }
            public string? FileName { get; set; }
            public string? ContentType { get; set; }
        }
        public record FileContent(byte[] Data, string FileName, string? ContentType);

        public int Id { get; set; }
        public int? PeriodId { get; set; }
        public int? TemplateId { get; set; }
        public LetterDirection Direction { get; private set; }
        public string LetterType { get; set; } = null!;
        public string? LetterNumber { get; set; }
        public string Subject { get; set; } = null!;
        public DateOnly? LetterDate { get; set; }
        public DateOnly? ReceivedDate { get; set; }
        public string? Sender { get; set; }
        public string? Recipient { get; set; }
        public string? Summary { get; set; }
        public string? Body { get; set; }
        // Terisi = file ada.
        public string? FileName { get; private set; }
        // Hanya-baca: diubah lewat Submit / Approve / Reject / MarkSent / Archive.
        public LetterStatus Status { get; private set; }
        public int CreatedBy { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private List<LetterApproval>? ApprovalList;
        private List<LetterDisposition>? DispositionList;

        // Kolom file (blob) tidak ikut di-select.
        private static IQueryable<EF.TLetter> _Querys
        {
            get
            {
                return DBClass.GetContext().TLetters.Select(x => new EF.TLetter
                {
                    Id = x.Id,
                    PeriodId = x.PeriodId,
                    TemplateId = x.TemplateId,
                    Direction = x.Direction,
                    LetterType = x.LetterType,
                    LetterNumber = x.LetterNumber,
                    Subject = x.Subject,
                    LetterDate = x.LetterDate,
                    ReceivedDate = x.ReceivedDate,
                    Sender = x.Sender,
                    Recipient = x.Recipient,
                    Summary = x.Summary,
                    Body = x.Body,
                    FileName = x.FileName,
                    Status = x.Status,
                    CreatedBy = x.CreatedBy,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy,
                    UpdDate = x.UpdDate,
                    UpdBy = x.UpdBy
                });
            }
        }
        public Letter()
        {
        }
        public Letter(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Letter>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Letter(EF.TLetter row)
        {
            SetData(row);
        }
        private void SetData(EF.TLetter row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Letter>();
            PeriodId = row.PeriodId;
            TemplateId = row.TemplateId;
            Direction = (LetterDirection)row.Direction;
            LetterType = row.LetterType;
            LetterNumber = row.LetterNumber;
            Subject = row.Subject;
            LetterDate = row.LetterDate;
            ReceivedDate = row.ReceivedDate;
            Sender = row.Sender;
            Recipient = row.Recipient;
            Summary = row.Summary;
            Body = row.Body;
            FileName = row.FileName;
            Status = (LetterStatus)row.Status;
            CreatedBy = row.CreatedBy;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
