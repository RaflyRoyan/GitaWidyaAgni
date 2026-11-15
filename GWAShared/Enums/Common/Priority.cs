using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_checklist_template_item.priority, t_activity_checklist.priority, t_program_task.priority
    /// </summary>
    public enum Priority
    {
        [Description("Rendah")] Rendah = 1,
        [Description("Sedang")] Sedang = 2,
        [Description("Tinggi")] Tinggi = 3
    }
}
