using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_activity_collaborator.status
    /// </summary>
    public enum CollaboratorStatus
    {
        [Description("Diundang")] Diundang = 1,
        [Description("Diterima")] Diterima = 2,
        [Description("Ditolak")] Ditolak = 3
    }
}
