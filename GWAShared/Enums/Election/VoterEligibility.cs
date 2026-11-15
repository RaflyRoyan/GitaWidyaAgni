using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_election_eligible_voter.eligibility_status
    /// </summary>
    public enum VoterEligibility
    {
        [Description("Berhak Memilih")] Eligible = 1,
        [Description("Tidak Berhak Memilih")] Ineligible = 2
    }
}
