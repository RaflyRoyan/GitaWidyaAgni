using GWALib.General;
using GWALib.Lib.Transaksi.Information;
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

namespace GWALib.Lib.Transaksi.Formulir
{
    public partial class Aspiration
    {
        private const int TEXT_LENGTH = 2000;

        #region SYNC
        public static List<Aspiration> GetDataList(AspirationStatus? status = null, AspirationCategory? category = null, int startdata = -1, int jmldata = 50)
        {
            return Page(FilterQuery(status, category, null), startdata, jmldata).ToList().MappingData<Aspiration, EF.TAspiration>();
        }
        // Aspirasi tidak anonim milik user login.
        public static List<Aspiration> GetMyList(string user)
        {
            var userId = ValidateUtility.ValidateUser(user);
            return FilterQuery(null, null, userId).ToList().MappingData<Aspiration, EF.TAspiration>();
        }
        public static Response<Aspiration> Submit(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TAspirations.Add(row);
                context.SaveChanges();
                return new Aspiration(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Aspiration>(msg));
        }
        // Kategori, judul dan isi; hanya pengirim selama Baru.
        public Response<Aspiration> Update(string user)
        {
            return Run(SetRow, user);
        }
        // null = hapus lampiran.
        public Response<Aspiration> UpdateAttachment(byte[]? file, string? fileName, string user)
        {
            return Run((context, row, userId) => SetAttachment(row, file, fileName, userId), user);
        }
        public Response<Aspiration> Respond(AspirationStatus status, string? response, string user)
        {
            return Run((context, row, userId) => SetRespond(context, row, status, response, userId), user);
        }
        public Response<Aspiration> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                ValidateDelete(row, userId);
                context.TAspirations.Remove(row);
                context.SaveChanges();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Aspiration>(msg));
        }
        public FileContent? GetAttachment()
        {
            using var context = DBClass.GetContext();
            return context.TAspirations.Where(x => x.Id == Id && x.Attachment != null)
                .Select(x => new FileContent(x.Attachment!, x.AttachmentFileName ?? "lampiran"))
                .SingleOrDefault();
        }
        #endregion

        #region ASYNC
        public static async Task<List<Aspiration>> GetDataListAsync(AspirationStatus? status = null, AspirationCategory? category = null, int startdata = -1, int jmldata = 50)
        {
            return await (await Page(FilterQuery(status, category, null), startdata, jmldata).ToListAsync()).MappingDataAsync<Aspiration, EF.TAspiration>();
        }
        public static async Task<List<Aspiration>> GetMyListAsync(string user)
        {
            var userId = ValidateUtility.ValidateUser(user);
            return await (await FilterQuery(null, null, userId).ToListAsync()).MappingDataAsync<Aspiration, EF.TAspiration>();
        }
        public static async Task<Response<Aspiration>> SubmitAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TAspirations.Add(row);
                await context.SaveChangesAsync();
                return new Aspiration(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Aspiration>(msg));
        }
        public Task<Response<Aspiration>> UpdateAsync(string user)
        {
            return RunAsync(SetRow, user);
        }
        public Task<Response<Aspiration>> UpdateAttachmentAsync(byte[]? file, string? fileName, string user)
        {
            return RunAsync((context, row, userId) => SetAttachment(row, file, fileName, userId), user);
        }
        public Task<Response<Aspiration>> RespondAsync(AspirationStatus status, string? response, string user)
        {
            return RunAsync((context, row, userId) => SetRespond(context, row, status, response, userId), user);
        }
        public async Task<Response<Aspiration>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                ValidateDelete(row, userId);
                context.TAspirations.Remove(row);
                await context.SaveChangesAsync();
                return this;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Aspiration>(msg));
        }
        public async Task<FileContent?> GetAttachmentAsync()
        {
            using var context = DBClass.GetContext();
            return await context.TAspirations.Where(x => x.Id == Id && x.Attachment != null)
                .Select(x => new FileContent(x.Attachment!, x.AttachmentFileName ?? "lampiran"))
                .SingleOrDefaultAsync();
        }
        #endregion

        #region HELPER
        private delegate void RowAction(EF.ModelContext context, EF.TAspiration row, int userId);

        private Response<Aspiration> Run(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                return new Aspiration(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Aspiration>(msg));
        }
        private async Task<Response<Aspiration>> RunAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                return new Aspiration(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Aspiration>(msg));
        }
        private static EF.TAspiration Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TAspiration> FilterQuery(AspirationStatus? status, AspirationCategory? category, int? userId)
        {
            return _Querys.Where(x => (status == null || x.Status == (int)status)
                    && (category == null || x.Category == (int)category)
                    && (userId == null || x.UserId == userId))
                .OrderByDescending(x => x.InsDate).ThenByDescending(x => x.Id);
        }
        private static IQueryable<EF.TAspiration> Page(IQueryable<EF.TAspiration> query, int startdata, int jmldata)
        {
            return startdata > -1 ? query.Skip(startdata).Take(jmldata) : query;
        }
        private static EF.TAspiration GetRow(EF.ModelContext context, int id)
        {
            return context.TAspirations.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Aspirasi"));
        }
        private static async Task<EF.TAspiration> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TAspirations.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Aspirasi"));
        }
        private static void SetValues(EF.TAspiration row, AspirationCategory category, string? title, string? content)
        {
            if (!Enum.IsDefined(category))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kategori Aspirasi"));
            }
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul"));
            }
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Isi Aspirasi"));
            }
            row.Category = (int)category;
            row.Title = title.Trim();
            row.Content = content.Trim();
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static EF.TAspiration CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            if (!Enum.IsDefined(newrow.IsAnonymous))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Mode Identitas"));
            }
            var anonymous = newrow.IsAnonymous == IdentityMode.Anonim;
            var row = new EF.TAspiration
            {
                // Anonim: identitas pengirim tidak disimpan sama sekali.
                UserId = anonymous ? null : userId,
                IsAnonymous = (int)newrow.IsAnonymous,
                Status = (int)AspirationStatus.Baru,
                InsDate = DateTime.Now,
                InsBy = anonymous ? null : userId
            };
            SetValues(row, newrow.Category, newrow.Title, newrow.Content);
            if (newrow.Attachment != null && newrow.Attachment.Length > 0)
            {
                row.AttachmentFileName = TransaksiHelper.Truncate(
                    TransaksiHelper.ValidateFile(newrow.Attachment, newrow.AttachmentFileName, "Lampiran", MAX_ATTACHMENT_BYTES), 255);
                row.Attachment = newrow.Attachment;
            }
            row.Id = SequenceUtility.GetNextId<EF.TAspiration>(context);
            return row;
        }
        private static void ValidateOwner(EF.TAspiration row, int userId, string action)
        {
            if (row.UserId == null || row.UserId != userId)
            {
                throw new ArgumentException(ExceptionMessage.ASPIRATION_NOT_OWNER);
            }
            TransaksiHelper.ValidateStatus((AspirationStatus)row.Status, action, AspirationStatus.Baru);
        }
        private void SetRow(EF.ModelContext context, EF.TAspiration row, int userId)
        {
            ValidateOwner(row, userId, "mengubah aspirasi");
            SetValues(row, Category, Title, Content);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetAttachment(EF.TAspiration row, byte[]? file, string? fileName, int userId)
        {
            ValidateOwner(row, userId, "mengubah lampiran aspirasi");
            if (file == null || file.Length == 0)
            {
                row.Attachment = null;
                row.AttachmentFileName = null;
            }
            else
            {
                row.AttachmentFileName = TransaksiHelper.Truncate(TransaksiHelper.ValidateFile(file, fileName, "Lampiran", MAX_ATTACHMENT_BYTES), 255);
                row.Attachment = file;
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        // Baru → Diproses / Selesai / Ditolak; Diproses → Selesai / Ditolak. Selesai / Ditolak wajib tanggapan.
        private static void SetRespond(EF.ModelContext context, EF.TAspiration row, AspirationStatus status, string? response, int userId)
        {
            var current = (AspirationStatus)row.Status;
            TransaksiHelper.ValidateStatus(current, "menanggapi aspirasi", AspirationStatus.Baru, AspirationStatus.Diproses);
            if (!Enum.IsDefined(status) || status <= current)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Aspirasi"));
            }
            var text = TransaksiHelper.ToNullable(response);
            if (text == null && status != AspirationStatus.Diproses)
            {
                throw new ArgumentException(ExceptionMessage.ASPIRATION_RESPONSE_REQUIRED);
            }
            if (text != null && text.Length > TEXT_LENGTH)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Tanggapan"));
            }
            row.Status = (int)status;
            if (text != null)
            {
                row.Response = text;
                row.RespondedBy = userId;
                row.RespondedAt = DateTime.Now;
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            // Aspirasi anonim tidak menyimpan pengirim, jadi tidak ada penerima.
            NotifyHelper.ToUser(context, row.UserId, NotifyHelper.TYPE_ASPIRATION, $"Aspirasi \"{row.Title}\" {status.GetDescription().ToLowerInvariant()}",
                text, NotifyHelper.LINK_MY_FORM, "t_aspiration", row.Id, userId);
        }
        // Baru: hanya pengirim (aspirasi anonim tidak bisa dihapus pengirim). Ditolak: boleh dihapus.
        private static void ValidateDelete(EF.TAspiration row, int userId)
        {
            if (row.Status == (int)AspirationStatus.Ditolak)
            {
                return;
            }
            ValidateOwner(row, userId, "menghapus aspirasi");
        }
        #endregion
    }
}
