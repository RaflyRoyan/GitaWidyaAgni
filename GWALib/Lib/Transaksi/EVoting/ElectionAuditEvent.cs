using GWALib.General;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.EVoting
{
    /// <summary>
    /// Jejak audit pemilihan (t_election_audit_event). Hanya-baca dari luar; ditulis otomatis oleh Election / ElectionCandidate /
    /// ElectionEligibleVoter (internal Add). Pemberian suara sengaja tidak dicatat per pemilih supaya waktu memilih tidak bisa
    /// dikaitkan dengan suara.
    /// </summary>
    public partial class ElectionAuditEvent : BaseLib
    {
        public const string EVENT_CREATED = "CREATED";
        public const string EVENT_UPDATED = "UPDATED";
        public const string EVENT_REGISTRATION_OPENED = "REGISTRATION_OPENED";
        public const string EVENT_OPENED = "OPENED";
        public const string EVENT_CLOSED = "CLOSED";
        public const string EVENT_RESULT_FINALIZED = "RESULT_FINALIZED";
        public const string EVENT_CANDIDATE_REGISTERED = "CANDIDATE_REGISTERED";
        public const string EVENT_CANDIDATE_VERIFIED = "CANDIDATE_VERIFIED";
        public const string EVENT_CANDIDATE_REJECTED = "CANDIDATE_REJECTED";
        public const string EVENT_CANDIDATE_WITHDRAWN = "CANDIDATE_WITHDRAWN";
        public const string EVENT_CANDIDATE_DELETED = "CANDIDATE_DELETED";
        public const string EVENT_VOTERS_ADDED = "VOTERS_ADDED";
        public const string EVENT_VOTER_CHANGED = "VOTER_CHANGED";
        public const string EVENT_VOTER_DELETED = "VOTER_DELETED";

        public int Id { get; set; }
        public int ElectionId { get; private set; }
        public int? ActorUserId { get; private set; }
        public string? ActorName { get; private set; }
        public string EventType { get; private set; } = null!;
        public string? Description { get; private set; }
        public string? IpAddress { get; private set; }
        public DateTime InsDate { get; private set; }

        private static IQueryable<EF.TElectionAuditEvent> _Querys
        {
            get
            {
                return DBClass.GetContext().TElectionAuditEvents.Select(x => new EF.TElectionAuditEvent
                {
                    Id = x.Id,
                    ElectionId = x.ElectionId,
                    ActorUserId = x.ActorUserId,
                    ActorUser = x.ActorUserId == null ? null : new EF.MUser { Id = x.ActorUser!.Id, Username = x.ActorUser.Username },
                    EventType = x.EventType,
                    Description = x.Description,
                    IpAddress = x.IpAddress,
                    InsDate = x.InsDate
                });
            }
        }
        public ElectionAuditEvent()
        {
        }
        public ElectionAuditEvent(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ElectionAuditEvent>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ElectionAuditEvent(EF.TElectionAuditEvent row)
        {
            SetData(row);
        }
        private void SetData(EF.TElectionAuditEvent row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ElectionAuditEvent>();
            ElectionId = row.ElectionId;
            ActorUserId = row.ActorUserId;
            ActorName = row.ActorUser?.Username;
            EventType = row.EventType;
            Description = row.Description;
            IpAddress = row.IpAddress;
            InsDate = row.InsDate;
        }
    }
}
