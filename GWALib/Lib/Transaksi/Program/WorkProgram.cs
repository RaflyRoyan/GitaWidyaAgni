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
    /// Program kerja periode kepengurusan (t_program). Tidak dinamai "Program" karena sama dengan nama namespace-nya.
    /// Alur status: Planned → Ongoing → Done; Planned / Ongoing → Cancelled. Ubah data hanya saat Planned / Ongoing.
    /// PreviousProgramId = program periode sebelumnya yang dilanjutkan. Delete hanya Planned yang belum dipakai data lain.
    /// </summary>
    public partial class WorkProgram : BaseLib
    {
        public class NewRow
        {
            public int PeriodId { get; set; }
            public int? PreviousProgramId { get; set; }
            public int? PicMemberId { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public DateOnly? StartDate { get; set; }
            public DateOnly? EndDate { get; set; }
            public decimal BudgetAmount { get; set; }
        }

        public int Id { get; set; }
        public int PeriodId { get; private set; }
        public string PeriodName { get; private set; } = null!;
        public int? PreviousProgramId { get; set; }
        public int? PicMemberId { get; set; }
        public string? PicMemberName { get; private set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public decimal BudgetAmount { get; set; }
        // Hanya-baca: diubah lewat SetStatus.
        public ProgramStatus Status { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TProgram> _Querys
        {
            get
            {
                return DBClass.GetContext().TPrograms.Include(x => x.Period).Include(x => x.PicMember).AsQueryable();
            }
        }
        public WorkProgram()
        {
        }
        public WorkProgram(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<WorkProgram>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal WorkProgram(EF.TProgram row)
        {
            SetData(row);
        }
        private void SetData(EF.TProgram row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<WorkProgram>();
            PeriodId = row.PeriodId;
            PeriodName = row.Period?.Name ?? string.Empty;
            PreviousProgramId = row.PreviousProgramId;
            PicMemberId = row.PicMemberId;
            PicMemberName = row.PicMember?.Fullname;
            Title = row.Title;
            Description = row.Description;
            StartDate = row.StartDate;
            EndDate = row.EndDate;
            BudgetAmount = row.BudgetAmount;
            Status = (ProgramStatus)row.Status;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
