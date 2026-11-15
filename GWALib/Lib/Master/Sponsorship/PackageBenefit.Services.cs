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

namespace GWALib.Lib.Master.Sponsorship
{
    public partial class PackageBenefit
    {
        #region SYNC
        public SponsorshipPackage GetPackage(bool refresh = false)
        {
            if (refresh || PackageData == null)
            {
                PackageData = new SponsorshipPackage(PackageId);
            }
            return PackageData;
        }
        public static List<PackageBenefit> GetDataList(int? packageId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(packageId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<PackageBenefit, EF.MSponsorshipPackageBenefit>();
        }
        public static Response<PackageBenefit> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MSponsorshipPackageBenefits.Add(row);
                context.SaveChanges();
                return new PackageBenefit(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<PackageBenefit>(msg));
        }
        public Response<PackageBenefit> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new PackageBenefit(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PackageBenefit>(msg));
        }
        public Response<PackageBenefit> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                context.MSponsorshipPackageBenefits.Remove(row);
                context.SaveChanges();
                return new PackageBenefit(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<PackageBenefit>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<SponsorshipPackage> GetPackageAsync(bool refresh = false)
        {
            if (refresh || PackageData == null)
            {
                using var context = DBClass.GetContext();
                var row = await context.MSponsorshipPackages.SingleAsync(x => x.Id == PackageId);
                PackageData = new SponsorshipPackage(row);
            }
            return PackageData;
        }
        public static async Task<List<PackageBenefit>> GetDataListAsync(int? packageId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(packageId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<PackageBenefit, EF.MSponsorshipPackageBenefit>();
        }
        public static async Task<Response<PackageBenefit>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MSponsorshipPackageBenefits.Add(row);
                await context.SaveChangesAsync();
                return new PackageBenefit(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<PackageBenefit>(msg));
        }
        public async Task<Response<PackageBenefit>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new PackageBenefit(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PackageBenefit>(msg));
        }
        public async Task<Response<PackageBenefit>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                context.MSponsorshipPackageBenefits.Remove(row);
                await context.SaveChangesAsync();
                return new PackageBenefit(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<PackageBenefit>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.MSponsorshipPackageBenefit> FilterQuery(int? packageId)
        {
            return _Querys.Where(x => packageId == null || x.PackageId == packageId)
                .OrderBy(x => x.PackageId).ThenBy(x => x.SeqNo).ThenBy(x => x.Id);
        }
        private static EF.MSponsorshipPackageBenefit GetRow(EF.ModelContext context, int id)
        {
            return context.MSponsorshipPackageBenefits.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Benefit Paket"));
        }
        private static async Task<EF.MSponsorshipPackageBenefit> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MSponsorshipPackageBenefits.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Benefit Paket"));
        }
        private static void ValidateData(string title, int seqNo)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Benefit"));
            }
            if (seqNo <= 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Urutan Benefit"));
            }
        }
        // Tanpa SaveChanges; dipakai juga oleh SponsorshipPackage.Save untuk membuat benefit bersama paketnya.
        internal static EF.MSponsorshipPackageBenefit CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var title = newrow.Title?.Trim() ?? string.Empty;
            SponsorshipPackage.GetPackage(context, newrow.PackageId);
            var seqNo = newrow.SeqNo > 0 ? newrow.SeqNo
                : ValueUtility.GetNextId<EF.MSponsorshipPackageBenefit>(context, x => x.SeqNo, x => x.PackageId == newrow.PackageId);
            ValidateData(title, seqNo);

            var row = new EF.MSponsorshipPackageBenefit
            {
                Id = SequenceUtility.GetNextId<EF.MSponsorshipPackageBenefit>(context),
                PackageId = newrow.PackageId,
                Title = title,
                Description = string.IsNullOrWhiteSpace(newrow.Description) ? null : newrow.Description.Trim(),
                SeqNo = seqNo,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Paket tidak bisa dipindah.
        private void SetRow(EF.ModelContext context, EF.MSponsorshipPackageBenefit row, int userId)
        {
            var title = Title?.Trim() ?? string.Empty;
            var seqNo = SeqNo > 0 ? SeqNo
                : ValueUtility.GetNextId<EF.MSponsorshipPackageBenefit>(context, x => x.SeqNo, x => x.PackageId == row.PackageId);
            ValidateData(title, seqNo);

            row.Title = title;
            row.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            row.SeqNo = seqNo;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        #endregion
    }
}
