using GWALib.General;
using GWALib.General.Utilities;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Organization
{
    public partial class OrganizationProfile
    {
        public record FileData(byte[] Data, string FileName, string ContentType);

        public const int MAX_SK_FILE_BYTES = 5 * 1024 * 1024;

        #region SYNC
        // null = profil organisasi belum diisi.
        public static OrganizationProfile? GetData()
        {
            var snapshot = _Querys.FirstOrDefault();
            return snapshot == null ? null : new OrganizationProfile(snapshot.Row, snapshot.HasLogo);
        }
        public static FileData? GetLogo()
        {
            using var context = DBClass.GetContext();
            var logo = context.MOrganizations.OrderBy(x => x.Id).Select(x => x.Logo).FirstOrDefault();
            return ToLogoFile(logo);
        }
        public static FileData? GetAdvisorSkFile()
        {
            using var context = DBClass.GetContext();
            var file = context.MOrganizations.OrderBy(x => x.Id)
                .Select(x => new { x.AdvisorSkFile, x.AdvisorSkFileName }).FirstOrDefault();
            return ToSkFile(file?.AdvisorSkFile, file?.AdvisorSkFileName);
        }
        public List<OrganizationPhoto.Info> GetPhotos(bool refresh)
        {
            if (refresh || PhotoList == null)
            {
                PhotoList = OrganizationPhoto.GetInfoList();
            }
            return PhotoList;
        }
        public static Response<OrganizationProfile> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.MOrganizations.Add(row);
                context.SaveChanges();
                return new OrganizationProfile(row, false);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<OrganizationProfile>(msg));
        }
        public Response<OrganizationProfile> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(row, userId);
                context.SaveChanges();
                return new OrganizationProfile(row, row.Logo != null);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<OrganizationProfile>(msg));
        }
        // logo null / kosong = logo dihapus.
        public Response<OrganizationProfile> UpdateLogo(byte[]? logo, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetLogo(row, logo, userId);
                context.SaveChanges();
                return new OrganizationProfile(row, row.Logo != null);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<OrganizationProfile>(msg));
        }
        // skFile null / kosong = file SK dihapus.
        public Response<OrganizationProfile> UpdateAdvisorSkFile(byte[]? skFile, string? fileName, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetAdvisorSkFile(row, skFile, fileName, userId);
                context.SaveChanges();
                return new OrganizationProfile(row, row.Logo != null);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<OrganizationProfile>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<OrganizationProfile?> GetDataAsync()
        {
            var snapshot = await _Querys.FirstOrDefaultAsync();
            return snapshot == null ? null : new OrganizationProfile(snapshot.Row, snapshot.HasLogo);
        }
        public static async Task<FileData?> GetLogoAsync()
        {
            using var context = DBClass.GetContext();
            var logo = await context.MOrganizations.OrderBy(x => x.Id).Select(x => x.Logo).FirstOrDefaultAsync();
            return ToLogoFile(logo);
        }
        public static async Task<FileData?> GetAdvisorSkFileAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.MOrganizations.OrderBy(x => x.Id)
                .Select(x => new { x.AdvisorSkFile, x.AdvisorSkFileName }).FirstOrDefaultAsync();
            return ToSkFile(file?.AdvisorSkFile, file?.AdvisorSkFileName);
        }
        public async Task<List<OrganizationPhoto.Info>> GetPhotosAsync(bool refresh)
        {
            if (refresh || PhotoList == null)
            {
                PhotoList = await OrganizationPhoto.GetInfoListAsync();
            }
            return PhotoList;
        }
        public static async Task<Response<OrganizationProfile>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await CreateRowAsync(context, newrow, userId);
                context.MOrganizations.Add(row);
                await context.SaveChangesAsync();
                return new OrganizationProfile(row, false);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<OrganizationProfile>(msg));
        }
        public async Task<Response<OrganizationProfile>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(row, userId);
                await context.SaveChangesAsync();
                return new OrganizationProfile(row, row.Logo != null);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<OrganizationProfile>(msg));
        }
        public async Task<Response<OrganizationProfile>> UpdateLogoAsync(byte[]? logo, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetLogo(row, logo, userId);
                await context.SaveChangesAsync();
                return new OrganizationProfile(row, row.Logo != null);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<OrganizationProfile>(msg));
        }
        public async Task<Response<OrganizationProfile>> UpdateAdvisorSkFileAsync(byte[]? skFile, string? fileName, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetAdvisorSkFile(row, skFile, fileName, userId);
                await context.SaveChangesAsync();
                return new OrganizationProfile(row, row.Logo != null);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<OrganizationProfile>(msg));
        }
        #endregion

        #region HELPER
        // Isi {org} untuk penomoran dokumen; null = profil organisasi belum diisi.
        internal static string? GetCode(EF.ModelContext context)
        {
            return context.MOrganizations.OrderBy(x => x.Id).Select(x => x.Code).FirstOrDefault();
        }
        private static EF.MOrganization GetRow(EF.ModelContext context, int id)
        {
            return context.MOrganizations.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Profil Organisasi"));
        }
        private static async Task<EF.MOrganization> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.MOrganizations.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Profil Organisasi"));
        }
        private static string? Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        private static FileData? ToLogoFile(byte[]? logo)
        {
            if (logo == null || logo.Length == 0)
            {
                return null;
            }
            var contentType = ImageUtility.GetContentType(logo);
            return new FileData(logo, contentType == "image/png" ? "logo.png" : "logo.jpg", contentType);
        }
        private static FileData? ToSkFile(byte[]? file, string? fileName)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }
            var name = string.IsNullOrWhiteSpace(fileName) ? "sk-pembina" : fileName;
            var contentType = Path.GetExtension(name).ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
            return new FileData(file, name, contentType);
        }
        private static void ValidateData(string code, string name)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Kode Organisasi"));
            }
            if (!Regex.IsMatch(code, "^[A-Z0-9]+$"))
            {
                throw new ArgumentException(ExceptionMessage.ORGANIZATION_CODE_INVALID);
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Nama Organisasi"));
            }
        }
        // Profil hanya satu baris: Save ditolak bila sudah ada, selanjutnya pakai Update.
        private static EF.MOrganization CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            if (context.MOrganizations.Any())
            {
                throw new ArgumentException(ExceptionMessage.ORGANIZATION_EXISTS);
            }
            return BuildRow(context, newrow, userId);
        }
        private static async Task<EF.MOrganization> CreateRowAsync(EF.ModelContext context, NewRow newrow, int userId)
        {
            if (await context.MOrganizations.AnyAsync())
            {
                throw new ArgumentException(ExceptionMessage.ORGANIZATION_EXISTS);
            }
            return BuildRow(context, newrow, userId);
        }
        private static EF.MOrganization BuildRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var code = newrow.Code?.Trim().ToUpperInvariant() ?? string.Empty;
            var name = newrow.Name?.Trim() ?? string.Empty;
            ValidateData(code, name);

            var row = new EF.MOrganization
            {
                Id = SequenceUtility.GetNextId<EF.MOrganization>(context),
                Code = code,
                Name = name,
                Description = Clean(newrow.Description),
                Vision = Clean(newrow.Vision),
                Mission = Clean(newrow.Mission),
                Email = Clean(newrow.Email),
                Phone = Clean(newrow.Phone),
                Instagram = Clean(newrow.Instagram),
                FoundedDate = newrow.FoundedDate,
                AdvisorName = Clean(newrow.AdvisorName),
                AdvisorPhone = Clean(newrow.AdvisorPhone),
                AdvisorEmail = Clean(newrow.AdvisorEmail),
                AdvisorSkNumber = Clean(newrow.AdvisorSkNumber),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Logo dan file SK tidak diubah di sini; pakai UpdateLogo / UpdateAdvisorSkFile.
        private void SetRow(EF.MOrganization row, int userId)
        {
            var code = Code?.Trim().ToUpperInvariant() ?? string.Empty;
            var name = Name?.Trim() ?? string.Empty;
            ValidateData(code, name);

            row.Code = code;
            row.Name = name;
            row.Description = Clean(Description);
            row.Vision = Clean(Vision);
            row.Mission = Clean(Mission);
            row.Email = Clean(Email);
            row.Phone = Clean(Phone);
            row.Instagram = Clean(Instagram);
            row.FoundedDate = FoundedDate;
            row.AdvisorName = Clean(AdvisorName);
            row.AdvisorPhone = Clean(AdvisorPhone);
            row.AdvisorEmail = Clean(AdvisorEmail);
            row.AdvisorSkNumber = Clean(AdvisorSkNumber);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetLogo(EF.MOrganization row, byte[]? logo, int userId)
        {
            row.Logo = logo == null || logo.Length == 0 ? null : ImageUtility.Normalize(logo).Data;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetAdvisorSkFile(EF.MOrganization row, byte[]? skFile, string? fileName, int userId)
        {
            if (skFile == null || skFile.Length == 0)
            {
                row.AdvisorSkFile = null;
                row.AdvisorSkFileName = null;
            }
            else
            {
                if (skFile.Length > MAX_SK_FILE_BYTES)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FILE_TOO_LARGE, "File SK", MAX_SK_FILE_BYTES / 1024 / 1024));
                }
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    throw new ArgumentException(ExceptionMessage.ADVISOR_FILE_NAME_REQUIRED);
                }
                row.AdvisorSkFile = skFile;
                row.AdvisorSkFileName = Path.GetFileName(fileName.Trim());
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        #endregion
    }
}
