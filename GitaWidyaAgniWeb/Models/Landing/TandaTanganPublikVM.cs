using GWALib.Lib.Transaksi.Information;

namespace GitaWidyaAgniWeb.Models.Landing
{
    /// <summary>Halaman verifikasi QR tanda tangan elektronik /ttd/{token} (LandingController.TandaTanganPublik).</summary>
    public class TandaTanganPublikVM
    {
        public DocumentSignature.Check? Data { get; set; }
        public string OrgName { get; set; } = "Gita Widya Agni";
        public string? WhatsApp { get; set; }
        public string JenisDokumen => Data == null ? "" : Reports.TandaTanganQr.Jenis.GetValueOrDefault(Data.DocType, "Dokumen");
    }
}
