namespace GitaWidyaAgniWeb.Infrastructure
{
    /// <summary>
    /// Isi QR label unit aset = URL publik /aset/{kodeUnit} (detail alat tanpa login, LandingController.AsetPublik).
    /// Scanner internal (checkout, pengembalian, inspeksi, perawatan) bisa mengetik URL itu atau kode unit saja;
    /// <see cref="KodeUnit"/> mengambil kode unitnya.
    /// Basis URL: konfigurasi App:PublicUrl (domain publik yang tercetak di label), atau host request bila kosong.
    /// </summary>
    public static class AsetLabel
    {
        public const string PUBLIC_PATH = "aset";

        /// <summary>"https://x/aset/snr001?y" / "SNR001" → "SNR001".</summary>
        public static string KodeUnit(string? input)
        {
            var text = (input ?? "").Trim();
            var cut = text.IndexOfAny(new[] { '?', '#' });
            if (cut >= 0)
            {
                text = text[..cut];
            }
            text = text.TrimEnd('/');
            var slash = text.LastIndexOf('/');
            if (slash >= 0)
            {
                text = text[(slash + 1)..];
            }
            return Uri.UnescapeDataString(text).Trim().ToUpperInvariant();
        }

        public static string BaseUrl(HttpRequest request, IConfiguration config)
        {
            var configured = config["App:PublicUrl"];
            return !string.IsNullOrWhiteSpace(configured)
                ? configured.Trim().TrimEnd('/')
                : $"{request.Scheme}://{request.Host}{request.PathBase}";
        }

        public static string PublicUrl(string baseUrl, string unitCode) => $"{baseUrl}/{PUBLIC_PATH}/{Uri.EscapeDataString(unitCode)}";
    }
}
