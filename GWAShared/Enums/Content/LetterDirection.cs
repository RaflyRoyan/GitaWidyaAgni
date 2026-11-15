using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_letter.direction | IN = surat masuk, OUT = surat keluar
    /// </summary>
    public enum LetterDirection
    {
        [Description("Surat Masuk")] In = 1,
        [Description("Surat Keluar")] Out = 2
    }
}
