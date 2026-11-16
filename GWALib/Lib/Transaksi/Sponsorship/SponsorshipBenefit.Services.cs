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

namespace GWALib.Lib.Transaksi.Sponsorship
{
    public partial class SponsorshipBenefit
    {
        public const int MAX_PROOF_BYTES = 5 * 1024 * 1024;
        private const int DESCRIPTION_LENGTH = 500;
        private const int NOTES_LENGTH = 255;

        #region SYNC
        public FileContent? GetProof()
        {
            using var context = DBClass.GetContext();
            var file = context.TSponsorshipBenefits.Where(x => x.Id == Id).Select(x => new { x.ProofFile, x.ProofFileName, x.ProofContentType }).SingleOrDefault();
            return file?.ProofFile == null ? null : new FileContent(file.ProofFile, file.ProofFileName ?? "bukti", file.ProofContentType);
        }
        public static List<SponsorshipBenefit> GetDataList(int sponsorshipId)
        {
            return FilterQuery(sponsorshipId).ToList().MappingData<SponsorshipBenefit, EF.TSponsorshipBenefit>();
        }
        public static Response<SponsorshipBenefit> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                return new SponsorshipBenefit(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<SponsorshipBenefit>(msg));
        }
        public Response<SponsorshipBenefit> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new SponsorshipBenefit(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipBenefit>(msg));
        }
        // Bukti opsional (file null = tanpa bukti / bukti lama tetap).
        public Response<SponsorshipBenefit> MarkDone(byte[]? proof, string? proofFileName, string? contentType, string? notes, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetDone(context, row, proof, proofFileName, contentType, notes, userId);
                context.SaveChanges();
                return new SponsorshipBenefit(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipBenefit>(msg));
        }
        public Response<SponsorshipBenefit> MarkUndone(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetUndone(context, row, userId);
                context.SaveChanges();
                return new SponsorshipBenefit(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipBenefit>(msg));
        }
        // file null / kosong = bukti dihapus.
        public Response<SponsorshipBenefit> UpdateProof(byte[]? file, string? fileName, string? contentType, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SponsorshipDeal.GetActiveSponsorship(context, row.SponsorshipId);
                SetProof(row, file, fileName, contentType);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                context.SaveChanges();
                return new SponsorshipBenefit(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipBenefit>(msg));
        }
        public Response<SponsorshipBenefit> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SponsorshipDeal.GetActiveSponsorship(context, row.SponsorshipId);
                var data = new SponsorshipBenefit(Reload(row.Id));
                context.TSponsorshipBenefits.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<SponsorshipBenefit>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<FileContent?> GetProofAsync()
        {
            using var context = DBClass.GetContext();
            var file = await context.TSponsorshipBenefits.Where(x => x.Id == Id).Select(x => new { x.ProofFile, x.ProofFileName, x.ProofContentType }).SingleOrDefaultAsync();
            return file?.ProofFile == null ? null : new FileContent(file.ProofFile, file.ProofFileName ?? "bukti", file.ProofContentType);
        }
        public static async Task<List<SponsorshipBenefit>> GetDataListAsync(int sponsorshipId)
        {
            return await (await FilterQuery(sponsorshipId).ToListAsync()).MappingDataAsync<SponsorshipBenefit, EF.TSponsorshipBenefit>();
        }
        public static async Task<Response<SponsorshipBenefit>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                return new SponsorshipBenefit(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<SponsorshipBenefit>(msg));
        }
        public async Task<Response<SponsorshipBenefit>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new SponsorshipBenefit(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipBenefit>(msg));
        }
        public async Task<Response<SponsorshipBenefit>> MarkDoneAsync(byte[]? proof, string? proofFileName, string? contentType, string? notes, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetDone(context, row, proof, proofFileName, contentType, notes, userId);
                await context.SaveChangesAsync();
                return new SponsorshipBenefit(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipBenefit>(msg));
        }
        public async Task<Response<SponsorshipBenefit>> MarkUndoneAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetUndone(context, row, userId);
                await context.SaveChangesAsync();
                return new SponsorshipBenefit(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipBenefit>(msg));
        }
        public async Task<Response<SponsorshipBenefit>> UpdateProofAsync(byte[]? file, string? fileName, string? contentType, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SponsorshipDeal.GetActiveSponsorship(context, row.SponsorshipId);
                SetProof(row, file, fileName, contentType);
                row.UpdDate = DateTime.Now;
                row.UpdBy = userId;
                await context.SaveChangesAsync();
                return new SponsorshipBenefit(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipBenefit>(msg));
        }
        public async Task<Response<SponsorshipBenefit>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SponsorshipDeal.GetActiveSponsorship(context, row.SponsorshipId);
                var data = new SponsorshipBenefit(Reload(row.Id));
                context.TSponsorshipBenefits.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<SponsorshipBenefit>(msg));
        }
        #endregion

        #region INTERNAL
        // Menyalin benefit paket ke sponsorship baru (urutan dan teks ikut paket).
        internal static void CopyFromPackage(EF.ModelContext context, int sponsorshipId, int packageId, int userId)
        {
            var benefits = context.MSponsorshipPackageBenefits.Where(x => x.PackageId == packageId).OrderBy(x => x.SeqNo).ToList();
            if (benefits.Count == 0)
            {
                return;
            }
            var nextId = SequenceUtility.ReserveIds<EF.TSponsorshipBenefit>(context, benefits.Count);
            var now = DateTime.Now;
            foreach (var benefit in benefits)
            {
                context.TSponsorshipBenefits.Add(new EF.TSponsorshipBenefit
                {
                    Id = nextId++,
                    SponsorshipId = sponsorshipId,
                    Title = benefit.Title,
                    Description = benefit.Description,
                    SeqNo = benefit.SeqNo,
                    IsDone = (int)SponsorshipBenefitStatus.Belum,
                    InsDate = now,
                    InsBy = userId
                });
            }
        }
        #endregion

        #region HELPER
        private static EF.TSponsorshipBenefit Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TSponsorshipBenefit> FilterQuery(int sponsorshipId)
        {
            return _Querys.Where(x => x.SponsorshipId == sponsorshipId).OrderBy(x => x.SeqNo).ThenBy(x => x.Id);
        }
        private static EF.TSponsorshipBenefit GetRow(EF.ModelContext context, int id)
        {
            return context.TSponsorshipBenefits.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Benefit Sponsorship"));
        }
        private static async Task<EF.TSponsorshipBenefit> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TSponsorshipBenefits.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Benefit Sponsorship"));
        }
        private static string ValidateTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Judul Benefit"));
            }
            return title.Trim();
        }
        private static void SetProof(EF.TSponsorshipBenefit row, byte[]? file, string? fileName, string? contentType)
        {
            if (file == null || file.Length == 0)
            {
                row.ProofFile = null;
                row.ProofFileName = null;
                row.ProofContentType = null;
                return;
            }
            row.ProofFileName = TransaksiHelper.ValidateFile(file, fileName, "File Bukti", MAX_PROOF_BYTES);
            row.ProofFile = file;
            row.ProofContentType = TransaksiHelper.ToNullable(contentType);
        }
        private static EF.TSponsorshipBenefit CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            SponsorshipDeal.GetActiveSponsorship(context, newrow.SponsorshipId);
            var seqNo = newrow.SeqNo > 0 ? newrow.SeqNo
                : ValueUtility.GetNextId<EF.TSponsorshipBenefit>(context, x => x.SeqNo, x => x.SponsorshipId == newrow.SponsorshipId);
            var row = new EF.TSponsorshipBenefit
            {
                Id = SequenceUtility.GetNextId<EF.TSponsorshipBenefit>(context),
                SponsorshipId = newrow.SponsorshipId,
                Title = ValidateTitle(newrow.Title),
                Description = TransaksiHelper.Truncate(newrow.Description, DESCRIPTION_LENGTH),
                SeqNo = seqNo,
                IsDone = (int)SponsorshipBenefitStatus.Belum,
                Notes = TransaksiHelper.Truncate(newrow.Notes, NOTES_LENGTH),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TSponsorshipBenefits.Add(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.TSponsorshipBenefit row, int userId)
        {
            SponsorshipDeal.GetActiveSponsorship(context, row.SponsorshipId);
            row.Title = ValidateTitle(Title);
            row.Description = TransaksiHelper.Truncate(Description, DESCRIPTION_LENGTH);
            row.SeqNo = SeqNo > 0 ? SeqNo : row.SeqNo;
            row.Notes = TransaksiHelper.Truncate(Notes, NOTES_LENGTH);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetDone(EF.ModelContext context, EF.TSponsorshipBenefit row, byte[]? proof, string? proofFileName, string? contentType, string? notes, int userId)
        {
            SponsorshipDeal.GetActiveSponsorship(context, row.SponsorshipId);
            TransaksiHelper.ValidateStatus((SponsorshipBenefitStatus)row.IsDone, "menandai benefit selesai", SponsorshipBenefitStatus.Belum);
            if (proof != null && proof.Length > 0)
            {
                SetProof(row, proof, proofFileName, contentType);
            }
            var now = DateTime.Now;
            row.IsDone = (int)SponsorshipBenefitStatus.Sudah;
            row.DoneAt = now;
            row.DoneBy = userId;
            if (!string.IsNullOrWhiteSpace(notes))
            {
                row.Notes = TransaksiHelper.Truncate(notes, NOTES_LENGTH);
            }
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        // Bukti tetap disimpan; hapus lewat UpdateProof bila perlu.
        private static void SetUndone(EF.ModelContext context, EF.TSponsorshipBenefit row, int userId)
        {
            SponsorshipDeal.GetActiveSponsorship(context, row.SponsorshipId);
            TransaksiHelper.ValidateStatus((SponsorshipBenefitStatus)row.IsDone, "membatalkan tanda benefit", SponsorshipBenefitStatus.Sudah);
            row.IsDone = (int)SponsorshipBenefitStatus.Belum;
            row.DoneAt = null;
            row.DoneBy = null;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
