using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_checklist_template_item.is_milestone, t_activity_checklist.is_milestone, t_program_task.is_milestone
    /// </summary>
    public enum MilestoneFlag
    {
        [Description("Tugas Biasa")] Biasa = 0,
        [Description("Milestone")] Milestone = 1
    }
}
