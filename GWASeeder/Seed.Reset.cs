using GWALib.General;
using Microsoft.EntityFrameworkCore;

namespace GWASeeder
{
    internal static partial class Seed
    {
        /// <summary>
        /// Mengosongkan SEMUA tabel model (termasuk m_user dan m_sequence) dalam satu koneksi dengan FOREIGN_KEY_CHECKS = 0.
        /// Struktur tabel tidak diubah. Dipanggil hanya lewat --reset setelah konfirmasi.
        /// </summary>
        public static int ResetDatabase()
        {
            using var context = DBClass.GetContext();
            var tables = context.Model.GetEntityTypes()
                .Select(e => e.GetTableName())
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .ToList();
            context.Database.OpenConnection();
            try
            {
                context.Database.ExecuteSqlRaw("SET FOREIGN_KEY_CHECKS = 0");
                foreach (var table in tables)
                {
                    context.Database.ExecuteSqlRaw($"DELETE FROM `{table}`");
                }
            }
            finally
            {
                context.Database.ExecuteSqlRaw("SET FOREIGN_KEY_CHECKS = 1");
                context.Database.CloseConnection();
            }
            return tables.Count;
        }
    }
}
