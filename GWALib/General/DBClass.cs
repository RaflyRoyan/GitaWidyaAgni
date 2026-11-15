using GWALib.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.General
{
    public class DBClass
    {
        public static readonly ServerVersion MySqlVersion = new MySqlServerVersion(new Version(8, 0, 30));
        private static string? _connectionString;

        public static void Configure(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string database tidak boleh kosong");
            }
            _connectionString = connectionString;
        }
        public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            optionsBuilder.UseMySql(connectionString, MySqlVersion);
        }
        public static ModelContext GetContext()
        {
            if (_connectionString == null)
            {
                throw new InvalidOperationException("DBClass belum dikonfigurasi, panggil DBClass.Configure(connectionString) saat startup");
            }
            var optionsBuilder = new DbContextOptionsBuilder<ModelContext>();
            ConfigureOptions(optionsBuilder, _connectionString);
            return new ModelContext(optionsBuilder.Options);
        }
    }
}
