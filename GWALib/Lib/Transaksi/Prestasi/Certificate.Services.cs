using GWALib.General;
using GWALib.Lib.Transaksi.Information;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Transaksi.Program;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Prestasi
{
    public partial class Certificate
    {
        public const string NUMBER_DOCUMENT_TYPE = "CERTIFICATE";
        private const string NUMBER_FALLBACK_PREFIX = "SRT-C";
        public const int MAX_FILE_BYTES = 5 * 1024 * 1024;
        private const string CODE_CHARS = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int CODE_LENGTH = 12;

        #region SYNC
        public FileContent? GetFile()
        {
            using var context = DBClass.GetContext();
            var file = context.TCertificates.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName }).SingleOrDefault();
            return file?.FileData == null ? null : new FileContent(file.FileData, file.FileName ?? "sertifikat");
        }
        public static List<Certificate> GetDataList(int? memberId = null, CertificateType? certificateType = null, int? activityId = null,
            CertificateStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(memberId, certificateType, activityId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<Certificate, EF.TCertificate>();
        }
        // Cek keaslian publik (tanpa login); null bila kode tidak dikenal. Status menunjukkan masih berlaku atau dicabut.
        public static Certificate? Verify(string code)
        {
            var value = code?.Trim().ToUpperInvariant() ?? string.Empty;
            var row = _Querys.SingleOrDefault(x => x.VerificationCode == value);
            return row != null ? new Certificate(row) : null;
        }
        public static Response<Certificate> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                transaction.Commit();
                return new Certificate(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Certificate>(msg));
        }
        // Hanya judul dan tanggal terbit.
        public Response<Certificate> Update(string user)
        {
            return Run((context, row, userId) => SetRow(row, userId), user);
        }
        // file null / kosong = file dihapus.
        public Response<Certificate> UpdateFile(byte[]? file, string? fileName, string user)
        {
            return Run((context, row, userId) => SetFile(row, file, fileName, userId), user);
        }
        public Response<Certificate> Revoke(string user)
        {
            return Run((context, row, userId) => SetStatus(row, CertificateStatus.Dicabut, userId), user);
        }
        public Response<Certificate> Restore(string user)
        {
            return Run((context, row, userId) => SetStatus(row, CertificateStatus.Terbit, userId), user);
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetFileAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TCertificates.Where(x => x.Id == Id).Select(x => new { x.FileData, x.FileName }).SingleOrDefaultAsync();
            return file?.FileData == null ? null : new FileContent(file.FileData, file.FileName ?? "sertifikat");
        }
        public static async Task<List<Certificate>> GetDataListAsync(int? memberId = null, CertificateType? certificateType = null, int? activityId = null,
            CertificateStatus? status = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(memberId, certificateType, activityId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<Certificate, EF.TCertificate>();
        }
        public static async Task<Certificate?> VerifyAsync(string code)
        {
            var value = code?.Trim().ToUpperInvariant() ?? string.Empty;
            var row = await _Querys.SingleOrDefaultAsync(x => x.VerificationCode == value);
            return row != null ? new Certificate(row) : null;
        }
        public static async Task<Response<Certificate>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new Certificate(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<Certificate>(msg));
        }
        public Task<Response<Certificate>> UpdateAsync(string user)
        {
            return RunAsync((context, row, userId) => SetRow(row, userId), user);
        }
        public Task<Response<Certificate>> UpdateFileAsync(byte[]? file, string? fileName, string user)
        {
            return RunAsync((context, row, userId) => SetFile(row, file, fileName, userId), user);
        }
        public Task<Response<Certificate>> RevokeAsync(string user)
        {
            return RunAsync((context, row, userId) => SetStatus(row, CertificateStatus.Dicabut, userId), user);
        }
        public Task<Response<Certificate>> RestoreAsync(string user)
        {
            return RunAsync((context, row, userId) => SetStatus(row, CertificateStatus.Terbit, userId), user);
        }
        #endregion

        #region HELPER
        private delegate void RowAction(EF.ModelContext context, EF.TCertificate row, int userId);

        private Response<Certificate> Run(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                return new Certificate(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Certificate>(msg));
        }
        private async Task<Response<Certificate>> RunAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                return new Certificate(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<Certificate>(msg));
        }
        private static EF.TCertificate Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TCertificate> FilterQuery(int? memberId, CertificateType? certificateType, int? activityId, CertificateStatus? status)
        {
            return _Querys.Where(x => (memberId == null || x.MemberId == memberId)
                    && (certificateType == null || x.CertificateType == (int)certificateType)
                    && (activityId == null || x.ActivityId == activityId)
                    && (status == null || x.Status == (int)status))
                .OrderByDescending(x => x.IssuedDate).ThenByDescending(x => x.Id);
        }
        private static EF.TCertificate GetRow(EF.ModelContext context, int id)
        {
            return context.TCertificates.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sertifikat"));
        }
        private static async Task<EF.TCertificate> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TCertificates.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sertifikat"));
        }
        // Kode verifikasi acak tanpa huruf / angka yang mirip (O/0, I/1), dicek unik.
        private static string NewVerificationCode(EF.ModelContext context)
        {
            while (true)
            {
                var chars = new char[CODE_LENGTH];
                for (var i = 0; i < CODE_LENGTH; i++)
                {
                    chars[i] = CODE_CHARS[RandomNumberGenerator.GetInt32(CODE_CHARS.Length)];
                }
                var code = new string(chars);
                if (!context.TCertificates.Any(x => x.VerificationCode == code))
                {
                    return code;
                }
            }
        }
        private record Refs(int? ActivityId, int? AssignmentId, int? AchievementId);

        private static Refs ValidateRefs(EF.ModelContext context, NewRow newrow)
        {
            var type = newrow.CertificateType;
            switch (type)
            {
                case CertificateType.Peserta:
                case CertificateType.Panitia:
                case CertificateType.Pemateri:
                    if (!(newrow.ActivityId > 0))
                    {
                        throw new ArgumentException(string.Format(ExceptionMessage.CERTIFICATE_REF_REQUIRED, type.GetDescription(), "kegiatan"));
                    }
                    Activity.GetActivity(context, newrow.ActivityId!.Value);
                    return new Refs(newrow.ActivityId, null, null);
                case CertificateType.Pengurus:
                    var assignmentMember = context.TManagementAssignments.Where(x => x.Id == newrow.AssignmentId).Select(x => (int?)x.MemberId).SingleOrDefault()
                        ?? throw new ArgumentException(string.Format(ExceptionMessage.CERTIFICATE_REF_REQUIRED, type.GetDescription(), "penugasan pengurus"));
                    if (assignmentMember != newrow.MemberId)
                    {
                        throw new ArgumentException(string.Format(ExceptionMessage.CERTIFICATE_MEMBER_MISMATCH, "Penugasan"));
                    }
                    return new Refs(null, newrow.AssignmentId, null);
                case CertificateType.Prestasi:
                    var achievement = context.TAchievements.Where(x => x.Id == newrow.AchievementId).Select(x => new { x.MemberId }).SingleOrDefault()
                        ?? throw new ArgumentException(string.Format(ExceptionMessage.CERTIFICATE_REF_REQUIRED, type.GetDescription(), "prestasi"));
                    if (achievement.MemberId != newrow.MemberId)
                    {
                        throw new ArgumentException(string.Format(ExceptionMessage.CERTIFICATE_MEMBER_MISMATCH, "Prestasi"));
                    }
                    return new Refs(null, null, newrow.AchievementId);
                case CertificateType.Lainnya:
                    var activityId = newrow.ActivityId > 0 ? newrow.ActivityId : null;
                    if (activityId != null)
                    {
                        Activity.GetActivity(context, activityId.Value);
                    }
                    return new Refs(activityId, null, null);
                default:
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Sertifikat"));
            }
        }
        private static EF.TCertificate CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            Member.GetMember(context, newrow.MemberId);
            if (string.IsNullOrWhiteSpace(newrow.Title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Sertifikat"));
            }
            if (newrow.IssuedDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Terbit"));
            }
            var refs = ValidateRefs(context, newrow);
            var id = SequenceUtility.GetNextId<EF.TCertificate>(context);
            var number = TransaksiHelper.Truncate(newrow.CertificateNumber, 100)
                ?? TransaksiHelper.GenerateNumber(context, NUMBER_DOCUMENT_TYPE, newrow.IssuedDate.ToDateTime(TimeOnly.MinValue), NUMBER_FALLBACK_PREFIX, id, userId);
            ValidateUtility.CheckDuplicateData<EF.TCertificate>(context, x => x.CertificateNumber == number, string.Format(ExceptionMessage.DATA_DUPLICATE, "Nomor Sertifikat"));

            var row = new EF.TCertificate
            {
                Id = id,
                MemberId = newrow.MemberId,
                CertificateType = (int)newrow.CertificateType,
                ActivityId = refs.ActivityId,
                AssignmentId = refs.AssignmentId,
                AchievementId = refs.AchievementId,
                CertificateNumber = number,
                Title = TransaksiHelper.Truncate(newrow.Title, 200)!,
                IssuedDate = newrow.IssuedDate,
                VerificationCode = NewVerificationCode(context),
                Status = (int)CertificateStatus.Terbit,
                InsDate = DateTime.Now,
                InsBy = userId
            };
            if (newrow.FileData != null && newrow.FileData.Length > 0)
            {
                row.FileName = TransaksiHelper.ValidateFile(newrow.FileData, newrow.FileName, "File Sertifikat", MAX_FILE_BYTES);
                row.FileData = newrow.FileData;
            }
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TCertificates.Add(row);
            NotifyHelper.ToMember(context, row.MemberId, NotifyHelper.TYPE_CERTIFICATE, "Sertifikat terbit: " + row.Title,
                "Nomor " + row.CertificateNumber, NotifyHelper.LINK_MY_ACHIEVEMENT, "t_certificate", row.Id, userId);
            return row;
        }
        private void SetRow(EF.TCertificate row, int userId)
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Sertifikat"));
            }
            if (IssuedDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Terbit"));
            }
            row.Title = TransaksiHelper.Truncate(Title, 200)!;
            row.IssuedDate = IssuedDate;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetFile(EF.TCertificate row, byte[]? file, string? fileName, int userId)
        {
            if (file == null || file.Length == 0)
            {
                row.FileData = null;
                row.FileName = null;
            }
            else
            {
                row.FileName = TransaksiHelper.ValidateFile(file, fileName, "File Sertifikat", MAX_FILE_BYTES);
                row.FileData = file;
            }
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetStatus(EF.TCertificate row, CertificateStatus target, int userId)
        {
            var from = target == CertificateStatus.Dicabut ? CertificateStatus.Terbit : CertificateStatus.Dicabut;
            TransaksiHelper.ValidateStatus((CertificateStatus)row.Status, target == CertificateStatus.Dicabut ? "mencabut sertifikat" : "menerbitkan ulang sertifikat", from);
            row.Status = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
