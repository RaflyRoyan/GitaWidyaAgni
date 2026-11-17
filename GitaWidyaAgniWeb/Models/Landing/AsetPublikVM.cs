using GWALib.Lib.Master.Inventaris;

namespace GitaWidyaAgniWeb.Models.Landing
{
    /// <summary>Halaman publik hasil scan label QR unit aset (/aset/{kode}). Tanpa harga perolehan / tarif / data peminjam.</summary>
    public class AsetPublikVM
    {
        public string Kode { get; set; } = "";
        public AssetUnit? Unit { get; set; }
        public Asset? Aset { get; set; }
        public string? Kategori { get; set; }
        public bool AdaFoto { get; set; }
        public string? WhatsApp { get; set; }
        public string OrgName { get; set; } = "Gita Widya Agni";
        public bool Login { get; set; }
    }
}
