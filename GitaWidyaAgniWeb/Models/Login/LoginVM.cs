namespace GitaWidyaAgniWeb.Models.Login
{
    public class LoginVM
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ReturnUrl { get; set; }
        public string? PesanError { get; set; }
    }
}
