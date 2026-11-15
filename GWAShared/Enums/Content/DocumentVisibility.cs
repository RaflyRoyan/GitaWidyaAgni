using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_document.visibility
    /// </summary>
    public enum DocumentVisibility
    {
        [Description("Publik")] Public = 1,
        [Description("Anggota")] Member = 2,
        [Description("Pengurus")] Pengurus = 3
    }
}
