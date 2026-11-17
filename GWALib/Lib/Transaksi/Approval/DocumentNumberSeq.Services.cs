using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Approval;
using GWALib.Lib.Master.Organization;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Approval
{
    public partial class DocumentNumberSeq
    {
        public const string PERIOD_KEY_ALL = "ALL";
        private const string ORG_PLACEHOLDER = "{org}";
        private const int MAX_INSERT_ATTEMPTS = 3;

        // UPDATE + LAST_INSERT_ID(expr) membuat kenaikan nomor atomik per baris; ROW_COUNT() = 0 berarti baris periode belum ada.
        private const string INCREMENT_SQL = @"UPDATE t_document_number_seq
SET last_number = LAST_INSERT_ID(last_number + 1), upd_date = @now, upd_by = @user
WHERE document_type = @type AND period_key = @period;
SELECT IF(ROW_COUNT() > 0, LAST_INSERT_ID(), 0);";
        // Id dari m_sequence. IGNORE: bila request lain lebih dulu membuat baris yang sama (uq_docnum_seq),
        // insert ini dilewati (id yang sudah dipesan menjadi celah) dan INCREMENT_SQL dicoba lagi.
        private const string INSERT_SQL = @"INSERT IGNORE INTO t_document_number_seq (id, document_type, period_key, last_number, ins_date, ins_by)
VALUES (@id, @type, @period, 0, @now, @user);";

        // OrgCode = isi {org} (kode profil organisasi), null bila format tidak memakai {org}.
        private record NumberSource(EF.MDocumentNumberFormat Format, string? OrgCode, string DocumentType, string PeriodKey);

        #region SYNC
        public static List<DocumentNumberSeq> GetDataList(string? documentType = null, string? periodKey = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(documentType, periodKey);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<DocumentNumberSeq, EF.TDocumentNumberSeq>();
        }
        // Pratinjau nomor berikutnya tanpa memakai nomor (bisa berbeda bila ada dokumen lain yang lebih dulu terbit).
        public static Response<string> GetNextNumber(string documentType, DateTime date)
        {
            return ServiceHandler.Handle(() =>
            {
                using var context = DBClass.GetContext();
                var source = GetSource(context, documentType, date);
                var lastNumber = context.TDocumentNumberSeqs
                    .Where(x => x.DocumentType == source.DocumentType && x.PeriodKey == source.PeriodKey)
                    .Select(x => (int?)x.LastNumber).FirstOrDefault() ?? 0;
                return Render(source, lastNumber + 1, date);
            }, data => ResponseFactory.DataFound(data), msg => ResponseFactory.Error<string>(msg));
        }
        // Mengambil (memakai) satu nomor dokumen; nomor yang sudah diambil tidak dikembalikan.
        public static Response<string> GenerateNumber(string documentType, DateTime date, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                return GenerateNumber(context, documentType, date, userId);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<string>(msg));
        }
        // Koreksi nomor terakhir (mis. melanjutkan penomoran manual); hanya boleh naik supaya nomor tidak terbit dua kali.
        public Response<DocumentNumberSeq> UpdateLastNumber(int lastNumber, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetLastNumber(row, lastNumber, userId);
                context.SaveChanges();
                return new DocumentNumberSeq(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DocumentNumberSeq>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<DocumentNumberSeq>> GetDataListAsync(string? documentType = null, string? periodKey = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(documentType, periodKey);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<DocumentNumberSeq, EF.TDocumentNumberSeq>();
        }
        public static async Task<Response<string>> GetNextNumberAsync(string documentType, DateTime date)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                using var context = DBClass.GetContext();
                var source = await GetSourceAsync(context, documentType, date);
                var lastNumber = await context.TDocumentNumberSeqs
                    .Where(x => x.DocumentType == source.DocumentType && x.PeriodKey == source.PeriodKey)
                    .Select(x => (int?)x.LastNumber).FirstOrDefaultAsync() ?? 0;
                return Render(source, lastNumber + 1, date);
            }, data => ResponseFactory.DataFound(data), msg => ResponseFactory.Error<string>(msg));
        }
        public static async Task<Response<string>> GenerateNumberAsync(string documentType, DateTime date, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                return await GenerateNumberAsync(context, documentType, date, userId);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<string>(msg));
        }
        public async Task<Response<DocumentNumberSeq>> UpdateLastNumberAsync(int lastNumber, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetLastNumber(row, lastNumber, userId);
                await context.SaveChangesAsync();
                return new DocumentNumberSeq(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DocumentNumberSeq>(msg));
        }
        #endregion

        #region INTERNAL
        // Dipanggil transaksi dokumen (surat, pengadaan, ...) dengan context-nya sendiri. Kenaikan nomor langsung dieksekusi
        // ke DB (bukan menunggu SaveChanges); bungkus dengan context.Database.BeginTransaction() supaya nomor ikut
        // di-rollback bila dokumen gagal disimpan, tanpa transaksi nomor tetap terpakai (nomor loncat).
        internal static string GenerateNumber(EF.ModelContext context, string documentType, DateTime date, int? userId)
        {
            var source = GetSource(context, documentType, date);
            return Render(source, Increment(context, source, userId), date);
        }
        internal static async Task<string> GenerateNumberAsync(EF.ModelContext context, string documentType, DateTime date, int? userId)
        {
            var source = await GetSourceAsync(context, documentType, date);
            return Render(source, await IncrementAsync(context, source, userId), date);
        }
        #endregion

        #region HELPER
        public static string ToPeriodKey(NumberResetPeriod resetPeriod, DateTime date)
        {
            return resetPeriod switch
            {
                NumberResetPeriod.Tahunan => date.ToString("yyyy"),
                NumberResetPeriod.Bulanan => date.ToString("yyyy-MM"),
                _ => PERIOD_KEY_ALL
            };
        }
        private static string Render(NumberSource source, int seq, DateTime date)
        {
            return DocumentNumberFormat.Render(source.Format.NumberFormat, source.Format.SeqPadding, seq, source.DocumentType, source.OrgCode, date);
        }
        // Profil organisasi hanya diwajibkan bila format memakai {org}.
        private static NumberSource ToSource(EF.MDocumentNumberFormat? format, string type, Func<string?> getOrgCode, DateTime date)
        {
            if (format == null)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.NUMBER_FORMAT_NOT_SET, type));
            }
            string? orgCode = null;
            if (format.NumberFormat.Contains(ORG_PLACEHOLDER))
            {
                orgCode = getOrgCode() ?? throw new ArgumentException(ExceptionMessage.ORGANIZATION_NOT_SET);
            }
            return new NumberSource(format, orgCode, type, ToPeriodKey((NumberResetPeriod)format.ResetPeriod, date));
        }
        private static NumberSource GetSource(EF.ModelContext context, string documentType, DateTime date)
        {
            var type = ValidateDocumentType(documentType);
            var format = DocumentNumberFormat.GetEffectiveRow(context, type);
            return ToSource(format, type, () => OrganizationProfile.GetCode(context), date);
        }
        private static async Task<NumberSource> GetSourceAsync(EF.ModelContext context, string documentType, DateTime date)
        {
            var type = ValidateDocumentType(documentType);
            var format = await DocumentNumberFormat.GetEffectiveRowAsync(context, type);
            return ToSource(format, type, () => OrganizationProfile.GetCode(context), date);
        }
        private static string ValidateDocumentType(string documentType)
        {
            var type = DocumentNumberFormat.ToTypeCode(documentType);
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Jenis Dokumen"));
            }
            return type;
        }
        private static int Increment(EF.ModelContext context, NumberSource source, int? userId)
        {
            context.Database.OpenConnection();
            try
            {
                for (var attempt = 0; attempt < MAX_INSERT_ATTEMPTS; attempt++)
                {
                    using (var command = CreateCommand(context, INCREMENT_SQL, source, userId))
                    {
                        var seq = Convert.ToInt32(command.ExecuteScalar());
                        if (seq > 0)
                        {
                            return seq;
                        }
                    }
                    using var insert = CreateCommand(context, INSERT_SQL, source, userId);
                    AddParameter(insert, "@id", SequenceUtility.GetNextId<EF.TDocumentNumberSeq>(context));
                    insert.ExecuteNonQuery();
                }
            }
            finally
            {
                context.Database.CloseConnection();
            }
            throw new InvalidOperationException($"Nomor urut {source.DocumentType} periode {source.PeriodKey} gagal dibuat");
        }
        private static async Task<int> IncrementAsync(EF.ModelContext context, NumberSource source, int? userId)
        {
            await context.Database.OpenConnectionAsync();
            try
            {
                for (var attempt = 0; attempt < MAX_INSERT_ATTEMPTS; attempt++)
                {
                    using (var command = CreateCommand(context, INCREMENT_SQL, source, userId))
                    {
                        var seq = Convert.ToInt32(await command.ExecuteScalarAsync());
                        if (seq > 0)
                        {
                            return seq;
                        }
                    }
                    using var insert = CreateCommand(context, INSERT_SQL, source, userId);
                    AddParameter(insert, "@id", await SequenceUtility.GetNextIdAsync<EF.TDocumentNumberSeq>(context));
                    await insert.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
            throw new InvalidOperationException($"Nomor urut {source.DocumentType} periode {source.PeriodKey} gagal dibuat");
        }
        // Command memakai koneksi dan transaksi aktif context, jadi ikut BeginTransaction milik pemanggil.
        private static DbCommand CreateCommand(EF.ModelContext context, string sql, NumberSource source, int? userId)
        {
            var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
            AddParameter(command, "@type", source.DocumentType);
            AddParameter(command, "@period", source.PeriodKey);
            AddParameter(command, "@user", (object?)userId ?? DBNull.Value);
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
        private static void SetLastNumber(EF.TDocumentNumberSeq row, int lastNumber, int userId)
        {
            if (lastNumber < row.LastNumber)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DOCUMENT_SEQ_DECREASE, row.LastNumber));
            }
            row.LastNumber = lastNumber;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static IQueryable<EF.TDocumentNumberSeq> FilterQuery(string? documentType, string? periodKey)
        {
            var type = string.IsNullOrWhiteSpace(documentType) ? null : DocumentNumberFormat.ToTypeCode(documentType);
            var period = string.IsNullOrWhiteSpace(periodKey) ? null : periodKey.Trim().ToUpperInvariant();
            return _Querys.Where(x => (type == null || x.DocumentType == type)
                    && (period == null || x.PeriodKey == period))
                .OrderBy(x => x.DocumentType).ThenByDescending(x => x.PeriodKey);
        }
        private static EF.TDocumentNumberSeq GetRow(EF.ModelContext context, int id)
        {
            return context.TDocumentNumberSeqs.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Nomor Urut Dokumen"));
        }
        private static async Task<EF.TDocumentNumberSeq> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TDocumentNumberSeqs.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Nomor Urut Dokumen"));
        }
        #endregion
    }
}
