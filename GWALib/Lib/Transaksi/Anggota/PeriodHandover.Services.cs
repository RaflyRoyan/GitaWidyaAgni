using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Transaksi.Approval;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Anggota
{
    public partial class PeriodHandover
    {
        public const string NUMBER_DOCUMENT_TYPE = "BA_ST";
        public const int MAX_LPJ_BYTES = 10 * 1024 * 1024;

        #region SYNC
        public List<PeriodHandoverItem> GetItems(bool refresh)
        {
            if (refresh || ItemList == null)
            {
                ItemList = PeriodHandoverItem.GetDataList(Id);
            }
            return ItemList;
        }
        public FileContent? GetLpj()
        {
            using var context = DBClass.GetContext();
            var file = context.TPeriodHandovers.Where(x => x.Id == Id).Select(x => new { x.LpjDocument, x.LpjFileName }).SingleOrDefault();
            return file?.LpjDocument == null ? null : new FileContent(file.LpjDocument, file.LpjFileName ?? "lpj");
        }
        public static List<PeriodHandover> GetDataList(HandoverStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<PeriodHandover, EF.TPeriodHandover>();
        }
        public static Response<PeriodHandover> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new PeriodHandover(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<PeriodHandover>(msg));
        }
        public Response<PeriodHandover> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new PeriodHandover(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodHandover>(msg));
        }
        // file null / kosong = LPJ dihapus.
        public Response<PeriodHandover> UpdateLpj(byte[]? file, string? fileName, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetLpj(row, file, fileName, userId);
                context.SaveChanges();
                return new PeriodHandover(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodHandover>(msg));
        }
        public Response<PeriodHandover> Submit(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetSubmit(row, userId);
                context.SaveChanges();
                return new PeriodHandover(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodHandover>(msg));
        }
        // Persetujuan + efeknya (periode, penugasan, nomor) dalam satu transaksi DB.
        public Response<PeriodHandover> Approve(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = GetRow(context, Id);
                SetApprove(context, row, userId);
                context.SaveChanges();
                transaction.Commit();
                return new PeriodHandover(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodHandover>(msg));
        }
        public Response<PeriodHandover> Reject(string? notes, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetReject(row, notes, userId);
                context.SaveChanges();
                return new PeriodHandover(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodHandover>(msg));
        }
        // Hanya Draft yang bisa dihapus (item ikut terhapus lewat ON DELETE CASCADE).
        public Response<PeriodHandover> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((HandoverStatus)row.Status, "menghapus berita acara", HandoverStatus.Draft);
                var data = new PeriodHandover(Reload(row.Id));
                context.TPeriodHandovers.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<PeriodHandover>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<PeriodHandoverItem>> GetItemsAsync(bool refresh)
        {
            if (refresh || ItemList == null)
            {
                ItemList = await PeriodHandoverItem.GetDataListAsync(Id);
            }
            return ItemList;
        }
        public async Task<FileContent?> GetLpjAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TPeriodHandovers.Where(x => x.Id == Id).Select(x => new { x.LpjDocument, x.LpjFileName }).SingleOrDefaultAsync();
            return file?.LpjDocument == null ? null : new FileContent(file.LpjDocument, file.LpjFileName ?? "lpj");
        }
        public static async Task<List<PeriodHandover>> GetDataListAsync(HandoverStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<PeriodHandover, EF.TPeriodHandover>();
        }
        public static async Task<Response<PeriodHandover>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new PeriodHandover(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<PeriodHandover>(msg));
        }
        public async Task<Response<PeriodHandover>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new PeriodHandover(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodHandover>(msg));
        }
        public async Task<Response<PeriodHandover>> UpdateLpjAsync(byte[]? file, string? fileName, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetLpj(row, file, fileName, userId);
                await context.SaveChangesAsync();
                return new PeriodHandover(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodHandover>(msg));
        }
        public async Task<Response<PeriodHandover>> SubmitAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetSubmit(row, userId);
                await context.SaveChangesAsync();
                return new PeriodHandover(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodHandover>(msg));
        }
        public async Task<Response<PeriodHandover>> ApproveAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = await GetRowAsync(context, Id);
                SetApprove(context, row, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new PeriodHandover(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodHandover>(msg));
        }
        public async Task<Response<PeriodHandover>> RejectAsync(string? notes, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetReject(row, notes, userId);
                await context.SaveChangesAsync();
                return new PeriodHandover(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodHandover>(msg));
        }
        public async Task<Response<PeriodHandover>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                TransaksiHelper.ValidateStatus((HandoverStatus)row.Status, "menghapus berita acara", HandoverStatus.Draft);
                var data = new PeriodHandover(Reload(row.Id));
                context.TPeriodHandovers.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<PeriodHandover>(msg));
        }
        #endregion

        #region INTERNAL
        // Berita acara yang masih boleh diubah (Draft / Ditolak); dipakai juga PeriodHandoverItem.
        // Local: berita acara yang baru di-Add dalam command yang sama (Save dengan Items) belum ada di DB.
        internal static EF.TPeriodHandover GetEditableHandover(EF.ModelContext context, int handoverId)
        {
            var row = context.TPeriodHandovers.Local.SingleOrDefault(x => x.Id == handoverId)
                ?? context.TPeriodHandovers.SingleOrDefault(x => x.Id == handoverId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Berita Acara Serah Terima"));
            TransaksiHelper.ValidateStatus((HandoverStatus)row.Status, "mengubah berita acara", HandoverStatus.Draft, HandoverStatus.Ditolak);
            return row;
        }
        #endregion

        #region HELPER
        private static EF.TPeriodHandover Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TPeriodHandover> FilterQuery(HandoverStatus? status)
        {
            return _Querys.Where(x => status == null || x.Status == (int)status)
                .OrderByDescending(x => x.HandoverDate).ThenByDescending(x => x.Id);
        }
        private static EF.TPeriodHandover GetRow(EF.ModelContext context, int id)
        {
            return context.TPeriodHandovers.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Berita Acara Serah Terima"));
        }
        private static async Task<EF.TPeriodHandover> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TPeriodHandovers.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Berita Acara Serah Terima"));
        }
        private static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        // Periode tujuan tidak boleh sudah ditutup; penyerah / penerima (bila diisi) harus anggota yang ada.
        private static void ValidateData(EF.ModelContext context, int fromPeriodId, int toPeriodId, DateOnly handoverDate, int? handedBy, int? receivedBy)
        {
            if (fromPeriodId == toPeriodId)
            {
                throw new ArgumentException(ExceptionMessage.HANDOVER_SAME_PERIOD);
            }
            ManagementPeriod.ValidateOpenPeriod(context, toPeriodId);
            if (handoverDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Serah Terima"));
            }
            if (handedBy != null)
            {
                Member.GetMember(context, handedBy.Value);
            }
            if (receivedBy != null)
            {
                Member.GetMember(context, receivedBy.Value);
            }
        }
        private static EF.TPeriodHandover CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            if (!context.MManagementPeriods.Any(x => x.Id == newrow.FromPeriodId))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Periode Asal"));
            }
            ValidateUtility.CheckDuplicateData<EF.TPeriodHandover>(context, x => x.FromPeriodId == newrow.FromPeriodId, ExceptionMessage.HANDOVER_EXISTS);
            var handedBy = newrow.HandedByMemberId > 0 ? newrow.HandedByMemberId : null;
            var receivedBy = newrow.ReceivedByMemberId > 0 ? newrow.ReceivedByMemberId : null;
            ValidateData(context, newrow.FromPeriodId, newrow.ToPeriodId, newrow.HandoverDate, handedBy, receivedBy);

            var row = new EF.TPeriodHandover
            {
                Id = SequenceUtility.GetNextId<EF.TPeriodHandover>(context),
                FromPeriodId = newrow.FromPeriodId,
                ToPeriodId = newrow.ToPeriodId,
                HandoverNumber = ToNullable(newrow.HandoverNumber),
                HandoverDate = newrow.HandoverDate,
                HandedByMemberId = handedBy,
                ReceivedByMemberId = receivedBy,
                Notes = ToNullable(newrow.Notes),
                Status = (int)HandoverStatus.Draft,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TPeriodHandovers.Add(row);

            foreach (var item in newrow.Items ?? new())
            {
                item.HandoverId = row.Id;
                context.TPeriodHandoverItems.Add(PeriodHandoverItem.CreateRow(context, item, userId));
            }
            return row;
        }
        // Periode asal tetap (satu berita acara per periode asal).
        private void SetRow(EF.ModelContext context, EF.TPeriodHandover row, int userId)
        {
            TransaksiHelper.ValidateStatus((HandoverStatus)row.Status, "mengubah berita acara", HandoverStatus.Draft, HandoverStatus.Ditolak);
            var handedBy = HandedByMemberId > 0 ? HandedByMemberId : null;
            var receivedBy = ReceivedByMemberId > 0 ? ReceivedByMemberId : null;
            ValidateData(context, row.FromPeriodId, ToPeriodId, HandoverDate, handedBy, receivedBy);

            row.ToPeriodId = ToPeriodId;
            row.HandoverNumber = ToNullable(HandoverNumber);
            row.HandoverDate = HandoverDate;
            row.HandedByMemberId = handedBy;
            row.ReceivedByMemberId = receivedBy;
            row.Notes = ToNullable(Notes);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetLpj(EF.TPeriodHandover row, byte[]? file, string? fileName, int userId)
        {
            TransaksiHelper.ValidateStatus((HandoverStatus)row.Status, "mengubah LPJ", HandoverStatus.Draft, HandoverStatus.Ditolak);
            if (file == null || file.Length == 0)
            {
                row.LpjDocument = null;
                row.LpjFileName = null;
            }
            else
            {
                if (file.Length > MAX_LPJ_BYTES)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FILE_TOO_LARGE, "File LPJ", MAX_LPJ_BYTES / 1024 / 1024));
                }
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama File LPJ"));
                }
                row.LpjDocument = file;
                row.LpjFileName = Path.GetFileName(fileName.Trim());
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetSubmit(EF.TPeriodHandover row, int userId)
        {
            TransaksiHelper.ValidateStatus((HandoverStatus)row.Status, "mengajukan berita acara", HandoverStatus.Draft, HandoverStatus.Ditolak);
            row.Status = (int)HandoverStatus.Diajukan;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetApprove(EF.ModelContext context, EF.TPeriodHandover row, int userId)
        {
            TransaksiHelper.ValidateStatus((HandoverStatus)row.Status, "menyetujui berita acara", HandoverStatus.Diajukan);
            var toPeriod = ManagementPeriod.ValidateOpenPeriod(context, row.ToPeriodId);
            var fromPeriod = context.MManagementPeriods.Single(x => x.Id == row.FromPeriodId);
            var now = DateTime.Now;

            if (row.HandoverNumber == null && context.MDocumentNumberFormats.Any(x => x.DocumentType == NUMBER_DOCUMENT_TYPE && x.IsActive == (int)ActiveStatus.Aktif))
            {
                row.HandoverNumber = DocumentNumberSeq.GenerateNumber(context, NUMBER_DOCUMENT_TYPE, row.HandoverDate.ToDateTime(TimeOnly.MinValue), userId);
            }
            ManagementAssignment.EndAllInPeriod(context, fromPeriod.Id, row.HandoverDate, userId);
            fromPeriod.Status = (int)ManagementPeriodStatus.Closed;
            fromPeriod.UpdDate = now;
            fromPeriod.UpdBy = userId;
            toPeriod.Status = (int)ManagementPeriodStatus.Active;
            toPeriod.UpdDate = now;
            toPeriod.UpdBy = userId;

            row.Status = (int)HandoverStatus.Disetujui;
            row.ApprovedBy = userId;
            row.ApprovedAt = now;
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        private static void SetReject(EF.TPeriodHandover row, string? notes, int userId)
        {
            TransaksiHelper.ValidateStatus((HandoverStatus)row.Status, "menolak berita acara", HandoverStatus.Diajukan);
            if (!string.IsNullOrWhiteSpace(notes))
            {
                row.Notes = TransaksiHelper.Truncate(string.IsNullOrWhiteSpace(row.Notes) ? "Ditolak: " + notes : row.Notes + " | Ditolak: " + notes, 2000);
            }
            row.Status = (int)HandoverStatus.Ditolak;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
