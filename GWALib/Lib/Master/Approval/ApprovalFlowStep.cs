using GWALib.General;
using GWALib.Lib.Master.Akses;
using GWALib.Lib.Master.Anggota;
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
    /// Langkah alur persetujuan (m_approval_flow_step), StepLevel mulai 1 dan unik per alur.
    /// Kolom approver yang dipakai bergantung ApproverType: Role untuk Role, Position untuk Jabatan, User untuk UserTertentu;
    /// kolom lainnya selalu dikosongkan (sesuai chk_flow_step_approver).
    /// </summary>
    public partial class ApprovalFlowStep : BaseLib
    {
        public class NewRow
        {
            public int FlowId { get; set; }
            // 0 = otomatis ditaruh paling akhir.
            public int StepLevel { get; set; }
            public string StepName { get; set; } = null!;
            public ApproverType ApproverType { get; set; }
            public int? RoleId { get; set; }
            public int? PositionId { get; set; }
            public int? UserId { get; set; }
            public RevisionPermission AllowRevision { get; set; } = RevisionPermission.Boleh;
        }

        public int Id { get; set; }
        public ApprovalFlow Flow { get; set; } = null!;
        public int StepLevel { get; set; }
        public string StepName { get; set; } = null!;
        public ApproverType ApproverType { get; set; }
        public Role? Role { get; set; }
        public Position? Position { get; set; }
        public UserAccount? User { get; set; }
        public RevisionPermission AllowRevision { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MApprovalFlowStep> _Query
        {
            get
            {
                return _context.MApprovalFlowSteps
                    .Include(x => x.Flow)
                    .Include(x => x.Role)
                    .Include(x => x.Position)
                    .Include(x => x.User)
                    .AsQueryable();
            }
        }
        private static IQueryable<EF.MApprovalFlowStep> _Querys
        {
            get
            {
                return DBClass.GetContext().MApprovalFlowSteps
                    .Include(x => x.Flow)
                    .Include(x => x.Role)
                    .Include(x => x.Position)
                    .Include(x => x.User)
                    .AsQueryable();
            }
        }
        public ApprovalFlowStep()
        {
            _context = DBClass.GetContext();
        }
        public ApprovalFlowStep(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<ApprovalFlowStep>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ApprovalFlowStep(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ApprovalFlowStep(EF.MApprovalFlowStep row)
        {
            SetData(row);
        }
        private void SetData(EF.MApprovalFlowStep row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ApprovalFlowStep>();
            Flow = row.Flow != null ? new ApprovalFlow(row.Flow) : new ApprovalFlow(row.FlowId);
            StepLevel = row.StepLevel;
            StepName = row.StepName;
            ApproverType = (ApproverType)row.ApproverType;
            Role = row.Role != null ? new Role(row.Role) : row.RoleId != null ? new Role(row.RoleId.Value) : null;
            Position = row.Position != null ? new Position(row.Position) : row.PositionId != null ? new Position(row.PositionId.Value) : null;
            User = row.User != null ? new UserAccount(row.User) : row.UserId != null ? new UserAccount(row.UserId.Value) : null;
            AllowRevision = (RevisionPermission)row.AllowRevision;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
