namespace GitaWidyaAgniWeb.Models.Login
{
    public class LoginVM
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ReturnUrl { get; set; }
        public string? PesanError { get; set; }
        /// <summary>Token sekali pakai hasil captcha seret ikon musik (Infrastructure/MusicCaptcha, Login/CaptchaVerifikasi).</summary>
        public string? CaptchaToken { get; set; }
    }
}
