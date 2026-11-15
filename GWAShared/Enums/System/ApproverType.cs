using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_approval_flow_step.approver_type
    /// </summary>
    public enum ApproverType
    {
        [Description("Role Pengurus")] Role = 1,
        [Description("Jabatan Pengurus")] Jabatan = 2,
        [Description("User Tertentu")] UserTertentu = 3
    }
}
