using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Presensi
{
    /// <summary>
    /// Jadwal rutin mingguan kegiatan (t_activity_schedule), mis. latihan tiap Rabu 16:00-18:00 di GOR Kampus.
    /// GenerateSessions membuat sesi presensi untuk setiap tanggal yang cocok (satu sesi per jadwal per tanggal).
    /// Lokasi (titik + radius) dan toleransi terlambat disalin ke sesi. Delete = nonaktif.
    /// </summary>
    public partial class ActivitySchedule : BaseLib
    {
        public class NewRow
        {
            public int ActivityId { get; set; }
            public ScheduleDay DayOfWeek { get; set; }
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public string? Location { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public int? RadiusMeter { get; set; }
            public int LateToleranceMinute { get; set; } = 10;
            public DateOnly ValidFrom { get; set; }
            public DateOnly? ValidUntil { get; set; }
        }

        public int Id { get; set; }
        public int ActivityId { get; private set; }
        public ScheduleDay DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? Location { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? RadiusMeter { get; set; }
        public int LateToleranceMinute { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly? ValidUntil { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TActivitySchedule> _Querys
        {
            get
            {
                return DBClass.GetContext().TActivitySchedules.AsQueryable();
            }
        }
        public ActivitySchedule()
        {
        }
        public ActivitySchedule(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ActivitySchedule>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ActivitySchedule(EF.TActivitySchedule row)
        {
            SetData(row);
        }
        private void SetData(EF.TActivitySchedule row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ActivitySchedule>();
            ActivityId = row.ActivityId;
            DayOfWeek = (ScheduleDay)row.DayOfWeek;
            StartTime = row.StartTime;
            EndTime = row.EndTime;
            Location = row.Location;
            Latitude = row.Latitude;
            Longitude = row.Longitude;
            RadiusMeter = row.RadiusMeter;
            LateToleranceMinute = row.LateToleranceMinute;
            ValidFrom = row.ValidFrom;
            ValidUntil = row.ValidUntil;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
