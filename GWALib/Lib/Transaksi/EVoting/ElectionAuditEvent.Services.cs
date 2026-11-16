using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.EVoting
{
    public partial class ElectionAuditEvent
    {
        #region SYNC
        public static List<ElectionAuditEvent> GetDataList(int electionId, string? eventType = null)
        {
            return FilterQuery(electionId, eventType).ToList().MappingData<ElectionAuditEvent, EF.TElectionAuditEvent>();
        }
        #endregion

        #region ASYNC
        public static async Task<List<ElectionAuditEvent>> GetDataListAsync(int electionId, string? eventType = null)
        {
            return await (await FilterQuery(electionId, eventType).ToListAsync()).MappingDataAsync<ElectionAuditEvent, EF.TElectionAuditEvent>();
        }
        #endregion

        #region INTERNAL
        // Tanpa SaveChanges; ikut tersimpan bersama perubahan yang dicatat.
        internal static void Add(EF.ModelContext context, int electionId, string eventType, string? description, int? actorUserId, string? ipAddress = null)
        {
            context.TElectionAuditEvents.Add(new EF.TElectionAuditEvent
            {
                Id = SequenceUtility.GetNextId<EF.TElectionAuditEvent>(context),
                ElectionId = electionId,
                ActorUserId = actorUserId,
                EventType = eventType,
                Description = TransaksiHelper.Truncate(description, 500),
                IpAddress = TransaksiHelper.Truncate(ipAddress, 45),
                InsDate = DateTime.Now,
                InsBy = actorUserId
            });
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TElectionAuditEvent> FilterQuery(int electionId, string? eventType)
        {
            var type = string.IsNullOrWhiteSpace(eventType) ? null : eventType.Trim().ToUpperInvariant();
            return _Querys.Where(x => x.ElectionId == electionId && (type == null || x.EventType == type))
                .OrderByDescending(x => x.InsDate).ThenByDescending(x => x.Id);
        }
        #endregion
    }
}
