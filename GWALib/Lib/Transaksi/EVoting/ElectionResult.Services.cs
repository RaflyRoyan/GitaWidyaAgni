using GWALib.General;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.EVoting
{
    public partial class ElectionResult
    {
        #region SYNC
        // Kosong sampai pemilihan difinalisasi.
        public static List<ElectionResult> GetDataList(int electionId)
        {
            return FilterQuery(electionId).ToList().MappingData<ElectionResult, EF.TElectionResult>();
        }
        #endregion

        #region ASYNC
        public static async Task<List<ElectionResult>> GetDataListAsync(int electionId)
        {
            return await (await FilterQuery(electionId).ToListAsync()).MappingDataAsync<ElectionResult, EF.TElectionResult>();
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TElectionResult> FilterQuery(int electionId)
        {
            return _Querys.Where(x => x.ElectionId == electionId).OrderBy(x => x.RankNo).ThenBy(x => x.Candidate.CandidateNumber);
        }
        #endregion
    }
}
