using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Program
{
    /// <summary>
    /// Rencana timeline periode per bulan (t_timeline_plan), mis. "Seminar nasional - Oktober 2026". Tanggal boleh belum pasti.
    /// Bila StartDate diisi, PlanYear / PlanMonth mengikuti StartDate. Alur status: Rencana → Terjadwal (LinkActivity) → Selesai;
    /// Rencana / Terjadwal → Batal.
    /// </summary>
    public partial class TimelinePlan : BaseLib
    {
        public class NewRow
        {
            public int? PeriodId { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public TimelinePlanType PlanType { get; set; } = TimelinePlanType.Event;
            public short PlanYear { get; set; }
            public int PlanMonth { get; set; }
            public DateOnly? StartDate { get; set; }
            public DateOnly? EndDate { get; set; }
            public int? ProgramId { get; set; }
            public int? PicMemberId { get; set; }
        }

        public int Id { get; set; }
        public int? PeriodId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public TimelinePlanType PlanType { get; set; }
        public short PlanYear { get; set; }
        public int PlanMonth { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        // Hanya-baca: diubah lewat LinkActivity / SetStatus.
        public TimelinePlanStatus Status { get; private set; }
        public int? ActivityId { get; private set; }
        public int? ProgramId { get; set; }
        public int? PicMemberId { get; set; }
        public string? PicMemberName { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TTimelinePlan> _Querys
        {
            get
            {
                return DBClass.GetContext().TTimelinePlans.Include(x => x.PicMember).AsQueryable();
            }
        }
        public TimelinePlan()
        {
        }
        public TimelinePlan(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<TimelinePlan>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal TimelinePlan(EF.TTimelinePlan row)
        {
            SetData(row);
        }
        private void SetData(EF.TTimelinePlan row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<TimelinePlan>();
            PeriodId = row.PeriodId;
            Title = row.Title;
            Description = row.Description;
            PlanType = (TimelinePlanType)row.PlanType;
            PlanYear = row.PlanYear;
            PlanMonth = row.PlanMonth;
            StartDate = row.StartDate;
            EndDate = row.EndDate;
            Status = (TimelinePlanStatus)row.Status;
            ActivityId = row.ActivityId;
            ProgramId = row.ProgramId;
            PicMemberId = row.PicMemberId;
            PicMemberName = row.PicMember?.Fullname;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
