using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_activity_collaborator.collab_role
    /// </summary>
    public enum CollaboratorRole
    {
        [Description("Penyelenggara Bersama")] PenyelenggaraBersama = 1,
        [Description("Pendukung")] Pendukung = 2
    }
}
