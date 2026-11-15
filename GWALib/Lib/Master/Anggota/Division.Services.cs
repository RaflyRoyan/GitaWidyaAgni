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
    public partial class Division
    {
        #region SYNC
        public static List<Division> GetDataList(FilterAktif aktif, DivisionType? divisionType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, divisionType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Division, EF.MDivision>();
        }
        public static Response<Division> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MDivisions.Add(row);
                context.SaveChanges();
                return new Division(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Division>(msg));
        }
        public Response<Division> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new Division(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Division>(msg));
        }
        public Response<Division> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new Division(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Division>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<Division>> GetDataListAsync(FilterAktif aktif, DivisionType? divisionType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, divisionType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Division, EF.MDivision>();
        }
        public static async Task<Response<Division>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MDivisions.Add(row);
                await context.SaveChangesAsync();
                return new Division(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Division>(msg));
        }
        public async Task<Response<Division>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new Division(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Division>(msg));
        }
        public async Task<Response<Division>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new Division(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<Division>(msg));
        }
        #endregion

        #region HELPER
        internal static void ValidateActiveDivision(EF.ModelContext context, int divisionId)
        {
            var row = context.MDivisions.SingleOrDefault(x => x.Id == divisionId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Divisi"));
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Divisi"));
            }
        }
        private static IQueryable<EF.MDivision> FilterQuery(FilterAktif aktif, DivisionType? divisionType)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (divisionType == null || x.DivisionType == (int)divisionType))
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name);
        }
        private static EF.MDivision GetRow(EF.ModelContext context, int id)
        {
            return context.MDivisions.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Divisi"));
        }
        private static async Task<EF.MDivision> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MDivisions.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Divisi"));
        }
        private static void ValidateData(EF.ModelContext context, int id, string name, DivisionType divisionType)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Divisi"));
            }
            if (!Enum.IsDefined(divisionType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Divisi"));
            }
            ValidateUtility.CheckDuplicateData<EF.MDivision>(context, x => x.Name == name && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Divisi"));
        }
        private static EF.MDivision CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            ValidateData(context, 0, name, newrow.DivisionType);

            var row = new EF.MDivision
            {
                Id = SequenceUtility.GetNextId<EF.MDivision>(context),
                Name = name,
                DivisionType = (int)newrow.DivisionType,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                SortOrder = newrow.SortOrder,
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.MDivision row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            ValidateData(context, row.Id, name, DivisionType);

            row.Name = name;
            row.DivisionType = (int)DivisionType;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.SortOrder = SortOrder;
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MDivision row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
