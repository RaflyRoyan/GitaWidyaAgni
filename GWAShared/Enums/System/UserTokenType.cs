using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_user_token.token_type
    /// </summary>
    public enum UserTokenType
    {
        [Description("Reset Password")] ResetPassword = 1,
        [Description("Verifikasi Email")] VerifyEmail = 2,
        [Description("Refresh Token")] Refresh = 3
    }
}
