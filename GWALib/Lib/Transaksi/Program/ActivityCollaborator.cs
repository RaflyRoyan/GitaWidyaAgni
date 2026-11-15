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
    /// Mitra luar yang diajak kolaborasi pada kegiatan (t_activity_collaborator). Satu mitra sekali per kegiatan.
    /// Alur status: Diundang → Diterima / Ditolak (Respond). Mitra blacklist / nonaktif tidak bisa diundang.
    /// </summary>
    public partial class ActivityCollaborator : BaseLib
    {
        public class NewRow
        {
            public int ActivityId { get; set; }
            public int ExternalPartyId { get; set; }
            public CollaboratorRole CollabRole { get; set; } = CollaboratorRole.PenyelenggaraBersama;
            public string? Notes { get; set; }
        }

        public int Id { get; set; }
        public int ActivityId { get; private set; }
        public int ExternalPartyId { get; private set; }
        public string ExternalPartyName { get; private set; } = null!;
        public CollaboratorRole CollabRole { get; set; }
        // Hanya-baca: diubah lewat Respond.
        public CollaboratorStatus Status { get; private set; }
        public string? Notes { get; set; }
        public int? RespondedBy { get; private set; }
        public DateTime? RespondedAt { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        // Hanya nama mitra yang diambil (tanpa blob logo).
        private static IQueryable<EF.TActivityCollaborator> _Querys
        {
            get
            {
                return DBClass.GetContext().TActivityCollaborators.Select(x => new EF.TActivityCollaborator
                {
                    Id = x.Id,
                    ActivityId = x.ActivityId,
                    ExternalPartyId = x.ExternalPartyId,
                    ExternalParty = new EF.MExternalParty { Id = x.ExternalParty.Id, Name = x.ExternalParty.Name },
                    CollabRole = x.CollabRole,
                    Status = x.Status,
                    Notes = x.Notes,
                    RespondedBy = x.RespondedBy,
                    RespondedAt = x.RespondedAt,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy,
                    UpdDate = x.UpdDate,
                    UpdBy = x.UpdBy
                });
            }
        }
        public ActivityCollaborator()
        {
        }
        public ActivityCollaborator(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ActivityCollaborator>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ActivityCollaborator(EF.TActivityCollaborator row)
        {
            SetData(row);
        }
        private void SetData(EF.TActivityCollaborator row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ActivityCollaborator>();
            ActivityId = row.ActivityId;
            ExternalPartyId = row.ExternalPartyId;
            ExternalPartyName = row.ExternalParty?.Name ?? string.Empty;
            CollabRole = (CollaboratorRole)row.CollabRole;
            Status = (CollaboratorStatus)row.Status;
            Notes = row.Notes;
            RespondedBy = row.RespondedBy;
            RespondedAt = row.RespondedAt;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
