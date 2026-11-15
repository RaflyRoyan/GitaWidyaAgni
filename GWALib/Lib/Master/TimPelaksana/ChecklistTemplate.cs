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
    /// Template checklist persiapan kegiatan (m_checklist_template), mis. Seminar, Lomba luar kota, Konser.
    /// Nama unik.
    /// </summary>
    public partial class ChecklistTemplate : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public ActivityType? ActivityType { get; set; }
            public string? Description { get; set; }
            // Opsional: item yang langsung dibuat bersama template (TemplateId diisi otomatis).
            public List<ChecklistTemplateItem.NewRow>? Items { get; set; }
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ActivityType? ActivityType { get; set; }
        public string? Description { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private List<ChecklistTemplateItem>? ItemList;

        EF.ModelContext _context = null!;
        private IQueryable<EF.MChecklistTemplate> _Query
        {
            get
            {
                return _context.MChecklistTemplates
                    .AsQueryable();
            }
        }
        private static IQueryable<EF.MChecklistTemplate> _Querys
        {
            get
            {
                return DBClass.GetContext().MChecklistTemplates
                    .AsQueryable();
            }
        }
        public ChecklistTemplate()
        {
            _context = DBClass.GetContext();
        }
        public ChecklistTemplate(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<ChecklistTemplate>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ChecklistTemplate(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ChecklistTemplate(EF.MChecklistTemplate row)
        {
            SetData(row);
        }
        private void SetData(EF.MChecklistTemplate row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ChecklistTemplate>();
            Name = row.Name;
            ActivityType = row.ActivityType != null ? (ActivityType)row.ActivityType : null;
            Description = row.Description;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
