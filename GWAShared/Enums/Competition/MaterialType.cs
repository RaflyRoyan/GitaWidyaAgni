using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_competition_material.material_type
    /// </summary>
    public enum MaterialType
    {
        [Description("File")] File = 1,
        [Description("Link")] Link = 2,
        [Description("Video")] Video = 3
    }
}
