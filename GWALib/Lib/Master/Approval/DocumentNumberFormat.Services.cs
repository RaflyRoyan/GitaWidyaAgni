using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Approval
{
    public partial class DocumentNumberFormat
    {
        public const int MAX_SEQ_PADDING = 10;
        private const string SEQ_PLACEHOLDER = "seq";
        private static readonly string[] PLACEHOLDERS = { "seq", "type", "org", "month_roman", "month", "year", "yy" };
        private static readonly string[] ROMAN_MONTHS = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII" };

        #region SYNC
        // {org} diisi kode organisasi dari profil; "ORG" bila profil belum diisi.
        public string GetPreview(int seq = 1)
        {
            using var context = DBClass.GetContext();
            return Render(NumberFormat, SeqPadding, seq, DocumentType, Organization.OrganizationProfile.GetCode(context) ?? "ORG", DateTime.Today);
        }
        public static DocumentNumberFormat? GetEffective(string documentType)
        {
            var row = EffectiveQuery(_Querys, ToTypeCode(documentType)).FirstOrDefault();
            return row != null ? new DocumentNumberFormat(row) : null;
        }
        public static List<DocumentNumberFormat> GetDataList(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<DocumentNumberFormat, EF.MDocumentNumberFormat>();
        }
        public static Response<DocumentNumberFormat> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MDocumentNumberFormats.Add(row);
                context.SaveChanges();
                return new DocumentNumberFormat(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<DocumentNumberFormat>(msg));
        }
        public Response<DocumentNumberFormat> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(row, userId);
                context.SaveChanges();
                return new DocumentNumberFormat(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DocumentNumberFormat>(msg));
        }
        public Response<DocumentNumberFormat> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new DocumentNumberFormat(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<DocumentNumberFormat>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<DocumentNumberFormat?> GetEffectiveAsync(string documentType)
        {
            var row = await EffectiveQuery(_Querys, ToTypeCode(documentType)).FirstOrDefaultAsync();
            return row != null ? new DocumentNumberFormat(row) : null;
        }
        public static async Task<List<DocumentNumberFormat>> GetDataListAsync(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<DocumentNumberFormat, EF.MDocumentNumberFormat>();
        }
        public static async Task<Response<DocumentNumberFormat>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MDocumentNumberFormats.Add(row);
                await context.SaveChangesAsync();
                return new DocumentNumberFormat(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<DocumentNumberFormat>(msg));
        }
        public async Task<Response<DocumentNumberFormat>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(row, userId);
                await context.SaveChangesAsync();
                return new DocumentNumberFormat(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<DocumentNumberFormat>(msg));
        }
        public async Task<Response<DocumentNumberFormat>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new DocumentNumberFormat(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<DocumentNumberFormat>(msg));
        }
        #endregion

        #region HELPER
        // Mengisi placeholder format: {seq} di-pad nol sesuai seqPadding, {month_roman} = I..XII, {yy} = 2 digit tahun.
        internal static string Render(string numberFormat, int seqPadding, int seq, string documentType, string? orgCode, DateTime date)
        {
            return Regex.Replace(numberFormat, @"\{(\w+)\}", m => m.Groups[1].Value switch
            {
                "seq" => seq.ToString().PadLeft(seqPadding, '0'),
                "type" => documentType,
                "org" => orgCode ?? string.Empty,
                "month_roman" => ROMAN_MONTHS[date.Month - 1],
                "month" => date.Month.ToString("00"),
                "year" => date.Year.ToString(),
                "yy" => (date.Year % 100).ToString("00"),
                _ => m.Value
            });
        }
        // Kode jenis dokumen: huruf besar, spasi / tanda baca jadi garis bawah (mis. "surat und" -> SURAT_UND).
        internal static string ToTypeCode(string? value)
        {
            return Regex.Replace(value?.Trim().ToUpperInvariant() ?? string.Empty, "[^A-Z0-9]+", "_").Trim('_');
        }
        // Dipakai DocumentNumberSeq di dalam context transaksinya; documentType sudah berupa kode (ToTypeCode).
        internal static EF.MDocumentNumberFormat? GetEffectiveRow(EF.ModelContext context, string documentType)
        {
            return EffectiveQuery(context.MDocumentNumberFormats, documentType).FirstOrDefault();
        }
        internal static async Task<EF.MDocumentNumberFormat?> GetEffectiveRowAsync(EF.ModelContext context, string documentType)
        {
            return await EffectiveQuery(context.MDocumentNumberFormats, documentType).FirstOrDefaultAsync();
        }
        private static IQueryable<EF.MDocumentNumberFormat> EffectiveQuery(IQueryable<EF.MDocumentNumberFormat> source, string documentType)
        {
            return source.Where(x => x.DocumentType == documentType && x.IsActive == (int)ActiveStatus.Aktif);
        }
        private static IQueryable<EF.MDocumentNumberFormat> FilterQuery(FilterAktif aktif)
        {
            return _Querys.Where(x => x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                .OrderBy(x => x.DocumentType);
        }
        private static EF.MDocumentNumberFormat GetRow(EF.ModelContext context, int id)
        {
            return context.MDocumentNumberFormats.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Format Nomor Dokumen"));
        }
        private static async Task<EF.MDocumentNumberFormat> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MDocumentNumberFormats.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Format Nomor Dokumen"));
        }
        // Format wajib memuat {seq} dan hanya boleh memakai placeholder yang dikenal.
        private static void ValidateData(string numberFormat, int seqPadding, NumberResetPeriod resetPeriod)
        {
            if (string.IsNullOrWhiteSpace(numberFormat))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Format Nomor"));
            }
            var placeholders = Regex.Matches(numberFormat, @"\{(\w+)\}").Select(m => m.Groups[1].Value).ToList();
            var unknown = placeholders.FirstOrDefault(p => !PLACEHOLDERS.Contains(p));
            if (unknown != null)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.NUMBER_FORMAT_PLACEHOLDER_INVALID, "{" + unknown + "}",
                    string.Join(" ", PLACEHOLDERS.Select(p => "{" + p + "}"))));
            }
            if (!placeholders.Contains(SEQ_PLACEHOLDER))
            {
                throw new ArgumentException(ExceptionMessage.NUMBER_FORMAT_SEQ_REQUIRED);
            }
            if (seqPadding < 1 || seqPadding > MAX_SEQ_PADDING)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jumlah Digit Nomor Urut"));
            }
            if (!Enum.IsDefined(resetPeriod))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Periode Reset"));
            }
        }
        private static EF.MDocumentNumberFormat CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var documentType = ToTypeCode(newrow.DocumentType);
            var numberFormat = newrow.NumberFormat?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(documentType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Jenis Dokumen"));
            }
            ValidateData(numberFormat, newrow.SeqPadding, newrow.ResetPeriod);
            ValidateUtility.CheckDuplicateData<EF.MDocumentNumberFormat>(context,
                x => x.DocumentType == documentType,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Jenis Dokumen"));

            var row = new EF.MDocumentNumberFormat
            {
                Id = SequenceUtility.GetNextId<EF.MDocumentNumberFormat>(context),
                DocumentType = documentType,
                NumberFormat = numberFormat,
                SeqPadding = newrow.SeqPadding,
                ResetPeriod = (int)newrow.ResetPeriod,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Jenis dokumen tetap (nomor urut yang sudah terbit mengacu ke jenis ini).
        private void SetRow(EF.MDocumentNumberFormat row, int userId)
        {
            var numberFormat = NumberFormat?.Trim() ?? string.Empty;
            ValidateData(numberFormat, SeqPadding, ResetPeriod);

            row.NumberFormat = numberFormat;
            row.SeqPadding = SeqPadding;
            row.ResetPeriod = (int)ResetPeriod;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MDocumentNumberFormat row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
