namespace GitaWidyaAgniWeb.Infrastructure
{
    /// <summary>
    /// Isi QR label unit aset = URL publik /aset/{qrToken} (token acak m_asset_unit.qr_token, detail alat tanpa login,
    /// LandingController.AsetPublik); kode unit tidak dimuat di QR sehingga URL tidak bisa ditebak.
    /// Scanner internal (checkout, pengembalian, inspeksi, perawatan) bisa mengetik URL itu atau kode unit saja;
    /// <see cref="ResolveKode"/> mengubah token / URL / kode menjadi kode unit.
    /// Basis URL: konfigurasi App:PublicUrl (domain publik yang tercetak di label), atau host request bila kosong.
    /// </summary>
    public static class AsetLabel
    {
        public const string PUBLIC_PATH = "aset";

        /// <summary>Segmen terakhir isi scan, huruf besar: "https://x/aset/ab12...?y" → "AB12...", "snr001" → "SNR001".</summary>
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

        /// <summary>Isi scan (URL QR, token, atau kode unit yang diketik) → kode unit. Token yang tidak dikenal dikembalikan apa adanya.</summary>
        public static string ResolveKode(string? input)
        {
            var text = KodeUnit(input);
            return GWALib.Lib.Master.Inventaris.AssetUnit.GetByQrToken(text)?.UnitCode ?? text;
        }

        public static string PublicUrl(string baseUrl, string qrToken) => $"{baseUrl}/{PUBLIC_PATH}/{Uri.EscapeDataString(qrToken)}";
    }
}
