using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_election_result.is_winner
    /// </summary>
    public enum ElectionOutcome
    {
        [Description("Kalah")] Lose = 0,
        [Description("Pemenang")] Winner = 1
    }
}
