using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_competition_material.visibility
    /// </summary>
    public enum MaterialVisibility
    {
        [Description("Publik")] Public = 1,
        [Description("Anggota")] Member = 2,
        [Description("Peserta")] Participant = 3
    }
}
