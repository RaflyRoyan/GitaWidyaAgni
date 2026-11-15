using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_approval_flow_step.allow_revision | boleh mengembalikan untuk revisi
    /// </summary>
    public enum RevisionPermission
    {
        [Description("Tidak Boleh Revisi")] TidakBoleh = 0,
        [Description("Boleh Dikembalikan untuk Revisi")] Boleh = 1
    }
}
