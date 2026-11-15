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
    public partial class ExternalParty
    {
        #region SYNC
        public List<ExternalPartyContact> GetContacts(bool refresh)
        {
            if (refresh || ContactList == null)
            {
                ContactList = ExternalPartyContact.GetDataList(FilterAktif.Semua, externalPartyId: Id);
            }
            return ContactList;
        }
        public LogoData? GetLogo()
        {
            using var context = DBClass.GetContext();
            var logo = context.MExternalParties.Where(x => x.Id == Id).Select(x => new { x.Logo, x.LogoFileName, x.LogoContentType }).SingleOrDefault();
            return logo?.Logo == null ? null
                : new LogoData(logo.Logo, logo.LogoFileName ?? "logo", logo.LogoContentType ?? ImageUtility.GetContentType(logo.Logo));
        }
        public static List<ExternalParty> GetDataList(ExternalPartyStatus? status = null, ExternalPartyType? partyType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, partyType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<ExternalParty, ListRow>();
        }
        public static Response<ExternalParty> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new ExternalParty(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ExternalParty>(msg));
        }
        public Response<ExternalParty> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new ExternalParty(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ExternalParty>(msg));
        }
        public Response<ExternalParty> UpdateLogo(byte[]? logo, string? fileName, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowLogo(row, logo, fileName, userId);
                context.SaveChanges();
                return new ExternalParty(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ExternalParty>(msg));
        }
        public Response<ExternalParty> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRowDelete(row, userId);
                context.SaveChanges();
                return new ExternalParty(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ExternalParty>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<ExternalPartyContact>> GetContactsAsync(bool refresh)
        {
            if (refresh || ContactList == null)
            {
                ContactList = await ExternalPartyContact.GetDataListAsync(FilterAktif.Semua, externalPartyId: Id);
            }
            return ContactList;
        }
        public async Task<LogoData?> GetLogoAsync()
        {
            using var context = DBClass.GetContext();
            var logo = await context.MExternalParties.Where(x => x.Id == Id).Select(x => new { x.Logo, x.LogoFileName, x.LogoContentType }).SingleOrDefaultAsync();
            return logo?.Logo == null ? null
                : new LogoData(logo.Logo, logo.LogoFileName ?? "logo", logo.LogoContentType ?? ImageUtility.GetContentType(logo.Logo));
        }
        public static async Task<List<ExternalParty>> GetDataListAsync(ExternalPartyStatus? status = null, ExternalPartyType? partyType = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, partyType);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<ExternalParty, ListRow>();
        }
        public static async Task<Response<ExternalParty>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new ExternalParty(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<ExternalParty>(msg));
        }
        public async Task<Response<ExternalParty>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new ExternalParty(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ExternalParty>(msg));
        }
        public async Task<Response<ExternalParty>> UpdateLogoAsync(byte[]? logo, string? fileName, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowLogo(row, logo, fileName, userId);
                await context.SaveChangesAsync();
                return new ExternalParty(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<ExternalParty>(msg));
        }
        public async Task<Response<ExternalParty>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRowDelete(row, userId);
                await context.SaveChangesAsync();
                return new ExternalParty(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<ExternalParty>(msg));
        }
        #endregion

        #region HELPER
        // Local: mitra yang baru di-Add dalam command yang sama (Save dengan Contacts) belum ada di DB.
        // Mitra blacklist dibedakan pesannya dari yang sekadar tidak aktif.
        internal static EF.MExternalParty GetActiveExternalParty(EF.ModelContext context, int externalPartyId)
        {
            var row = context.MExternalParties.Local.SingleOrDefault(x => x.Id == externalPartyId)
                ?? context.MExternalParties.SingleOrDefault(x => x.Id == externalPartyId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Mitra"));
            if (row.Status == (int)ExternalPartyStatus.Blacklist)
            {
                throw new ArgumentException(ExceptionMessage.EXTERNAL_PARTY_BLACKLISTED);
            }
            if (row.Status != (int)ExternalPartyStatus.Aktif)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.OBJECT_NOT_ACTIVE, "Mitra"));
            }
            return row;
        }
        private static IQueryable<ListRow> FilterQuery(ExternalPartyStatus? status, ExternalPartyType? partyType)
        {
            return _Querys.Where(x => (status == null || x.ExternalParty.Status == (int)status)
                    && (partyType == null || x.ExternalParty.PartyType == (int)partyType))
                .OrderBy(x => x.ExternalParty.Name);
        }
        private static EF.MExternalParty GetRow(EF.ModelContext context, int id)
        {
            return context.MExternalParties.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Mitra"));
        }
        private static async Task<EF.MExternalParty> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MExternalParties.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Mitra"));
        }
        private static string? ToNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        private static void ValidateData(EF.ModelContext context, int id, string name, ExternalPartyType partyType)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Mitra"));
            }
            if (!Enum.IsDefined(partyType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Mitra"));
            }
            ValidateUtility.CheckDuplicateData<EF.MExternalParty>(context, x => x.Name == name && x.Id != id,
                string.Format(ExceptionMessage.DATA_DUPLICATE, "Nama Mitra"));
        }
        private static EF.MExternalParty CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var name = newrow.Name?.Trim() ?? string.Empty;
            ValidateData(context, 0, name, newrow.PartyType);

            var row = new EF.MExternalParty
            {
                Id = SequenceUtility.GetNextId<EF.MExternalParty>(context),
                Name = name,
                PartyType = (int)newrow.PartyType,
                Industry = ToNullable(newrow.Industry),
                Address = ToNullable(newrow.Address),
                City = ToNullable(newrow.City),
                Phone = ToNullable(newrow.Phone),
                Email = ToNullable(newrow.Email),
                Website = ToNullable(newrow.Website),
                Instagram = ToNullable(newrow.Instagram),
                Notes = ToNullable(newrow.Notes),
                Status = (int)ExternalPartyStatus.Aktif,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            if (newrow.Logo != null)
            {
                ApplyLogo(row, newrow.Logo, newrow.LogoFileName);
            }
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.MExternalParties.Add(row);

            foreach (var contact in newrow.Contacts ?? new())
            {
                contact.ExternalPartyId = row.Id;
                context.MExternalPartyContacts.Add(ExternalPartyContact.CreateRow(context, contact, userId));
            }
            return row;
        }
        // Logo diubah lewat UpdateLogo.
        private void SetRow(EF.ModelContext context, EF.MExternalParty row, int userId)
        {
            var name = Name?.Trim() ?? string.Empty;
            ValidateData(context, row.Id, name, PartyType);
            if (!Enum.IsDefined(Status))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Status Mitra"));
            }

            row.Name = name;
            row.PartyType = (int)PartyType;
            row.Industry = ToNullable(Industry);
            row.Address = ToNullable(Address);
            row.City = ToNullable(City);
            row.Phone = ToNullable(Phone);
            row.Email = ToNullable(Email);
            row.Website = ToNullable(Website);
            row.Instagram = ToNullable(Instagram);
            row.Notes = ToNullable(Notes);
            row.Status = (int)Status;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void ApplyLogo(EF.MExternalParty row, byte[] logo, string? fileName)
        {
            var image = ImageUtility.Normalize(logo);
            row.Logo = image.Data;
            row.LogoContentType = image.ContentType;
            row.LogoFileName = ToNullable(fileName);
        }
        // null = hapus logo.
        private static void SetRowLogo(EF.MExternalParty row, byte[]? logo, string? fileName, int userId)
        {
            if (logo != null)
            {
                ApplyLogo(row, logo, fileName);
            }
            else
            {
                row.Logo = null;
                row.LogoContentType = null;
                row.LogoFileName = null;
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetRowDelete(EF.MExternalParty row, int userId)
        {
            row.Status = (int)ExternalPartyStatus.TidakAktif;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
