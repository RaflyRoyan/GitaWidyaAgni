using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_aspiration.is_anonymous, t_form.is_anonymous
    /// </summary>
    public enum IdentityMode
    {
        [Description("Dengan Identitas")] Teridentifikasi = 0,
        [Description("Anonim")] Anonim = 1
    }
}
