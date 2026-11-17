using GWALib.Lib.Transaksi.Approval;
using GWAShared.Enums;
using System.Net;
using System.Net.Mail;

namespace GitaWidyaAgniWeb.Infrastructure.Jobs
{
    /// <summary>
    /// Pengaturan pengiriman t_message_outbox. Saluran tanpa konfigurasi tidak dikirim (pesan tetap Menunggu).
    /// Isi lewat user-secrets / environment, jangan di appsettings.json:
    ///   Smtp:Host, Smtp:Port (587), Smtp:EnableSsl (true), Smtp:Username, Smtp:Password, Smtp:From, Smtp:FromName
    ///   WhatsApp:Url (endpoint gateway, mis. https://api.fonnte.com/send), WhatsApp:Token (header Authorization)
    /// </summary>
    public class MessageSenderOptions
    {
        public SmtpOptions Smtp { get; set; } = new();
        public WhatsAppOptions WhatsApp { get; set; } = new();

        public class SmtpOptions
        {
            public string? Host { get; set; }
            public int Port { get; set; } = 587;
            public bool EnableSsl { get; set; } = true;
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? From { get; set; }
            public string? FromName { get; set; }
            public bool Configured => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(From);
        }

        public class WhatsAppOptions
        {
            public string? Url { get; set; }
            public string? Token { get; set; }
            public bool Configured => !string.IsNullOrWhiteSpace(Url);
        }
    }

    /// <summary>
    /// Mengirim satu pesan outbox. Email lewat SMTP; WhatsApp lewat gateway HTTP bergaya Fonnte
    /// (POST form target + message, header Authorization = token; nomor sudah berformat 62…).
    /// </summary>
    public class MessageSender
    {
        private readonly MessageSenderOptions _options;
        private readonly IHttpClientFactory _http;

        public MessageSender(IConfiguration config, IHttpClientFactory http)
        {
            _options = new MessageSenderOptions();
            config.GetSection("Smtp").Bind(_options.Smtp);
            config.GetSection("WhatsApp").Bind(_options.WhatsApp);
            _http = http;
        }

        public bool CanSend(NotificationChannel channel) => channel switch
        {
            NotificationChannel.Email => _options.Smtp.Configured,
            NotificationChannel.WhatsApp => _options.WhatsApp.Configured,
            _ => false
        };

        /// <summary>Melempar exception bila gagal; pesannya dicatat sebagai last_error.</summary>
        public async Task SendAsync(MessageOutbox message, CancellationToken token)
        {
            switch (message.Channel)
            {
                case NotificationChannel.Email:
                    await SendEmailAsync(message, token);
                    break;
                case NotificationChannel.WhatsApp:
                    await SendWhatsAppAsync(message, token);
                    break;
                default:
                    throw new InvalidOperationException("Saluran tidak didukung: " + message.Channel);
            }
        }

        private async Task SendEmailAsync(MessageOutbox message, CancellationToken token)
        {
            var smtp = _options.Smtp;
            using var client = new SmtpClient(smtp.Host, smtp.Port) { EnableSsl = smtp.EnableSsl };
            if (!string.IsNullOrWhiteSpace(smtp.Username))
            {
                client.Credentials = new NetworkCredential(smtp.Username, smtp.Password);
            }
            using var mail = new MailMessage
            {
                From = new MailAddress(smtp.From!, smtp.FromName ?? "Gita Widya Agni"),
                Subject = message.Subject ?? "Gita Widya Agni",
                Body = message.Body,
                IsBodyHtml = message.Body.TrimStart().StartsWith('<')
            };
            mail.To.Add(message.Recipient);
            await client.SendMailAsync(mail, token);
        }

        private async Task SendWhatsAppAsync(MessageOutbox message, CancellationToken token)
        {
            var wa = _options.WhatsApp;
            var client = _http.CreateClient(nameof(MessageSender));
            using var request = new HttpRequestMessage(HttpMethod.Post, wa.Url)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["target"] = message.Recipient,
                    ["message"] = string.IsNullOrWhiteSpace(message.Subject) ? message.Body : $"*{message.Subject}*\n{message.Body}"
                })
            };
            if (!string.IsNullOrWhiteSpace(wa.Token))
            {
                request.Headers.TryAddWithoutValidation("Authorization", wa.Token);
            }
            using var response = await client.SendAsync(request, token);
            var body = await response.Content.ReadAsStringAsync(token);
            // Gateway bergaya Fonnte membalas 200 dengan "status": false bila gagal.
            if (!response.IsSuccessStatusCode || body.Contains("\"status\":false", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Gateway WhatsApp {(int)response.StatusCode}: {Truncate(body, 300)}");
            }
        }

        private static string Truncate(string text, int max) => text.Length <= max ? text : text[..max];
    }
}
