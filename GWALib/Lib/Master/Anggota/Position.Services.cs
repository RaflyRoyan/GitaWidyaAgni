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

namespace GWALib.Lib.Master.Anggota
{
    public partial class Position
    {
        #region SYNC
        public static List<Position> GetDataList(FilterAktif aktif, PositionLevel? isCore = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, isCore);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Position, EF.MPosition>();
        }
        public static Response<Position> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MPositions.Add(row);
                context.SaveChanges();
                return new Position(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Position>(msg));
        }
        public Response<Position> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new Position(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Position>(msg));
        }
        public Response<Position> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new Position(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Position>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<Position>> GetDataListAsync(FilterAktif aktif, PositionLevel? isCore = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, isCore);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Position, EF.MPosition>();
        }
        public static async Task<Response<Position>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MPositions.Add(row);
                await context.SaveChangesAsync();
                return new Position(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Position>(msg));
        }
        public async Task<Response<Position>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new Position(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Position>(msg));
        }
        public async Task<Response<Position>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new Position(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Position>(msg));
        }
        #endregion

        #region HELPER
        internal static void ValidateActivePosition(EF.ModelContext context, int positionId)
        {
            var row = context.MPositions.SingleOrDefault(x => x.Id == positionId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Jabatan"));
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Jabatan"));
            }
        }
        private static IQueryable<EF.MPosition> FilterQuery(FilterAktif aktif, PositionLevel? isCore)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (isCore == null || x.IsCore == (int)isCore))
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name);
        }
        private static EF.MPosition GetRow(EF.ModelContext context, int id)
        {
            return context.MPositions.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Jabatan"));
        }
        private static async Task<EF.MPosition> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MPositions.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Jabatan"));
        }
        private static void ValidateData(EF.ModelContext context, int id, string name, PositionLevel isCore)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Jabatan"));
            }
            if (!Enum.IsDefined(isCore))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Level Jabatan"));
            }
            ValidateUtility.CheckDuplicateData<EF.MPosition>(context, x => x.Name == name && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Jabatan"));
        }
        private static EF.MPosition CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            ValidateData(context, 0, name, newrow.IsCore);

            var row = new EF.MPosition
            {
                Id = SequenceUtility.GetNextId<EF.MPosition>(context),
                Name = name,
                IsCore = (int)newrow.IsCore,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                SortOrder = newrow.SortOrder,
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.MPosition row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            ValidateData(context, row.Id, name, IsCore);

            row.Name = name;
            row.IsCore = (int)IsCore;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.SortOrder = SortOrder;
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MPosition row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
