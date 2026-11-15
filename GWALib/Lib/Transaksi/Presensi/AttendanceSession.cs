using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Presensi
{
    /// <summary>
    /// Sesi presensi kegiatan (t_attendance_session), dibuat manual atau dari jadwal rutin (ScheduleId).
    /// Alur status: Scheduled → Open (QrToken dibuat) → Closed; Scheduled / Open → Cancelled (CancelReason).
    /// Close mencatat ALPA untuk anggota sasaran (sesuai audiens kegiatan) yang belum punya catatan presensi.
    /// </summary>
    public partial class AttendanceSession : BaseLib
    {
        public class NewRow
        {
            public int ActivityId { get; set; }
            public string Name { get; set; } = null!;
            public DateTime OpenAt { get; set; }
            public DateTime CloseAt { get; set; }
            public int LateToleranceMinute { get; set; } = 10;
            public string? Location { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public int? RadiusMeter { get; set; }
        }

        public int Id { get; set; }
        public int ActivityId { get; private set; }
        public int? ScheduleId { get; private set; }
        public string Name { get; set; } = null!;
        public DateTime OpenAt { get; set; }
        public DateTime CloseAt { get; set; }
        public int LateToleranceMinute { get; set; }
        public string? Location { get; set; }
        // Isi QR yang dipindai anggota; hanya ada saat sesi dibuka. Tidak ikut diserialisasi ke daftar umum.
        [JsonIgnore]
        public string? QrToken { get; private set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? RadiusMeter { get; set; }
        // Hanya-baca: diubah lewat Open / Close / Cancel.
        public AttendanceSessionStatus Status { get; private set; }
        public string? CancelReason { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TAttendanceSession> _Querys
        {
            get
            {
                return DBClass.GetContext().TAttendanceSessions.AsQueryable();
            }
        }
        public AttendanceSession()
        {
        }
        public AttendanceSession(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<AttendanceSession>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal AttendanceSession(EF.TAttendanceSession row)
        {
            SetData(row);
        }
        private void SetData(EF.TAttendanceSession row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<AttendanceSession>();
            ActivityId = row.ActivityId;
            ScheduleId = row.ScheduleId;
            Name = row.Name;
            OpenAt = row.OpenAt;
            CloseAt = row.CloseAt;
            LateToleranceMinute = row.LateToleranceMinute;
            Location = row.Location;
            QrToken = row.QrToken;
            Latitude = row.Latitude;
            Longitude = row.Longitude;
            RadiusMeter = row.RadiusMeter;
            Status = (AttendanceSessionStatus)row.Status;
            CancelReason = row.CancelReason;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
