using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_management_assignment.is_current
    /// </summary>
    public enum AssignmentPeriod
    {
        [Description("Periode Lalu")] Lampau = 0,
        [Description("Periode Berjalan")] Berjalan = 1
    }
}
