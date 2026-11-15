using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_letter_disposition.status
    /// </summary>
    public enum DispositionStatus
    {
        [Description("Terbuka")] Open = 1,
        [Description("Selesai")] Done = 2
    }
}
