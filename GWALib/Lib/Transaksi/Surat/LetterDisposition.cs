using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Surat
{
    /// <summary>
    /// Disposisi surat ke anggota (t_letter_disposition). Hanya untuk surat masuk (Received / Archived) atau surat keluar yang sudah disetujui.
    /// FromUserId = user pemberi. Open → Done (MarkDone) dan Done bisa dibuka lagi (Reopen). Ubah / hapus hanya saat Open.
    /// GetDataList(toMemberId) untuk daftar disposisi milik seorang anggota.
    /// </summary>
    public partial class LetterDisposition : BaseLib
    {
        public class NewRow
        {
            public int LetterId { get; set; }
            public int ToMemberId { get; set; }
            public string Instruction { get; set; } = null!;
            public DateOnly? DueDate { get; set; }
        }

        public int Id { get; set; }
        public int LetterId { get; private set; }
        public string LetterSubject { get; private set; } = null!;
        public int FromUserId { get; private set; }
        public int ToMemberId { get; set; }
        public string ToMemberName { get; private set; } = null!;
        public string Instruction { get; set; } = null!;
        public DateOnly? DueDate { get; set; }
        // Hanya-baca: diubah lewat MarkDone / Reopen.
        public DispositionStatus Status { get; private set; }
        public DateTime? DoneAt { get; private set; }
        public DateTime InsDate { get; private set; }

        private static IQueryable<EF.TLetterDisposition> _Querys
        {
            get
            {
                return DBClass.GetContext().TLetterDispositions.Select(x => new EF.TLetterDisposition
                {
                    Id = x.Id,
                    LetterId = x.LetterId,
                    Letter = new EF.TLetter { Id = x.LetterId, Subject = x.Letter.Subject },
                    FromUserId = x.FromUserId,
                    ToMemberId = x.ToMemberId,
                    ToMember = x.ToMember,
                    Instruction = x.Instruction,
                    DueDate = x.DueDate,
                    Status = x.Status,
                    DoneAt = x.DoneAt,
                    InsDate = x.InsDate
                });
            }
        }
        public LetterDisposition()
        {
        }
        public LetterDisposition(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<LetterDisposition>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal LetterDisposition(EF.TLetterDisposition row)
        {
            SetData(row);
        }
        private void SetData(EF.TLetterDisposition row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<LetterDisposition>();
            LetterId = row.LetterId;
            LetterSubject = row.Letter?.Subject ?? string.Empty;
            FromUserId = row.FromUserId;
            ToMemberId = row.ToMemberId;
            ToMemberName = row.ToMember?.Fullname ?? string.Empty;
            Instruction = row.Instruction;
            DueDate = row.DueDate;
            Status = (DispositionStatus)row.Status;
            DoneAt = row.DoneAt;
            InsDate = row.InsDate;
        }
    }
}
