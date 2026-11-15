using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GWALib.EF
{
    /// <summary>
    /// Tambahan konfigurasi ModelContext di luar file hasil scaffold (tidak tertimpa saat scaffold ulang).
    /// </summary>
    public partial class ModelContext
    {
        private static readonly HashSet<Type> NumericTypes = new() { typeof(int), typeof(short), typeof(long), typeof(byte), typeof(decimal) };

        // EF Core 7 menganggap nilai CLR default (0) sebagai "tidak diisi" pada kolom yang punya DEFAULT di database,
        // sehingga insert dengan nilai 0 (mis. is_active = 0, allow_revision = 0) diam-diam memakai DEFAULT kolom (mis. 1).
        // Default SQL dilepas dari model untuk kolom angka non-nullable agar nilai dari kode selalu dikirim;
        // DEFAULT di database tetap ada. Kolom tanggal (ins_date) tidak diubah.
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(x => x.GetProperties()))
            {
                if (NumericTypes.Contains(property.ClrType) && property.GetDefaultValueSql() != null && !property.IsKey())
                {
                    property.SetDefaultValueSql(null);
                    property.ValueGenerated = ValueGenerated.Never;
                }
            }
        }
    }
}
