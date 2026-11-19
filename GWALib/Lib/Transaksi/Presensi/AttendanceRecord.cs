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
    /// Catatan presensi anggota per sesi (t_attendance_record), satu per (sesi, anggota). Bukti check-in / check-out
    /// (foto wajib + koordinat) ada di t_attendance_record_in / _out dan tidak ikut dimuat (GetCheckInPhoto / GetCheckOutPhoto).
    /// CheckIn lewat token QR, atau SessionId untuk sesi mode Tanpa QR (sejak 30 menit sebelum mulai): status Hadir, atau Terlambat bila lewat OpenAt + toleransi. SetManual untuk Izin / Sakit / Alpa / Hadir oleh pengurus.
    /// </summary>
    public partial class AttendanceRecord : BaseLib
    {
        // Data check-in / check-out dari aplikasi anggota.
        // Sesi dipilih lewat QrToken (hasil scan) atau SessionId (daftar sesi hari ini; hanya untuk sesi mode Tanpa QR).
        public class CheckRow
        {
            public string? QrToken { get; set; }
            public int? SessionId { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            // Isi file foto asli; dikompres oleh ImageUtility.
            public byte[] Photo { get; set; } = null!;
        }
        public record PhotoContent(byte[] Data, string ContentType, decimal? Latitude, decimal? Longitude);

        public int Id { get; private set; }
        public int SessionId { get; private set; }
        public int MemberId { get; private set; }
        public string MemberName { get; private set; } = null!;
        public DateTime? CheckInAt { get; private set; }
        public DateTime? CheckOutAt { get; private set; }
        public AttendanceStatus Status { get; private set; }
        public string? Notes { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TAttendanceRecord> _Querys
        {
            get
            {
                return DBClass.GetContext().TAttendanceRecords.Include(x => x.Member).AsQueryable();
            }
        }
        public AttendanceRecord()
        {
        }
        public AttendanceRecord(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<AttendanceRecord>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal AttendanceRecord(EF.TAttendanceRecord row)
        {
            SetData(row);
        }
        private void SetData(EF.TAttendanceRecord row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<AttendanceRecord>();
            SessionId = row.SessionId;
            MemberId = row.MemberId;
            MemberName = row.Member?.Fullname ?? string.Empty;
            CheckInAt = row.CheckInAt;
            CheckOutAt = row.CheckOutAt;
            Status = (AttendanceStatus)row.Status;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
