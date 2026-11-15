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

namespace GWALib.Lib.Master.Mitra
{
    public partial class ExternalPartyContact
    {
        #region SYNC
        public ExternalParty GetExternalParty(bool refresh = false)
        {
            if (refresh || ExternalPartyData == null)
            {
                ExternalPartyData = new ExternalParty(ExternalPartyId);
            }
            return ExternalPartyData;
        }
        public static ExternalPartyContact? GetPrimaryByExternalParty(int externalPartyId)
        {
            var row = PrimaryQuery(externalPartyId).FirstOrDefault();
            return row != null ? new ExternalPartyContact(row) : null;
        }
        public static List<ExternalPartyContact> GetDataList(FilterAktif aktif, int? externalPartyId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, externalPartyId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ExternalPartyContact, EF.MExternalPartyContact>();
        }
        public static Response<ExternalPartyContact> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MExternalPartyContacts.Add(row);
                context.SaveChanges();
                return new ExternalPartyContact(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ExternalPartyContact>(msg));
        }
        public Response<ExternalPartyContact> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ExternalPartyContact(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ExternalPartyContact>(msg));
        }
        public Response<ExternalPartyContact> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new ExternalPartyContact(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ExternalPartyContact>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<ExternalPartyContact?> GetPrimaryByExternalPartyAsync(int externalPartyId)
        {
            var row = await PrimaryQuery(externalPartyId).FirstOrDefaultAsync();
            return row != null ? new ExternalPartyContact(row) : null;
        }
        public static async Task<List<ExternalPartyContact>> GetDataListAsync(FilterAktif aktif, int? externalPartyId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(aktif, externalPartyId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ExternalPartyContact, EF.MExternalPartyContact>();
        }
        public static async Task<Response<ExternalPartyContact>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MExternalPartyContacts.Add(row);
                await context.SaveChangesAsync();
                return new ExternalPartyContact(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ExternalPartyContact>(msg));
        }
        public async Task<Response<ExternalPartyContact>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ExternalPartyContact(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ExternalPartyContact>(msg));
        }
        public async Task<Response<ExternalPartyContact>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new ExternalPartyContact(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ExternalPartyContact>(msg));
        }
        #endregion

        #region HELPER
        internal static void ValidateActiveContact(EF.ModelContext context, int contactId, int externalPartyId)
        {
            var row = context.MExternalPartyContacts.SingleOrDefault(x => x.Id == contactId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kontak Mitra"));
            if (row.ExternalPartyId != externalPartyId)
            {
                throw new ArgumentException(ExceptionMessage.EXTERNAL_PARTY_CONTACT_MISMATCH);
            }
            if (row.IsActive != (int)ActiveStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Kontak Mitra"));
            }
        }
        private static IQueryable<EF.MExternalPartyContact> PrimaryQuery(int externalPartyId)
        {
            return _Querys.Where(x => x.ExternalPartyId == externalPartyId && x.IsActive == (int)ActiveStatus.Aktif && x.IsPrimary == (int)ContactRank.Utama);
        }
        private static IQueryable<EF.MExternalPartyContact> FilterQuery(FilterAktif aktif, int? externalPartyId)
        {
            return _Querys.Where(x => (x.IsActive == (int)aktif || aktif == FilterAktif.Semua)
                    && (externalPartyId == null || x.ExternalPartyId == externalPartyId))
                .OrderByDescending(x => x.IsPrimary).ThenBy(x => x.Name);
        }
        private static EF.MExternalPartyContact GetRow(EF.ModelContext context, int id)
        {
            return context.MExternalPartyContacts.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kontak Mitra"));
        }
        private static async Task<EF.MExternalPartyContact> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MExternalPartyContacts.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kontak Mitra"));
        }
        private static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        private static void ValidateData(string name, ContactRank isPrimary)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Kontak"));
            }
            if (!Enum.IsDefined(isPrimary))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kontak Utama"));
            }
        }
        // Turunkan kontak utama aktif lain milik mitra yang sama (termasuk yang baru di-Add di context ini).
        private static void DemoteOtherPrimary(EF.ModelContext context, int externalPartyId, int exceptId, int userId)
        {
            var others = context.MExternalPartyContacts.Where(x => x.ExternalPartyId == externalPartyId && x.Id != exceptId
                    && x.IsActive == (int)ActiveStatus.Aktif && x.IsPrimary == (int)ContactRank.Utama).ToList()
                .Union(context.MExternalPartyContacts.Local.Where(x => x.ExternalPartyId == externalPartyId && x.Id != exceptId
                    && x.IsActive == (int)ActiveStatus.Aktif && x.IsPrimary == (int)ContactRank.Utama));
            foreach (var other in others)
            {
                other.IsPrimary = (int)ContactRank.Tambahan;
                other.UpdDate = DateTime.Now;
                other.UpdBy = userId;
            }
        }
        // Tanpa SaveChanges; dipakai juga oleh ExternalParty.Save untuk membuat kontak bersama mitranya.
        internal static EF.MExternalPartyContact CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            ExternalParty.GetActiveExternalParty(context, newrow.ExternalPartyId);
            ValidateData(name, newrow.IsPrimary);

            var row = new EF.MExternalPartyContact
            {
                Id = SequenceUtility.GetNextId<EF.MExternalPartyContact>(context),
                ExternalPartyId = newrow.ExternalPartyId,
                Name = name,
                Position = ToNullable(newrow.Position),
                Phone = ToNullable(newrow.Phone),
                Email = ToNullable(newrow.Email),
                IsPrimary = (int)newrow.IsPrimary,
                IsActive = (int)ActiveStatus.Aktif,
                Notes = ToNullable(newrow.Notes),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            if (newrow.IsPrimary == ContactRank.Utama)
            {
                DemoteOtherPrimary(context, row.ExternalPartyId, row.Id, userId);
            }
            return row;
        }
        // Mitra tidak bisa diganti. Kontak tidak aktif tidak bisa menjadi kontak utama.
        private void SetRow(EF.ModelContext context, EF.MExternalPartyContact row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            ValidateData(name, IsPrimary);
            var isPrimary = IsActive == ActiveStatus.Aktif ? IsPrimary : ContactRank.Tambahan;
            if (isPrimary == ContactRank.Utama)
            {
                DemoteOtherPrimary(context, row.ExternalPartyId, row.Id, userId);
            }

            row.Name = name;
            row.Position = ToNullable(Position);
            row.Phone = ToNullable(Phone);
            row.Email = ToNullable(Email);
            row.IsPrimary = (int)isPrimary;
            row.IsActive = (int)IsActive;
            row.Notes = ToNullable(Notes);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MExternalPartyContact row, int userId)
        {
            row.IsActive = (int)ActiveStatus.TidakAktif;
            row.IsPrimary = (int)ContactRank.Tambahan;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
