using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Approval
{
    /// <summary>
    /// Alur persetujuan yang diatur admin (m_approval_flow), mis. FUNDING_REQUEST: Bendahara → Ketua Umum.
    /// Satu alur aktif per jenis dokumen (lihat GetEffective); active_key kolom computed, tidak pernah ditulis.
    /// </summary>
    public partial class ApprovalFlow : BaseLib
    {
        public class NewRow
        {
            public string DocumentType { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            // TidakAktif = disiapkan dulu tanpa mengganggu alur yang sedang aktif.
            public ActiveStatus IsActive { get; set; } = ActiveStatus.Aktif;
            // Opsional: langkah yang langsung dibuat bersama alur (FlowId diisi otomatis).
            public List<ApprovalFlowStep.NewRow>? Steps { get; set; }
        }

        public int Id { get; set; }
        public string DocumentType { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private List<ApprovalFlowStep>? StepList;

        EF.ModelContext _context = null!;
        private IQueryable<EF.MApprovalFlow> _Query
        {
            get
            {
                return _context.MApprovalFlows.AsQueryable();
            }
        }
        private static IQueryable<EF.MApprovalFlow> _Querys
        {
            get
            {
                return DBClass.GetContext().MApprovalFlows.AsQueryable();
            }
        }
        public ApprovalFlow()
        {
            _context = DBClass.GetContext();
        }
        public ApprovalFlow(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<ApprovalFlow>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ApprovalFlow(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ApprovalFlow(EF.MApprovalFlow row)
        {
            SetData(row);
        }
        private void SetData(EF.MApprovalFlow row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ApprovalFlow>();
            DocumentType = row.DocumentType;
            Name = row.Name;
            Description = row.Description;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
