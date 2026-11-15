using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.TimPelaksana
{
    /// <summary>
    /// Checklist persiapan kegiatan (t_activity_checklist), diisi manual atau dari template (GenerateFromTemplate:
    /// tenggat = hari H + OffsetDay, sie dicocokkan dengan nama; item template yang sudah pernah dibuat dilewati).
    /// Alur status: Belum → Proses → Selesai (bisa dibuka lagi ke Proses); Belum / Proses → Batal. Selesai mencatat DoneAt + DoneBy.
    /// </summary>
    public partial class ActivityChecklist : BaseLib
    {
        public class NewRow
        {
            public int ActivityId { get; set; }
            public int? SectionId { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public DateOnly? DueDate { get; set; }
            public int? PicMemberId { get; set; }
            public Priority Priority { get; set; } = Priority.Sedang;
            public MilestoneFlag IsMilestone { get; set; } = MilestoneFlag.Biasa;
        }

        public int Id { get; set; }
        public int ActivityId { get; private set; }
        public int? SectionId { get; set; }
        public string? SectionName { get; private set; }
        public int? TemplateItemId { get; private set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateOnly? DueDate { get; set; }
        public int? PicMemberId { get; set; }
        public string? PicMemberName { get; private set; }
        public Priority Priority { get; set; }
        public MilestoneFlag IsMilestone { get; set; }
        // Hanya-baca: diubah lewat SetStatus.
        public TaskProgressStatus Status { get; private set; }
        public DateTime? DoneAt { get; private set; }
        public int? DoneBy { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TActivityChecklist> _Querys
        {
            get
            {
                return DBClass.GetContext().TActivityChecklists.Include(x => x.Section).Include(x => x.PicMember).AsQueryable();
            }
        }
        public ActivityChecklist()
        {
        }
        public ActivityChecklist(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ActivityChecklist>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ActivityChecklist(EF.TActivityChecklist row)
        {
            SetData(row);
        }
        private void SetData(EF.TActivityChecklist row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ActivityChecklist>();
            ActivityId = row.ActivityId;
            SectionId = row.SectionId;
            SectionName = row.Section?.Name;
            TemplateItemId = row.TemplateItemId;
            Title = row.Title;
            Description = row.Description;
            DueDate = row.DueDate;
            PicMemberId = row.PicMemberId;
            PicMemberName = row.PicMember?.Fullname;
            Priority = (Priority)row.Priority;
            IsMilestone = (MilestoneFlag)row.IsMilestone;
            Status = (TaskProgressStatus)row.Status;
            DoneAt = row.DoneAt;
            DoneBy = row.DoneBy;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
