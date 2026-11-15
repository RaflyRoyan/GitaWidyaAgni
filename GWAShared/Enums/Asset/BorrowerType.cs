using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_borrow_request.borrower_type
    /// </summary>
    public enum BorrowerType
    {
        [Description("Anggota")] Anggota = 1,
        [Description("Organisasi / Instansi")] OrganisasiInstansi = 2,
        [Description("Perorangan Luar")] PeroranganLuar = 3
    }
}
