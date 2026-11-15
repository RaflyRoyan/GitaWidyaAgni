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
    /// Tugas pengurus (t_program_task), bagian dari program kerja (ProgramId) atau tugas umum periode (ProgramId NULL).
    /// Periode tugas program mengikuti periodenya. Alur status: Belum → Proses → Selesai; Belum / Proses → Batal;
    /// Selesai bisa dibuka lagi ke Proses. Selesai mencatat CompletedAt + DoneBy.
    /// </summary>
    public partial class ProgramTask : BaseLib
    {
        public class NewRow
        {
            // Diabaikan bila ProgramId diisi (ikut periode program).
            public int? PeriodId { get; set; }
            public int? ProgramId { get; set; }
            public int? DivisionId { get; set; }
            public int? ResponsibleMemberId { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public Priority Priority { get; set; } = Priority.Sedang;
            public DateOnly? StartDate { get; set; }
            public DateOnly? DueDate { get; set; }
            public MilestoneFlag IsMilestone { get; set; } = MilestoneFlag.Biasa;
        }

        public int Id { get; set; }
        public int PeriodId { get; private set; }
        public int? ProgramId { get; private set; }
        public string? ProgramTitle { get; private set; }
        public int? DivisionId { get; set; }
        public int? ResponsibleMemberId { get; set; }
        public string? ResponsibleMemberName { get; private set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public Priority Priority { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? DueDate { get; set; }
        public MilestoneFlag IsMilestone { get; set; }
        // Hanya-baca: diubah lewat SetStatus.
        public TaskProgressStatus Status { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public int? DoneBy { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TProgramTask> _Querys
        {
            get
            {
                return DBClass.GetContext().TProgramTasks.Include(x => x.Program).Include(x => x.ResponsibleMember).AsQueryable();
            }
        }
        public ProgramTask()
        {
        }
        public ProgramTask(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ProgramTask>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ProgramTask(EF.TProgramTask row)
        {
            SetData(row);
        }
        private void SetData(EF.TProgramTask row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ProgramTask>();
            PeriodId = row.PeriodId;
            ProgramId = row.ProgramId;
            ProgramTitle = row.Program?.Title;
            DivisionId = row.DivisionId;
            ResponsibleMemberId = row.ResponsibleMemberId;
            ResponsibleMemberName = row.ResponsibleMember?.Fullname;
            Title = row.Title;
            Description = row.Description;
            Priority = (Priority)row.Priority;
            StartDate = row.StartDate;
            DueDate = row.DueDate;
            IsMilestone = (MilestoneFlag)row.IsMilestone;
            Status = (TaskProgressStatus)row.Status;
            CompletedAt = row.CompletedAt;
            DoneBy = row.DoneBy;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
