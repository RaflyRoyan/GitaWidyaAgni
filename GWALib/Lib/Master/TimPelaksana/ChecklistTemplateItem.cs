using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.TimPelaksana
{
    /// <summary>
    /// Item template checklist (m_checklist_template_item). Disalin ke t_activity_checklist saat template dipakai kegiatan;
    /// OffsetDay relatif terhadap hari H (negatif = sebelum). SectionName dicocokkan dengan t_committee_section.name.
    /// </summary>
    public partial class ChecklistTemplateItem : BaseLib
    {
        public class NewRow
        {
            public int TemplateId { get; set; }
            public string? SectionName { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public int OffsetDay { get; set; }
            public Priority Priority { get; set; } = Priority.Sedang;
            public MilestoneFlag IsMilestone { get; set; } = MilestoneFlag.Biasa;
            // 0 = otomatis ditaruh paling akhir.
            public int SeqNo { get; set; }
        }

        public int Id { get; set; }
        public ChecklistTemplate Template { get; set; } = null!;
        public string? SectionName { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int OffsetDay { get; set; }
        public Priority Priority { get; set; }
        public MilestoneFlag IsMilestone { get; set; }
        public int SeqNo { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MChecklistTemplateItem> _Query
        {
            get
            {
                return _context.MChecklistTemplateItems
                    .AsQueryable();
            }
        }
        private static IQueryable<EF.MChecklistTemplateItem> _Querys
        {
            get
            {
                return DBClass.GetContext().MChecklistTemplateItems
                    .AsQueryable();
            }
        }
        public ChecklistTemplateItem()
        {
            _context = DBClass.GetContext();
        }
        public ChecklistTemplateItem(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<ChecklistTemplateItem>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ChecklistTemplateItem(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ChecklistTemplateItem(EF.MChecklistTemplateItem row)
        {
            SetData(row);
        }
        private void SetData(EF.MChecklistTemplateItem row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ChecklistTemplateItem>();
            Template = row.Template != null ? new ChecklistTemplate(row.Template) : new ChecklistTemplate(row.TemplateId);
            SectionName = row.SectionName;
            Title = row.Title;
            Description = row.Description;
            OffsetDay = row.OffsetDay;
            Priority = (Priority)row.Priority;
            IsMilestone = (MilestoneFlag)row.IsMilestone;
            SeqNo = row.SeqNo;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
