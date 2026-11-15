using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_form.status
    /// </summary>
    public enum FormStatus
    {
        [Description("Draft")] Draft = 1,
        [Description("Dibuka")] Open = 2,
        [Description("Ditutup")] Closed = 3
    }
}
