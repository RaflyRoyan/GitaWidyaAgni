using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_document_number_format.reset_period
    /// </summary>
    public enum NumberResetPeriod
    {
        [Description("Tahunan")] Tahunan = 1,
        [Description("Bulanan")] Bulanan = 2,
        [Description("Tidak Pernah")] TidakPernah = 3
    }
}
