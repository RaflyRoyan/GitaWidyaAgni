using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// t_borrow_request.rental_scheme | Seikhlasnya: anggota, sewa tidak ditagih
    /// </summary>
    public enum RentalScheme
    {
        [Description("Tarif Tetap")] TarifTetap = 1,
        [Description("Seikhlasnya")] Seikhlasnya = 2,
        [Description("Gratis")] Gratis = 3
    }
}
