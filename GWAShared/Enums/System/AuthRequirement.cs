using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_controller_action.need_auth
    /// </summary>
    public enum AuthRequirement
    {
        [Description("Tanpa Login")] NonAuth = 0,
        [Description("Wajib Login")] Auth = 1
    }
}
