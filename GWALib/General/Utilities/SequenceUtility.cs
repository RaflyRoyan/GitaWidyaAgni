using GWALib.EF;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace GWALib.General.Utilities
{
    /// <summary>
    /// Id primary key dari m_sequence (last_id per nama tabel). Kenaikan atomik lewat UPDATE + LAST_INSERT_ID(expr)
    /// dan langsung tercatat, tidak menunggu SaveChanges: di luar transaksi, id yang gagal disimpan menjadi celah;
    /// di dalam BeginTransaction milik pemanggil ikut di-rollback (baris m_sequence tabel itu terkunci sampai commit).
    /// Baris m_sequence yang belum ada dibuat otomatis dari MAX(id) tabelnya.
    /// </summary>
    public static class SequenceUtility
    {
        private const int MAX_ATTEMPTS = 3;

        private const string INCREMENT_SQL = @"UPDATE m_sequence
SET last_id = LAST_INSERT_ID(last_id + @count), upd_date = @now
WHERE table_name = @table;
SELECT IF(ROW_COUNT() > 0, LAST_INSERT_ID(), 0);";
        // IGNORE: bila request lain lebih dulu membuat baris yang sama, insert ini dilewati dan INCREMENT_SQL dicoba lagi.
        private const string INIT_SQL = @"INSERT IGNORE INTO m_sequence (table_name, last_id, upd_date)
SELECT @table, IFNULL(MAX(`{1}`), 0), @now FROM `{0}`;";

        private static readonly Regex IdentifierRegex = new(@"^[A-Za-z0-9_]+$", RegexOptions.Compiled);

        public static int GetNextId<T>(DbContext context) where T : class
        {
            return ReserveIds<T>(context, 1);
        }
        public static Task<int> GetNextIdAsync<T>(DbContext context) where T : class
        {
            return ReserveIdsAsync<T>(context, 1);
        }
        // Memesan beberapa id sekaligus (mis. baris anak yang dibuat bersama induknya); hasil = id pertama, berikutnya id + 1 dst.
        public static int ReserveIds<T>(DbContext context, int count) where T : class
        {
            var target = GetTarget<T>(context, count);
            context.Database.OpenConnection();
            try
            {
                for (var attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
                {
                    using (var command = CreateCommand(context, INCREMENT_SQL, target.Table, count))
                    {
                        var last = Convert.ToInt32(command.ExecuteScalar());
                        if (last > 0)
                        {
                            return last - count + 1;
                        }
                    }
                    using var init = CreateCommand(context, string.Format(INIT_SQL, target.Table, target.KeyColumn), target.Table, count);
                    init.ExecuteNonQuery();
                }
            }
            finally
            {
                context.Database.CloseConnection();
            }
            throw new InvalidOperationException($"Id tabel {target.Table} gagal dibuat dari m_sequence");
        }
        public static async Task<int> ReserveIdsAsync<T>(DbContext context, int count) where T : class
        {
            var target = GetTarget<T>(context, count);
            await context.Database.OpenConnectionAsync();
            try
            {
                for (var attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
                {
                    using (var command = CreateCommand(context, INCREMENT_SQL, target.Table, count))
                    {
                        var last = Convert.ToInt32(await command.ExecuteScalarAsync());
                        if (last > 0)
                        {
                            return last - count + 1;
                        }
                    }
                    using var init = CreateCommand(context, string.Format(INIT_SQL, target.Table, target.KeyColumn), target.Table, count);
                    await init.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
            throw new InvalidOperationException($"Id tabel {target.Table} gagal dibuat dari m_sequence");
        }

        // Id terbesar yang sudah ada di tabel; dipakai master Sequence agar last_id tidak diturunkan di bawahnya.
        internal static int GetMaxId(DbContext context, string tableName)
        {
            var entity = context.Model.GetEntityTypes().FirstOrDefault(x => x.GetTableName() == tableName);
            Target target;
            try
            {
                target = GetTarget(entity ?? throw new InvalidOperationException(), 1);
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.SEQUENCE_TABLE_INVALID, tableName));
            }
            context.Database.OpenConnection();
            try
            {
                using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = $"SELECT IFNULL(MAX(`{target.KeyColumn}`), 0) FROM `{target.Table}`";
                command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
                return Convert.ToInt32(command.ExecuteScalar());
            }
            finally
            {
                context.Database.CloseConnection();
            }
        }

        private record Target(string Table, string KeyColumn);

        private static Target GetTarget<T>(DbContext context, int count) where T : class
        {
            var entity = context.Model.FindEntityType(typeof(T))
                ?? throw new InvalidOperationException($"{typeof(T).Name} bukan entity ModelContext");
            return GetTarget(entity, count);
        }
        // Nama tabel dan kolom key diambil dari model EF (bukan input), lalu tetap divalidasi karena disisipkan ke SQL.
        private static Target GetTarget(IEntityType entity, int count)
        {
            if (count < 1)
            {
                throw new InvalidOperationException("Jumlah id yang dipesan minimal 1");
            }
            if (entity.ClrType == typeof(TElectionVote))
            {
                throw new InvalidOperationException("Id t_election_vote dibuat acak, bukan dari m_sequence");
            }
            var table = entity.GetTableName()
                ?? throw new InvalidOperationException($"{entity.ClrType.Name} tidak dipetakan ke tabel");
            var key = entity.FindPrimaryKey();
            if (key == null || key.Properties.Count != 1 || key.Properties[0].ClrType != typeof(int))
            {
                throw new InvalidOperationException($"Tabel {table} tidak memakai primary key int tunggal");
            }
            var column = key.Properties[0].GetColumnName();
            if (!IdentifierRegex.IsMatch(table) || !IdentifierRegex.IsMatch(column))
            {
                throw new InvalidOperationException($"Nama tabel / kolom {table}.{column} tidak valid");
            }
            return new Target(table, column);
        }
        // Command memakai koneksi dan transaksi aktif context, jadi ikut BeginTransaction milik pemanggil.
        private static DbCommand CreateCommand(DbContext context, string sql, string table, int count)
        {
            var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
            AddParameter(command, "@table", table);
            AddParameter(command, "@count", count);
            AddParameter(command, "@now", DateTime.Now);
            return command;
        }
        private static void AddParameter(DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
    }
}
