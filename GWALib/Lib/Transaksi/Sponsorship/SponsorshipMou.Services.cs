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
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Sponsorship
{
    public partial class SponsorshipMou
    {
        public const string NUMBER_DOCUMENT_TYPE = "MOU";
        private const string NUMBER_FALLBACK_PREFIX = "MOU";
        public const int MAX_DOCUMENT_BYTES = 10 * 1024 * 1024;
        private const int NOTES_LENGTH = 500;

        #region SYNC
        public FileContent? GetDocument()
        {
            using var context = DBClass.GetContext();
            var file = context.TSponsorshipMous.Where(x => x.Id == Id).Select(x => new { x.Document, x.DocumentFileName, x.DocumentContentType }).SingleOrDefault();
            return file?.Document == null ? null : new FileContent(file.Document, file.DocumentFileName ?? "mou", file.DocumentContentType);
        }
        public static List<SponsorshipMou> GetDataList(int sponsorshipId)
        {
            return FilterQuery(sponsorshipId).ToList().MappingData<SponsorshipMou, EF.TSponsorshipMou>();
        }
        // MoU sponsorship Aktif yang belum berakhir pada tanggal (default hari ini); dipakai berita acara serah terima.
        public static List<SponsorshipMou> GetActiveList(DateOnly? date = null)
        {
            return ActiveQuery(date ?? DateOnly.FromDateTime(DateTime.Today)).ToList().MappingData<SponsorshipMou, EF.TSponsorshipMou>();
        }
        public static Response<SponsorshipMou> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                transaction.Commit();
                return new SponsorshipMou(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<SponsorshipMou>(msg));
        }
        public Response<SponsorshipMou> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new SponsorshipMou(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipMou>(msg));
        }
        // file null / kosong = dokumen dihapus.
        public Response<SponsorshipMou> UpdateDocument(byte[]? file, string? fileName, string? contentType, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateSponsorship(context, row.SponsorshipId);
                SetDocument(row, file, fileName, contentType);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                context.SaveChanges();
                return new SponsorshipMou(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipMou>(msg));
        }
        public Response<SponsorshipMou> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateDelete(context, row);
                var data = new SponsorshipMou(Reload(row.Id));
                context.TSponsorshipMous.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<SponsorshipMou>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetDocumentAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TSponsorshipMous.Where(x => x.Id == Id).Select(x => new { x.Document, x.DocumentFileName, x.DocumentContentType }).SingleOrDefaultAsync();
            return file?.Document == null ? null : new FileContent(file.Document, file.DocumentFileName ?? "mou", file.DocumentContentType);
        }
        public static async Task<List<SponsorshipMou>> GetDataListAsync(int sponsorshipId)
        {
            return await (await FilterQuery(sponsorshipId).ToListAsync()).MappingDataAsync<SponsorshipMou, EF.TSponsorshipMou>();
        }
        public static async Task<List<SponsorshipMou>> GetActiveListAsync(DateOnly? date = null)
        {
            return await (await ActiveQuery(date ?? DateOnly.FromDateTime(DateTime.Today)).ToListAsync()).MappingDataAsync<SponsorshipMou, EF.TSponsorshipMou>();
        }
        public static async Task<Response<SponsorshipMou>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new SponsorshipMou(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<SponsorshipMou>(msg));
        }
        public async Task<Response<SponsorshipMou>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new SponsorshipMou(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipMou>(msg));
        }
        public async Task<Response<SponsorshipMou>> UpdateDocumentAsync(byte[]? file, string? fileName, string? contentType, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateSponsorship(context, row.SponsorshipId);
                SetDocument(row, file, fileName, contentType);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                await context.SaveChangesAsync();
                return new SponsorshipMou(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipMou>(msg));
        }
        public async Task<Response<SponsorshipMou>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateDelete(context, row);
                var data = new SponsorshipMou(Reload(row.Id));
                context.TSponsorshipMous.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<SponsorshipMou>(msg));
        }
        #endregion

        #region HELPER
        private static EF.TSponsorshipMou Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TSponsorshipMou> FilterQuery(int sponsorshipId)
        {
            return _Querys.Where(x => x.SponsorshipId == sponsorshipId).OrderBy(x => x.StartDate).ThenBy(x => x.Id);
        }
        private static IQueryable<EF.TSponsorshipMou> ActiveQuery(DateOnly date)
        {
            return _Querys.Where(x => x.Sponsorship.Status == (int)SponsorshipStatus.Aktif && (x.EndDate == null || x.EndDate >= date))
                .OrderBy(x => x.EndDate).ThenBy(x => x.Id);
        }
        private static EF.TSponsorshipMou GetRow(EF.ModelContext context, int id)
        {
            return context.TSponsorshipMous.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "MoU Sponsorship"));
        }
        private static async Task<EF.TSponsorshipMou> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TSponsorshipMous.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "MoU Sponsorship"));
        }
        // Sponsorship harus ada dan tidak Batal (Selesai masih boleh, mis. melengkapi dokumen).
        private static EF.TSponsorship ValidateSponsorship(EF.ModelContext context, int sponsorshipId)
        {
            var row = context.TSponsorships.SingleOrDefault(x => x.Id == sponsorshipId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sponsorship"));
            TransaksiHelper.ValidateStatus((SponsorshipStatus)row.Status, "mengubah MoU sponsorship", SponsorshipStatus.Aktif, SponsorshipStatus.Selesai);
            return row;
        }
        private static void ValidateDates(DateOnly? startDate, DateOnly? endDate)
        {
            if (startDate != null && endDate != null && endDate < startDate)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
        }
        private static void ValidateDelete(EF.ModelContext context, EF.TSponsorshipMou row)
        {
            ValidateSponsorship(context, row.SponsorshipId);
            if (context.TPeriodHandoverItems.Any(x => x.ItemType == (int)HandoverItemType.MouAktif && x.RefId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.MOU_IN_HANDOVER);
            }
        }
        private static void SetDocument(EF.TSponsorshipMou row, byte[]? file, string? fileName, string? contentType)
        {
            if (file == null || file.Length == 0)
            {
                row.Document = null;
                row.DocumentFileName = null;
                row.DocumentContentType = null;
                return;
            }
            row.DocumentFileName = TransaksiHelper.ValidateFile(file, fileName, "File MoU", MAX_DOCUMENT_BYTES);
            row.Document = file;
            row.DocumentContentType = TransaksiHelper.ToNullable(contentType);
        }
        private static EF.TSponsorshipMou CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            ValidateSponsorship(context, newrow.SponsorshipId);
            ValidateDates(newrow.StartDate, newrow.EndDate);
            var id = SequenceUtility.GetNextId<EF.TSponsorshipMou>(context);
            var number = TransaksiHelper.ToNullable(newrow.MouNumber)
                ?? TransaksiHelper.GenerateNumber(context, NUMBER_DOCUMENT_TYPE, (newrow.SignedDate ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue),
                    NUMBER_FALLBACK_PREFIX, id, userId);
            ValidateUtility.CheckDuplicateData<EF.TSponsorshipMou>(context, x => x.MouNumber == number, string.Format(ExceptionMessage.DATA_DUPLICATE, "Nomor MoU"));

            var row = new EF.TSponsorshipMou
            {
                Id = id,
                SponsorshipId = newrow.SponsorshipId,
                MouNumber = number,
                SignedDate = newrow.SignedDate,
                StartDate = newrow.StartDate,
                EndDate = newrow.EndDate,
                Notes = TransaksiHelper.Truncate(newrow.Notes, NOTES_LENGTH),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            SetDocument(row, newrow.Document, newrow.DocumentFileName, newrow.DocumentContentType);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TSponsorshipMous.Add(row);
            return row;
        }
        // Sponsorship induk tetap; dokumen tidak diubah di sini (UpdateDocument).
        private void SetRow(EF.ModelContext context, EF.TSponsorshipMou row, int userId)
        {
            ValidateSponsorship(context, row.SponsorshipId);
            ValidateDates(StartDate, EndDate);
            var number = TransaksiHelper.ToNullable(MouNumber)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nomor MoU"));
            ValidateUtility.CheckDuplicateData<EF.TSponsorshipMou>(context, x => x.MouNumber == number && x.Id != row.Id, string.Format(ExceptionMessage.DATA_DUPLICATE, "Nomor MoU"));

            row.MouNumber = number;
            row.SignedDate = SignedDate;
            row.StartDate = StartDate;
            row.EndDate = EndDate;
            row.Notes = TransaksiHelper.Truncate(Notes, NOTES_LENGTH);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        #endregion
    }
}
