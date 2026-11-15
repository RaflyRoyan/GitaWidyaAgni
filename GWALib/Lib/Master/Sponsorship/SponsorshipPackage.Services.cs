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

namespace GWALib.Lib.Master.Sponsorship
{
    public partial class SponsorshipPackage
    {
        #region SYNC
        public List<PackageBenefit> GetBenefits(bool refresh)
        {
            if (refresh || BenefitList == null)
            {
                BenefitList = PackageBenefit.GetDataList(packageId: Id);
            }
            return BenefitList;
        }
        public static List<SponsorshipPackage> GetDataList(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<SponsorshipPackage, EF.MSponsorshipPackage>();
        }
        public static Response<SponsorshipPackage> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new SponsorshipPackage(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<SponsorshipPackage>(msg));
        }
        public Response<SponsorshipPackage> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new SponsorshipPackage(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipPackage>(msg));
        }
        public Response<SponsorshipPackage> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new SponsorshipPackage(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<SponsorshipPackage>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<PackageBenefit>> GetBenefitsAsync(bool refresh)
        {
            if (refresh || BenefitList == null)
            {
                BenefitList = await PackageBenefit.GetDataListAsync(packageId: Id);
            }
            return BenefitList;
        }
        public static async Task<List<SponsorshipPackage>> GetDataListAsync(FilterAktif aktif, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<SponsorshipPackage, EF.MSponsorshipPackage>();
        }
        public static async Task<Response<SponsorshipPackage>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new SponsorshipPackage(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<SponsorshipPackage>(msg));
        }
        public async Task<Response<SponsorshipPackage>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new SponsorshipPackage(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipPackage>(msg));
        }
        public async Task<Response<SponsorshipPackage>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new SponsorshipPackage(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<SponsorshipPackage>(msg));
        }
        #endregion

        #region HELPER
        // Status apa pun: benefit boleh disiapkan pada paket yang belum / tidak aktif.
        // Local: paket yang baru di-Add dalam command yang sama (Save dengan Benefits) belum ada di DB.
        internal static EF.MSponsorshipPackage GetPackage(EF.ModelContext context, int packageId)
        {
            return context.MSponsorshipPackages.Local.SingleOrDefault(x => x.Id == packageId)
                ?? context.MSponsorshipPackages.SingleOrDefault(x => x.Id == packageId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Paket Sponsorship"));
        }
        // Untuk transaksi sponsorship: paket harus ada dan aktif.
        internal static EF.MSponsorshipPackage GetActivePackage(EF.ModelContext context, int packageId)
        {
            var row = GetPackage(context, packageId);
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Paket Sponsorship"));
            }
            return row;
        }
        private static IQueryable<EF.MSponsorshipPackage> FilterQuery(FilterAktif aktif)
        {
            return _Querys.Where(x => x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                .OrderBy(x => x.SeqNo).ThenBy(x => x.Name);
        }
        private static EF.MSponsorshipPackage GetRow(EF.ModelContext context, int id)
        {
            return context.MSponsorshipPackages.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Paket Sponsorship"));
        }
        private static async Task<EF.MSponsorshipPackage> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MSponsorshipPackages.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Paket Sponsorship"));
        }
        private static void ValidateData(EF.ModelContext context, int id, string name, decimal price, int seqNo)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Paket"));
            }
            if (price < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Nilai Paket"));
            }
            if (seqNo <= 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Urutan Paket"));
            }
            ValidateUtility.CheckDuplicateData<EF.MSponsorshipPackage>(context, x => x.Name == name && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Paket"));
        }
        private static EF.MSponsorshipPackage CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            var seqNo = newrow.SeqNo > 0 ? newrow.SeqNo : ValueUtility.GetNextId<EF.MSponsorshipPackage>(context, x => x.SeqNo);
            ValidateData(context, 0, name, newrow.Price, seqNo);

            var row = new EF.MSponsorshipPackage
            {
                Id = SequenceUtility.GetNextId<EF.MSponsorshipPackage>(context),
                Name = name,
                Price = newrow.Price,
                SeqNo = seqNo,
                IsActive = (int)ActiveStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.MSponsorshipPackages.Add(row);

            foreach (var benefit in newrow.Benefits ?? new())
            {
                benefit.PackageId = row.Id;
                context.MSponsorshipPackageBenefits.Add(PackageBenefit.CreateRow(context, benefit, userId));
            }
            return row;
        }
        // Benefit diubah lewat PackageBenefit. Mengubah nilai paket tidak mengubah sponsorship yang sudah dibuat.
        private void SetRow(EF.ModelContext context, EF.MSponsorshipPackage row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            ValidateData(context, row.Id, name, Price, SeqNo);
            if (!Enum.IsDefined(IsActive))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status"));
            }

            row.Name = name;
            row.Price = Price;
            row.SeqNo = SeqNo;
            row.IsActive = (int)IsActive;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MSponsorshipPackage row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
