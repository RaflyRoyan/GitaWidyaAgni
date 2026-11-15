using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Akses
{
    public partial class Sequence
    {
        #region SYNC
        public static List<Sequence> GetDataList(int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery();

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Sequence, EF.MSequence>();
        }
        public static Response<Sequence> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow);
                context.MSequences.Add(row);
                context.SaveChanges();
                return new Sequence(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Sequence>(msg));
        }
        public Response<Sequence> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, TableName);
                SetRow(context, row);
                context.SaveChanges();
                return new Sequence(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Sequence>(msg));
        }
        public Response<Sequence> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, TableName);
                context.MSequences.Remove(row);
                context.SaveChanges();
                return new Sequence(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Sequence>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<Sequence>> GetDataListAsync(int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery();

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Sequence, EF.MSequence>();
        }
        public static async Task<Response<Sequence>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow);
                context.MSequences.Add(row);
                await context.SaveChangesAsync();
                return new Sequence(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Sequence>(msg));
        }
        public async Task<Response<Sequence>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, TableName);
                SetRow(context, row);
                await context.SaveChangesAsync();
                return new Sequence(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Sequence>(msg));
        }
        public async Task<Response<Sequence>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, TableName);
                context.MSequences.Remove(row);
                await context.SaveChangesAsync();
                return new Sequence(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Sequence>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.MSequence> FilterQuery()
        {
            return _Querys.OrderBy(x => x.TableName);
        }
        private static EF.MSequence GetRow(EF.ModelContext context, string tableName)
        {
            return context.MSequences.SingleOrDefault(x => x.TableName == tableName)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sequence"));
        }
        private static async Task<EF.MSequence> GetRowAsync(EF.ModelContext context, string tableName)
        {
            return await context.MSequences.SingleOrDefaultAsync(x => x.TableName == tableName)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sequence"));
        }
        // Nama tabel harus tabel ModelContext yang id-nya dari m_sequence (GetMaxId menolak yang lain).
        private static void ValidateLastId(EF.ModelContext context, string tableName, int lastId)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Tabel"));
            }
            if (lastId < 0)
            {
                throw new ArgumentException(ExceptionMessage.SEQUENCE_LAST_ID_INVALID);
            }
            var maxId = SequenceUtility.GetMaxId(context, tableName);
            if (lastId < maxId)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.SEQUENCE_LAST_ID_BELOW_MAX, tableName, maxId));
            }
        }
        private static EF.MSequence CreateRow(EF.ModelContext context, NewRow newrow)
        {
            var tableName = newrow.TableName?.Trim().ToLowerInvariant() ?? string.Empty;
            ValidateLastId(context, tableName, newrow.LastId);
            ValidateUtility.CheckDuplicateData<EF.MSequence>(context, x => x.TableName == tableName,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Tabel"));

            var row = new EF.MSequence
            {
                TableName = tableName,
                LastId = newrow.LastId,
                UpdDate = DateTime.Now
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.MSequence row)
        {
            ValidateLastId(context, row.TableName, LastId);

            row.LastId = LastId;
            row.UpdDate = DateTime.Now;
        }
        #endregion
    }
}
