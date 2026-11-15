using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Presensi
{
    /// <summary>
    /// Notulen rapat (t_meeting_minutes), satu per sesi presensi. Alur status: Draft → Final (Finalize); notulen Final dikunci.
    /// Lampiran tidak ikut dimuat (GetFile / UpdateFile).
    /// </summary>
    public partial class MeetingMinutes : BaseLib
    {
        public class NewRow
        {
            public int SessionId { get; set; }
            public int? ChairedByMemberId { get; set; }
            public int? NotulisMemberId { get; set; }
            public string? Agenda { get; set; }
            public string Content { get; set; } = null!;
            public string? Decisions { get; set; }
            public string? FollowUp { get; set; }
        }
        public record FileContent(byte[] Data, string FileName, string ContentType);

        public int Id { get; set; }
        public int SessionId { get; private set; }
        public int? ChairedByMemberId { get; set; }
        public string? ChairedByName { get; private set; }
        public int? NotulisMemberId { get; set; }
        public string? NotulisName { get; private set; }
        public string? Agenda { get; set; }
        public string Content { get; set; } = null!;
        public string? Decisions { get; set; }
        public string? FollowUp { get; set; }
        // Terisi = lampiran ada.
        public string? FileName { get; private set; }
        // Hanya-baca: diubah lewat Finalize.
        public MeetingMinuteStatus Status { get; private set; }
        public int? FinalizedBy { get; private set; }
        public DateTime? FinalizedAt { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        // Lampiran (blob) tidak ikut di-select.
        private static IQueryable<EF.TMeetingMinute> _Querys
        {
            get
            {
                return Project(DBClass.GetContext().TMeetingMinutes);
            }
        }
        internal static IQueryable<EF.TMeetingMinute> Project(IQueryable<EF.TMeetingMinute> query)
        {
            return query.Select(x => new EF.TMeetingMinute
            {
                Id = x.Id,
                SessionId = x.SessionId,
                ChairedByMemberId = x.ChairedByMemberId,
                ChairedByMember = x.ChairedByMember == null ? null : new EF.MMember { Id = x.ChairedByMember.Id, Fullname = x.ChairedByMember.Fullname },
                NotulisMemberId = x.NotulisMemberId,
                NotulisMember = x.NotulisMember == null ? null : new EF.MMember { Id = x.NotulisMember.Id, Fullname = x.NotulisMember.Fullname },
                Agenda = x.Agenda,
                Content = x.Content,
                Decisions = x.Decisions,
                FollowUp = x.FollowUp,
                FileName = x.FileName,
                ContentType = x.ContentType,
                Status = x.Status,
                FinalizedBy = x.FinalizedBy,
                FinalizedAt = x.FinalizedAt,
                InsDate = x.InsDate,
                InsBy = x.InsBy,
                UpdDate = x.UpdDate,
                UpdBy = x.UpdBy
            });
        }
        public MeetingMinutes()
        {
        }
        public MeetingMinutes(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<MeetingMinutes>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal MeetingMinutes(EF.TMeetingMinute row)
        {
            SetData(row);
        }
        private void SetData(EF.TMeetingMinute row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<MeetingMinutes>();
            SessionId = row.SessionId;
            ChairedByMemberId = row.ChairedByMemberId;
            ChairedByName = row.ChairedByMember?.Fullname;
            NotulisMemberId = row.NotulisMemberId;
            NotulisName = row.NotulisMember?.Fullname;
            Agenda = row.Agenda;
            Content = row.Content;
            Decisions = row.Decisions;
            FollowUp = row.FollowUp;
            FileName = row.FileName;
            Status = (MeetingMinuteStatus)row.Status;
            FinalizedBy = row.FinalizedBy;
            FinalizedAt = row.FinalizedAt;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
