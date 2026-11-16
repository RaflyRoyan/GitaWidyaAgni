using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Master.Mitra;
using GWALib.Lib.Master.Sponsorship;
using GWALib.Lib.Transaksi.Keuangan;
using GWALib.Lib.Transaksi.Program;
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
    public partial class SponsorshipDeal
    {
        public const string NUMBER_DOCUMENT_TYPE = "SPONSORSHIP";
        private const string NUMBER_FALLBACK_PREFIX = "SPN";
        private const int NOTES_LENGTH = 500;

        #region SYNC
        public List<SponsorshipBenefit> GetBenefits(bool refresh)
        {
            if (refresh || BenefitList == null)
            {
                BenefitList = SponsorshipBenefit.GetDataList(Id);
            }
            return BenefitList;
        }
        public List<SponsorshipMou> GetMous(bool refresh)
        {
            if (refresh || MouList == null)
            {
                MouList = SponsorshipMou.GetDataList(Id);
            }
            return MouList;
        }
        public static List<SponsorshipDeal> GetDataList(int? externalPartyId = null, int? periodId = null, int? activityId = null, SponsorshipStatus? status = null,
            int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(externalPartyId, periodId, activityId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return SetReceived(query.ToList().MappingData<SponsorshipDeal, EF.TSponsorship>());
        }
        // Benefit paket ikut disalin; nomor ikut di-rollback bila simpan gagal.
        public static Response<SponsorshipDeal> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                transaction.Commit();
                return new SponsorshipDeal(row.Id.ToString().EncryptString<SponsorshipDeal>());
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<SponsorshipDeal>(msg));
        }
        public Response<SponsorshipDeal> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new SponsorshipDeal(EncDecrKey);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipDeal>(msg));
        }
        // Dana sponsor masuk: transaksi Income Posted dengan sponsorship_id; total tidak boleh melebihi nilai sponsorship.
        public Response<SponsorshipDeal> ReceivePayment(int accountId, int categoryId, decimal amount, DateOnly receivedDate, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = GetRow(context, Id);
                SetPayment(context, row, accountId, categoryId, amount, receivedDate, userId);
                context.SaveChanges();
                transaction.Commit();
                return new SponsorshipDeal(EncDecrKey);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipDeal>(msg));
        }
        public Response<SponsorshipDeal> Finish(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetFinish(context, row, userId);
                context.SaveChanges();
                return new SponsorshipDeal(EncDecrKey);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipDeal>(msg));
        }
        public Response<SponsorshipDeal> Cancel(string? reason, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetCancel(context, row, reason, userId);
                context.SaveChanges();
                return new SponsorshipDeal(EncDecrKey);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipDeal>(msg));
        }
        // Hanya sponsorship Aktif tanpa transaksi, MoU dan aset; benefit ikut dihapus.
        public Response<SponsorshipDeal> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                var data = new SponsorshipDeal(EncDecrKey);
                SetDelete(context, row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<SponsorshipDeal>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<SponsorshipBenefit>> GetBenefitsAsync(bool refresh)
        {
            if (refresh || BenefitList == null)
            {
                BenefitList = await SponsorshipBenefit.GetDataListAsync(Id);
            }
            return BenefitList;
        }
        public async Task<List<SponsorshipMou>> GetMousAsync(bool refresh)
        {
            if (refresh || MouList == null)
            {
                MouList = await SponsorshipMou.GetDataListAsync(Id);
            }
            return MouList;
        }
        public static async Task<List<SponsorshipDeal>> GetDataListAsync(int? externalPartyId = null, int? periodId = null, int? activityId = null, SponsorshipStatus? status = null,
            int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(externalPartyId, periodId, activityId, status);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return SetReceived(await (await query.ToListAsync()).MappingDataAsync<SponsorshipDeal, EF.TSponsorship>());
        }
        public static async Task<Response<SponsorshipDeal>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new SponsorshipDeal(row.Id.ToString().EncryptString<SponsorshipDeal>());
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<SponsorshipDeal>(msg));
        }
        public async Task<Response<SponsorshipDeal>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new SponsorshipDeal(EncDecrKey);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipDeal>(msg));
        }
        public async Task<Response<SponsorshipDeal>> ReceivePaymentAsync(int accountId, int categoryId, decimal amount, DateOnly receivedDate, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = await GetRowAsync(context, Id);
                SetPayment(context, row, accountId, categoryId, amount, receivedDate, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new SponsorshipDeal(EncDecrKey);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipDeal>(msg));
        }
        public async Task<Response<SponsorshipDeal>> FinishAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetFinish(context, row, userId);
                await context.SaveChangesAsync();
                return new SponsorshipDeal(EncDecrKey);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipDeal>(msg));
        }
        public async Task<Response<SponsorshipDeal>> CancelAsync(string? reason, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetCancel(context, row, reason, userId);
                await context.SaveChangesAsync();
                return new SponsorshipDeal(EncDecrKey);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SponsorshipDeal>(msg));
        }
        public async Task<Response<SponsorshipDeal>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                var data = new SponsorshipDeal(EncDecrKey);
                SetDelete(context, row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<SponsorshipDeal>(msg));
        }
        #endregion

        #region INTERNAL
        // Sponsorship yang masih berjalan (Aktif); dipakai SponsorshipBenefit.
        internal static EF.TSponsorship GetActiveSponsorship(EF.ModelContext context, int sponsorshipId)
        {
            var row = context.TSponsorships.Local.SingleOrDefault(x => x.Id == sponsorshipId)
                ?? context.TSponsorships.SingleOrDefault(x => x.Id == sponsorshipId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sponsorship"));
            TransaksiHelper.ValidateStatus((SponsorshipStatus)row.Status, "mengubah sponsorship", SponsorshipStatus.Aktif);
            return row;
        }
        // Dana sponsor yang sudah diterima = jumlah transaksi Income Posted per sponsorship.
        internal static Dictionary<int, decimal> GetReceived(List<int> sponsorshipIds)
        {
            using var context = DBClass.GetContext();
            return context.TFinancialTransactions
                .Where(x => x.SponsorshipId != null && sponsorshipIds.Contains(x.SponsorshipId.Value)
                    && x.Status == (int)TransactionStatus.Posted && x.TransactionType == (int)CashFlowType.Income)
                .GroupBy(x => x.SponsorshipId!.Value)
                .Select(g => new { g.Key, Amount = g.Sum(x => x.Amount) })
                .ToDictionary(x => x.Key, x => x.Amount);
        }
        #endregion

        #region HELPER
        private record Refs(int? ContactId, int? PeriodId, int? ActivityId, int? PicMemberId, string? InKindDescription);

        private static List<SponsorshipDeal> SetReceived(List<SponsorshipDeal> list)
        {
            if (list.Count > 0)
            {
                var map = GetReceived(list.Select(x => x.Id).ToList());
                foreach (var item in list)
                {
                    item.ReceivedAmount = map.GetValueOrDefault(item.Id);
                }
            }
            return list;
        }
        private static IQueryable<EF.TSponsorship> FilterQuery(int? externalPartyId, int? periodId, int? activityId, SponsorshipStatus? status)
        {
            return _Querys.Where(x => (externalPartyId == null || x.ExternalPartyId == externalPartyId)
                    && (periodId == null || x.PeriodId == periodId)
                    && (activityId == null || x.ActivityId == activityId)
                    && (status == null || x.Status == (int)status))
                .OrderByDescending(x => x.Id);
        }
        private static EF.TSponsorship GetRow(EF.ModelContext context, int id)
        {
            return context.TSponsorships.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sponsorship"));
        }
        private static async Task<EF.TSponsorship> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TSponsorships.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Sponsorship"));
        }
        private static Refs ValidateData(EF.ModelContext context, int externalPartyId, int? contactId, int? periodId, int? activityId, int? picMemberId,
            SponsorshipForm form, decimal amount, string? inKindDescription, DateOnly? startDate, DateOnly? endDate)
        {
            ExternalParty.GetActiveExternalParty(context, externalPartyId);
            contactId = contactId > 0 ? contactId : null;
            if (contactId != null)
            {
                ExternalPartyContact.ValidateActiveContact(context, contactId.Value, externalPartyId);
            }
            if (!Enum.IsDefined(form))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Bentuk Sponsorship"));
            }
            var inKind = TransaksiHelper.ToNullable(inKindDescription);
            if (form == SponsorshipForm.Dana)
            {
                TransaksiHelper.ValidatePositive(amount, "Nilai Sponsorship");
            }
            else
            {
                if (amount < 0)
                {
                    throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Nilai Sponsorship"));
                }
                if (inKind == null)
                {
                    throw new ArgumentException(ExceptionMessage.SPONSORSHIP_IN_KIND_REQUIRED);
                }
            }
            if (startDate != null && endDate != null && endDate < startDate)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            periodId = (periodId > 0 ? periodId : null) ?? FinancialTransaction.GetActivePeriodId(context);
            if (periodId != null)
            {
                ManagementPeriod.ValidateOpenPeriod(context, periodId.Value);
            }
            activityId = activityId > 0 ? activityId : null;
            if (activityId != null)
            {
                Activity.GetActivity(context, activityId.Value);
            }
            picMemberId = picMemberId > 0 ? picMemberId : null;
            if (picMemberId != null)
            {
                Member.GetActiveMember(context, picMemberId.Value);
            }
            return new Refs(contactId, periodId, activityId, picMemberId, inKind);
        }
        private static EF.TSponsorship CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            EF.MSponsorshipPackage? package = null;
            if (newrow.PackageId > 0)
            {
                package = SponsorshipPackage.GetActivePackage(context, newrow.PackageId.Value);
            }
            var amount = newrow.Amount == 0 && package != null ? package.Price : newrow.Amount;
            var refs = ValidateData(context, newrow.ExternalPartyId, newrow.ContactId, newrow.PeriodId, newrow.ActivityId, newrow.PicMemberId,
                newrow.SponsorshipForm, amount, newrow.InKindDescription, newrow.StartDate, newrow.EndDate);

            var id = SequenceUtility.GetNextId<EF.TSponsorship>(context);
            var now = DateTime.Now;
            var row = new EF.TSponsorship
            {
                Id = id,
                SponsorshipNumber = TransaksiHelper.GenerateNumber(context, NUMBER_DOCUMENT_TYPE, now, NUMBER_FALLBACK_PREFIX, id, userId),
                ExternalPartyId = newrow.ExternalPartyId,
                ContactId = refs.ContactId,
                PackageId = package?.Id,
                PackageName = package?.Name,
                PeriodId = refs.PeriodId,
                ActivityId = refs.ActivityId,
                SponsorshipForm = (int)newrow.SponsorshipForm,
                Amount = amount,
                InKindDescription = refs.InKindDescription,
                StartDate = newrow.StartDate,
                EndDate = newrow.EndDate,
                PicMemberId = refs.PicMemberId,
                Status = (int)SponsorshipStatus.Aktif,
                Notes = TransaksiHelper.Truncate(newrow.Notes, NOTES_LENGTH),
                InsDate = now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            context.TSponsorships.Add(row);
            if (package != null)
            {
                SponsorshipBenefit.CopyFromPackage(context, row.Id, package.Id, userId);
            }
            return row;
        }
        // Nomor dan paket tetap; nilai tidak boleh di bawah dana yang sudah diterima.
        private void SetRow(EF.ModelContext context, EF.TSponsorship row, int userId)
        {
            TransaksiHelper.ValidateStatus((SponsorshipStatus)row.Status, "mengubah sponsorship", SponsorshipStatus.Aktif);
            var refs = ValidateData(context, ExternalPartyId, ContactId, PeriodId, ActivityId, PicMemberId, SponsorshipForm, Amount, InKindDescription, StartDate, EndDate);
            var received = GetReceived(new() { row.Id }).GetValueOrDefault(row.Id);
            if (Amount < received)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.SPONSORSHIP_PAYMENT_EXCEEDS, Amount - received));
            }

            row.ExternalPartyId = ExternalPartyId;
            row.ContactId = refs.ContactId;
            row.PeriodId = refs.PeriodId;
            row.ActivityId = refs.ActivityId;
            row.SponsorshipForm = (int)SponsorshipForm;
            row.Amount = Amount;
            row.InKindDescription = refs.InKindDescription;
            row.StartDate = StartDate;
            row.EndDate = EndDate;
            row.PicMemberId = refs.PicMemberId;
            row.Notes = TransaksiHelper.Truncate(Notes, NOTES_LENGTH);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetPayment(EF.ModelContext context, EF.TSponsorship row, int accountId, int categoryId, decimal amount, DateOnly receivedDate, int userId)
        {
            TransaksiHelper.ValidateStatus((SponsorshipStatus)row.Status, "menerima dana sponsorship", SponsorshipStatus.Aktif);
            TransaksiHelper.ValidatePositive(amount, "Dana Diterima");
            var remaining = row.Amount - GetReceived(new() { row.Id }).GetValueOrDefault(row.Id);
            if (amount > remaining)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.SPONSORSHIP_PAYMENT_EXCEEDS, remaining));
            }
            var partyName = context.MExternalParties.Where(x => x.Id == row.ExternalPartyId).Select(x => x.Name).Single();
            FinancialTransaction.CreateRow(context, new FinancialTransaction.NewRow
            {
                AccountId = accountId,
                CategoryId = categoryId,
                PeriodId = FinancialTransaction.OpenPeriodOrDefault(context, row.PeriodId),
                TransactionType = CashFlowType.Income,
                Amount = amount,
                TransactionDate = receivedDate,
                Description = $"Sponsorship {row.SponsorshipNumber} - {partyName}",
                ActivityId = row.ActivityId,
                SponsorshipId = row.Id
            }, null, true, userId);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetFinish(EF.ModelContext context, EF.TSponsorship row, int userId)
        {
            TransaksiHelper.ValidateStatus((SponsorshipStatus)row.Status, "menyelesaikan sponsorship", SponsorshipStatus.Aktif);
            var pending = context.TSponsorshipBenefits.Count(x => x.SponsorshipId == row.Id && x.IsDone == (int)SponsorshipBenefitStatus.Belum);
            if (pending > 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.SPONSORSHIP_BENEFIT_PENDING, pending));
            }
            row.Status = (int)SponsorshipStatus.Selesai;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetCancel(EF.ModelContext context, EF.TSponsorship row, string? reason, int userId)
        {
            TransaksiHelper.ValidateStatus((SponsorshipStatus)row.Status, "membatalkan sponsorship", SponsorshipStatus.Aktif);
            if (GetReceived(new() { row.Id }).GetValueOrDefault(row.Id) > 0)
            {
                throw new ArgumentException(ExceptionMessage.SPONSORSHIP_HAS_PAYMENT);
            }
            row.Notes = TransaksiHelper.AppendNote(row.Notes, "Batal", reason, NOTES_LENGTH);
            row.Status = (int)SponsorshipStatus.Batal;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetDelete(EF.ModelContext context, EF.TSponsorship row)
        {
            TransaksiHelper.ValidateStatus((SponsorshipStatus)row.Status, "menghapus sponsorship", SponsorshipStatus.Aktif);
            if (context.TFinancialTransactions.Any(x => x.SponsorshipId == row.Id) || context.TSponsorshipMous.Any(x => x.SponsorshipId == row.Id)
                || context.MAssets.Any(x => x.SponsorshipId == row.Id))
            {
                throw new ArgumentException(ExceptionMessage.SPONSORSHIP_IN_USE);
            }
            context.TSponsorshipBenefits.RemoveRange(context.TSponsorshipBenefits.Where(x => x.SponsorshipId == row.Id));
            context.TSponsorships.Remove(row);
        }
        #endregion
    }
}
