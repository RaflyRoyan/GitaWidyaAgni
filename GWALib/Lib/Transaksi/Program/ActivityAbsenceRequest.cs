using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GWALib.Lib.Transaksi.Program
{
    /// <summary>
    /// Pengajuan tidak ikut kegiatan (t_activity_absence_request). Anggota yang termasuk audiens kegiatan (Semua Anggota, Pengurus,
    /// Divisi, Section, Tim Pelaksana) otomatis terdaftar tanpa mendaftar; mereka mengajukan tidak ikut satu sesi (SessionId) atau
    /// seluruh kegiatan (SessionId NULL), lalu pengurus menyetujui / menolak.
    /// Alur: Menunggu → Disetujui / Ditolak (catatan wajib); Menunggu → Ditarik (oleh anggota sendiri).
    /// Efek persetujuan: seluruh kegiatan = anggota tidak lagi dihitung wajib hadir (tidak dicatat Alpa); satu sesi = dicatat Izin
    /// saat sesi ditutup (atau Alpa yang sudah tercatat diubah menjadi Izin).
    /// Kegiatan dengan audiens Peserta Terdaftar tetap memakai daftar / batal di ActivityParticipant.
    /// </summary>
    public partial class ActivityAbsenceRequest : BaseLib
    {
        public class NewRow
        {
            public int ActivityId { get; set; }
            /// <summary>Kosong = seluruh kegiatan.</summary>
            public int? SessionId { get; set; }
            public string Reason { get; set; } = null!;
        }

        public const int REASON_LENGTH = 500;

        public int Id { get; set; }
        public int ActivityId { get; private set; }
        public string ActivityTitle { get; private set; } = null!;
        public int MemberId { get; private set; }
        public string MemberName { get; private set; } = null!;
        public string? MemberNumber { get; private set; }
        public int? SessionId { get; private set; }
        public string? SessionName { get; private set; }
        public DateTime? SessionAt { get; private set; }
        public string Reason { get; private set; } = null!;
        public AbsenceRequestStatus Status { get; private set; }
        public string? ReviewNote { get; private set; }
        public int? ReviewedBy { get; private set; }
        public DateTime? ReviewedAt { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        /// <summary>true = untuk seluruh kegiatan, false = satu sesi.</summary>
        public bool WholeActivity => SessionId == null;

        private static IQueryable<EF.TActivityAbsenceRequest> _Querys
        {
            get
            {
                return DBClass.GetContext().TActivityAbsenceRequests
                    .Include(x => x.Activity).Include(x => x.Member).Include(x => x.Session).AsQueryable();
            }
        }
        public ActivityAbsenceRequest()
        {
        }
        public ActivityAbsenceRequest(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ActivityAbsenceRequest>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ActivityAbsenceRequest(EF.TActivityAbsenceRequest row)
        {
            SetData(row);
        }
        private void SetData(EF.TActivityAbsenceRequest row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ActivityAbsenceRequest>();
            ActivityId = row.ActivityId;
            ActivityTitle = row.Activity?.Title ?? string.Empty;
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname ?? string.Empty;
            MemberNumber = row.Member?.MemberNumber;
            SessionId = row.SessionId;
            SessionName = row.Session?.Name;
            SessionAt = row.Session?.OpenAt;
            Reason = row.Reason;
            Status = (AbsenceRequestStatus)row.Status;
            ReviewNote = row.ReviewNote;
            ReviewedBy = row.ReviewedBy;
            ReviewedAt = row.ReviewedAt;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
