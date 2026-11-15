using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_period_handover_item.item_type
    /// </summary>
    public enum HandoverItemType
    {
        [Description("Saldo")] Saldo = 1,
        [Description("Aset")] Aset = 2,
        [Description("Program")] Program = 3,
        [Description("Piutang")] Piutang = 4,
        [Description("Hutang")] Hutang = 5,
        [Description("Pinjaman")] Pinjaman = 6,
        [Description("Dokumen")] Dokumen = 7,
        [Description("Lainnya")] Lainnya = 8,
        [Description("MoU Aktif")] MouAktif = 9,
        [Description("Kewajiban Sponsor")] KewajibanSponsor = 10,
        [Description("Sponsor Berjalan")] SponsorBerjalan = 11,
        [Description("Pengadaan Berjalan")] PengadaanBerjalan = 12
    }
}
