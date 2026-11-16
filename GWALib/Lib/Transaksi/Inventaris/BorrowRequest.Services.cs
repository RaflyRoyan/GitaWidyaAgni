using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
using GWALib.Lib.Master.Inventaris;
using GWALib.Lib.Master.Mitra;
using GWALib.Lib.Transaksi.Keuangan;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Inventaris
{
    public partial class BorrowRequest
    {
        public const string NUMBER_DOCUMENT_TYPE = "BORROW";
        private const string NUMBER_FALLBACK_PREFIX = "PJM";
        private const int TEXT_LENGTH = 255;

        // Pengambilan per item: UnitCodes wajib untuk aset per unit (jumlahnya = Quantity item); Condition untuk aset cukup jumlah.
        public class CheckoutLine
        {
            public int BorrowItemId { get; set; }
            public List<string>? UnitCodes { get; set; }
            public ItemCondition? Condition { get; set; }
        }

        #region SYNC
        public List<BorrowItem> GetItems(bool refresh)
        {
            if (refresh || ItemList == null)
            {
                ItemList = BorrowItem.GetDataList(Id);
            }
            return ItemList;
        }
        public static List<BorrowRequest> GetDataList(BorrowRequestStatus? status = null, BorrowerType? borrowerType = null, int? memberId = null,
            int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, borrowerType, memberId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<BorrowRequest, EF.TBorrowRequest>();
        }
        public static Response<BorrowRequest> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                transaction.Commit();
                return new BorrowRequest(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<BorrowRequest>(msg));
        }
        public Response<BorrowRequest> Update(string user)
        {
            return Run(SetRow, user);
        }
        public Response<BorrowRequest> Approve(string user)
        {
            return Run((context, row, userId) => SetDecision(row, BorrowRequestStatus.Approved, null, userId), user);
        }
        public Response<BorrowRequest> Reject(string? reason, string user)
        {
            return Run((context, row, userId) => SetDecision(row, BorrowRequestStatus.Rejected, reason, userId), user);
        }
        public Response<BorrowRequest> Cancel(string? reason, string user)
        {
            return Run((context, row, userId) => SetCancel(row, reason, userId), user);
        }
        // Semua aset diambil sekaligus; unit dipindai, status unit / aset dan mutasi Pinjam dicatat dalam satu transaksi DB.
        public Response<BorrowRequest> Checkout(List<CheckoutLine> lines, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = GetRow(context, Id);
                SetCheckout(context, row, lines, userId);
                context.SaveChanges();
                transaction.Commit();
                return new BorrowRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<BorrowRequest>(msg));
        }
        public Response<BorrowRequest> WaiveLateFee(decimal amount, string? note, string user)
        {
            return Run((context, row, userId) => SetWaive(row, amount, note, userId), user);
        }
        public Response<BorrowRequest> ReceivePayment(int accountId, int categoryId, decimal amount, DateOnly paidDate, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = GetRow(context, Id);
                SetPayment(context, row, accountId, categoryId, amount, paidDate, userId);
                context.SaveChanges();
                transaction.Commit();
                return new BorrowRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<BorrowRequest>(msg));
        }
        public Response<BorrowRequest> CancelPayment(int transactionId, string? reason, string user)
        {
            return Run((context, row, userId) => SetCancelPayment(context, row, transactionId, reason, userId), user);
        }
        // Untuk job terjadwal: Borrowed yang lewat rencana tanggal kembali menjadi Overdue. Mengembalikan jumlah yang ditandai.
        public static Response<int> MarkOverdue(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = SetOverdue(context, userId);
                context.SaveChanges();
                return count;
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<int>(msg));
        }
        // Hanya Pending / Rejected / Cancelled (item ikut terhapus lewat ON DELETE CASCADE).
        public Response<BorrowRequest> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                TransaksiHelper.ValidateStatus((BorrowRequestStatus)row.Status, "menghapus peminjaman",
                    BorrowRequestStatus.Pending, BorrowRequestStatus.Rejected, BorrowRequestStatus.Cancelled);
                var data = new BorrowRequest(Reload(row.Id));
                context.TBorrowRequests.Remove(row);
                context.SaveChanges();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<BorrowRequest>(msg));
        }
        #endregion

        #region ASYNC
        public async Task<List<BorrowItem>> GetItemsAsync(bool refresh)
        {
            if (refresh || ItemList == null)
            {
                ItemList = await BorrowItem.GetDataListAsync(Id);
            }
            return ItemList;
        }
        public static async Task<List<BorrowRequest>> GetDataListAsync(BorrowRequestStatus? status = null, BorrowerType? borrowerType = null, int? memberId = null,
            int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(status, borrowerType, memberId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<BorrowRequest, EF.TBorrowRequest>();
        }
        public static async Task<Response<BorrowRequest>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new BorrowRequest(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<BorrowRequest>(msg));
        }
        public Task<Response<BorrowRequest>> UpdateAsync(string user)
        {
            return RunAsync(SetRow, user);
        }
        public Task<Response<BorrowRequest>> ApproveAsync(string user)
        {
            return RunAsync((context, row, userId) => SetDecision(row, BorrowRequestStatus.Approved, null, userId), user);
        }
        public Task<Response<BorrowRequest>> RejectAsync(string? reason, string user)
        {
            return RunAsync((context, row, userId) => SetDecision(row, BorrowRequestStatus.Rejected, reason, userId), user);
        }
        public Task<Response<BorrowRequest>> CancelAsync(string? reason, string user)
        {
            return RunAsync((context, row, userId) => SetCancel(row, reason, userId), user);
        }
        public async Task<Response<BorrowRequest>> CheckoutAsync(List<CheckoutLine> lines, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = await GetRowAsync(context, Id);
                SetCheckout(context, row, lines, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new BorrowRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<BorrowRequest>(msg));
        }
        public Task<Response<BorrowRequest>> WaiveLateFeeAsync(decimal amount, string? note, string user)
        {
            return RunAsync((context, row, userId) => SetWaive(row, amount, note, userId), user);
        }
        public async Task<Response<BorrowRequest>> ReceivePaymentAsync(int accountId, int categoryId, decimal amount, DateOnly paidDate, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = await GetRowAsync(context, Id);
                SetPayment(context, row, accountId, categoryId, amount, paidDate, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new BorrowRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<BorrowRequest>(msg));
        }
        public Task<Response<BorrowRequest>> CancelPaymentAsync(int transactionId, string? reason, string user)
        {
            return RunAsync((context, row, userId) => SetCancelPayment(context, row, transactionId, reason, userId), user);
        }
        public static async Task<Response<int>> MarkOverdueAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var count = SetOverdue(context, userId);
                await context.SaveChangesAsync();
                return count;
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<int>(msg));
        }
        public async Task<Response<BorrowRequest>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                TransaksiHelper.ValidateStatus((BorrowRequestStatus)row.Status, "menghapus peminjaman",
                    BorrowRequestStatus.Pending, BorrowRequestStatus.Rejected, BorrowRequestStatus.Cancelled);
                var data = new BorrowRequest(Reload(row.Id));
                context.TBorrowRequests.Remove(row);
                await context.SaveChangesAsync();
                return data;
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<BorrowRequest>(msg));
        }
        #endregion

        #region INTERNAL
        // Peminjaman yang item-nya masih boleh diubah (Pending); dipakai BorrowItem.
        // Local: peminjaman yang baru di-Add dalam command yang sama belum ada di DB.
        internal static EF.TBorrowRequest GetPendingRequest(EF.ModelContext context, int requestId)
        {
            var row = GetLocalOrDb(context, requestId);
            TransaksiHelper.ValidateStatus((BorrowRequestStatus)row.Status, "mengubah aset peminjaman", BorrowRequestStatus.Pending);
            return row;
        }
        // Peminjaman yang sedang di luar (Borrowed / Overdue); dipakai pengembalian.
        internal static EF.TBorrowRequest GetOutRequest(EF.ModelContext context, int requestId)
        {
            var row = GetLocalOrDb(context, requestId);
            TransaksiHelper.ValidateStatus((BorrowRequestStatus)row.Status, "mengembalikan aset", BorrowRequestStatus.Borrowed, BorrowRequestStatus.Overdue);
            return row;
        }
        // Lama sewa = CEIL(hari rencana kembali - tanggal pinjam), minimal 1.
        internal static int GetRentalDays(EF.TBorrowRequest request)
        {
            return Math.Max(1, (int)Math.Ceiling((request.ExpectedReturnDate - request.BorrowDate).TotalDays));
        }
        internal static decimal GetRentalFee(EF.TBorrowRequest request, decimal feePerDay, int quantity)
        {
            return request.RentalScheme == (int)RentalScheme.TarifTetap ? GetRentalDays(request) * feePerDay * quantity : 0;
        }
        // Total sewa / denda dari item yang tersisa di context, status pembayaran, dan Returned bila semua aset sudah kembali.
        internal static void RecalculateTotals(EF.ModelContext context, EF.TBorrowRequest request, int userId)
        {
            if (context.Entry(request).State != EntityState.Added)
            {
                context.TBorrowItems.Where(x => x.BorrowRequestId == request.Id).Load();
            }
            var items = context.TBorrowItems.Local.Where(x => x.BorrowRequestId == request.Id).ToList();
            request.TotalRentalFee = items.Sum(x => x.RentalFee);
            request.TotalLateFee = items.Sum(x => x.LateFee);
            request.LateFeeWaived = Math.Min(request.LateFeeWaived, request.TotalLateFee);
            request.PaymentStatus = (int)GetPaymentStatus(request.TotalRentalFee + request.TotalLateFee - request.LateFeeWaived, request.PaidAmount);
            var now = DateTime.Now;
            if ((request.Status == (int)BorrowRequestStatus.Borrowed || request.Status == (int)BorrowRequestStatus.Overdue)
                && items.Count > 0 && items.All(x => x.ReturnedQuantity >= x.Quantity))
            {
                request.Status = (int)BorrowRequestStatus.Returned;
                request.ActualReturnDate = now;
            }
            request.UpdDate = now;
            request.UpdBy = userId;
        }
        #endregion

        #region HELPER
        private delegate void RowAction(EF.ModelContext context, EF.TBorrowRequest row, int userId);

        private Response<BorrowRequest> Run(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                return new BorrowRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<BorrowRequest>(msg));
        }
        private async Task<Response<BorrowRequest>> RunAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                return new BorrowRequest(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<BorrowRequest>(msg));
        }
        private static EF.TBorrowRequest GetLocalOrDb(EF.ModelContext context, int requestId)
        {
            return context.TBorrowRequests.Local.SingleOrDefault(x => x.Id == requestId)
                ?? context.TBorrowRequests.SingleOrDefault(x => x.Id == requestId)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Peminjaman"));
        }
        private static EF.TBorrowRequest Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TBorrowRequest> FilterQuery(BorrowRequestStatus? status, BorrowerType? borrowerType, int? memberId)
        {
            return _Querys.Where(x => (status == null || x.Status == (int)status)
                    && (borrowerType == null || x.BorrowerType == (int)borrowerType)
                    && (memberId == null || x.MemberId == memberId))
                .OrderByDescending(x => x.BorrowDate).ThenByDescending(x => x.Id);
        }
        private static EF.TBorrowRequest GetRow(EF.ModelContext context, int id)
        {
            return context.TBorrowRequests.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Peminjaman"));
        }
        private static async Task<EF.TBorrowRequest> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TBorrowRequests.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Peminjaman"));
        }
        private static PaymentStatus GetPaymentStatus(decimal due, decimal paid)
        {
            if (due <= 0)
            {
                return paid > 0 ? PaymentStatus.Lunas : PaymentStatus.TidakAdaTagihan;
            }
            return paid >= due ? PaymentStatus.Lunas : paid > 0 ? PaymentStatus.Sebagian : PaymentStatus.BelumBayar;
        }
        private record Borrower(int? MemberId, int? ExternalPartyId, int? ContactId, string? Name);

        // Kolom peminjam yang tidak sesuai jenisnya dikosongkan; nama diisi dari anggota / mitra bila kosong.
        private static Borrower ValidateBorrower(EF.ModelContext context, BorrowerType borrowerType, int? memberId, int? externalPartyId, int? contactId,
            string? borrowerName, RentalScheme scheme, int? letterId, string? purpose, DateTime borrowDate, DateTime expectedReturnDate)
        {
            if (!Enum.IsDefined(borrowerType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Peminjam"));
            }
            if (!Enum.IsDefined(scheme))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Skema Sewa"));
            }
            if (scheme == RentalScheme.Seikhlasnya && borrowerType != BorrowerType.Anggota)
            {
                throw new ArgumentException(ExceptionMessage.BORROW_SCHEME_MEMBER_ONLY);
            }
            if (string.IsNullOrWhiteSpace(purpose))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Keperluan"));
            }
            if (borrowDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Pinjam"));
            }
            if (expectedReturnDate <= borrowDate)
            {
                throw new ArgumentException(ExceptionMessage.BORROW_RETURN_DATE_INVALID);
            }
            if (letterId > 0 && !context.TLetters.Any(x => x.Id == letterId))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Surat Permohonan"));
            }
            var name = TransaksiHelper.Truncate(borrowerName, 150);
            switch (borrowerType)
            {
                case BorrowerType.Anggota:
                    if (!(memberId > 0))
                    {
                        throw new ArgumentException(ExceptionMessage.BORROWER_MEMBER_REQUIRED);
                    }
                    var member = Member.GetActiveMember(context, memberId!.Value);
                    return new Borrower(member.Id, null, null, name ?? member.Fullname);
                case BorrowerType.OrganisasiInstansi:
                    if (!(externalPartyId > 0))
                    {
                        throw new ArgumentException(ExceptionMessage.BORROWER_PARTY_REQUIRED);
                    }
                    var party = ExternalParty.GetActiveExternalParty(context, externalPartyId!.Value);
                    contactId = contactId > 0 ? contactId : null;
                    if (contactId != null)
                    {
                        ExternalPartyContact.ValidateActiveContact(context, contactId.Value, party.Id);
                    }
                    return new Borrower(null, party.Id, contactId, name ?? party.Name);
                default:
                    if (name == null)
                    {
                        throw new ArgumentException(ExceptionMessage.BORROWER_NAME_REQUIRED);
                    }
                    return new Borrower(null, null, null, name);
            }
        }
        private static EF.TBorrowRequest CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var borrower = ValidateBorrower(context, newrow.BorrowerType, newrow.MemberId, newrow.ExternalPartyId, newrow.ContactId, newrow.BorrowerName,
                newrow.RentalScheme, newrow.LetterId, newrow.Purpose, newrow.BorrowDate, newrow.ExpectedReturnDate);
            if (newrow.Items == null || newrow.Items.Count == 0)
            {
                throw new ArgumentException(ExceptionMessage.BORROW_ITEM_REQUIRED);
            }
            var id = SequenceUtility.GetNextId<EF.TBorrowRequest>(context);
            var row = new EF.TBorrowRequest
            {
                Id = id,
                RequestNumber = TransaksiHelper.GenerateNumber(context, NUMBER_DOCUMENT_TYPE, newrow.BorrowDate, NUMBER_FALLBACK_PREFIX, id, userId),
                BorrowerType = (int)newrow.BorrowerType,
                MemberId = borrower.MemberId,
                ExternalPartyId = borrower.ExternalPartyId,
                ContactId = borrower.ContactId,
                BorrowerName = borrower.Name,
                BorrowerInstitution = TransaksiHelper.Truncate(newrow.BorrowerInstitution, 150),
                BorrowerPhone = TransaksiHelper.ToNullable(newrow.BorrowerPhone),
                BorrowerEmail = TransaksiHelper.ToNullable(newrow.BorrowerEmail),
                Guarantee = TransaksiHelper.Truncate(newrow.Guarantee, TEXT_LENGTH),
                LetterId = newrow.LetterId > 0 ? newrow.LetterId : null,
                Purpose = TransaksiHelper.Truncate(newrow.Purpose, TEXT_LENGTH)!,
                BorrowDate = newrow.BorrowDate,
                ExpectedReturnDate = newrow.ExpectedReturnDate,
                RentalScheme = (int)newrow.RentalScheme,
                TotalLateFee = 0,
                LateFeeWaived = 0,
                PaidAmount = 0,
                Status = (int)BorrowRequestStatus.Pending,
                Notes = TransaksiHelper.Truncate(newrow.Notes, TEXT_LENGTH),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            context.TBorrowRequests.Add(row);
            foreach (var item in newrow.Items)
            {
                item.BorrowRequestId = row.Id;
                BorrowItem.CreateRow(context, item, userId);
            }
            RecalculateTotals(context, row, userId);
            row.UpdDate = null;
            row.UpdBy = null;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        // Hanya Pending; sewa item dihitung ulang dari tanggal / skema baru.
        private void SetRow(EF.ModelContext context, EF.TBorrowRequest row, int userId)
        {
            TransaksiHelper.ValidateStatus((BorrowRequestStatus)row.Status, "mengubah peminjaman", BorrowRequestStatus.Pending);
            var borrower = ValidateBorrower(context, BorrowerType, MemberId, ExternalPartyId, ContactId, BorrowerName, RentalScheme, LetterId, Purpose, BorrowDate, ExpectedReturnDate);
            row.BorrowerType = (int)BorrowerType;
            row.MemberId = borrower.MemberId;
            row.ExternalPartyId = borrower.ExternalPartyId;
            row.ContactId = borrower.ContactId;
            row.BorrowerName = borrower.Name;
            row.BorrowerInstitution = TransaksiHelper.Truncate(BorrowerInstitution, 150);
            row.BorrowerPhone = TransaksiHelper.ToNullable(BorrowerPhone);
            row.BorrowerEmail = TransaksiHelper.ToNullable(BorrowerEmail);
            row.Guarantee = TransaksiHelper.Truncate(Guarantee, TEXT_LENGTH);
            row.LetterId = LetterId > 0 ? LetterId : null;
            row.Purpose = TransaksiHelper.Truncate(Purpose, TEXT_LENGTH)!;
            row.BorrowDate = BorrowDate;
            row.ExpectedReturnDate = ExpectedReturnDate;
            row.RentalScheme = (int)RentalScheme;
            row.Notes = TransaksiHelper.Truncate(Notes, TEXT_LENGTH);
            foreach (var item in context.TBorrowItems.Where(x => x.BorrowRequestId == row.Id).ToList())
            {
                item.RentalDays = GetRentalDays(row);
                item.RentalFee = GetRentalFee(row, item.RentalFeePerDay, item.Quantity);
            }
            RecalculateTotals(context, row, userId);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        private static void SetDecision(EF.TBorrowRequest row, BorrowRequestStatus decision, string? reason, int userId)
        {
            TransaksiHelper.ValidateStatus((BorrowRequestStatus)row.Status, decision == BorrowRequestStatus.Approved ? "menyetujui peminjaman" : "menolak peminjaman",
                BorrowRequestStatus.Pending);
            var now = DateTime.Now;
            if (decision == BorrowRequestStatus.Rejected)
            {
                row.Notes = TransaksiHelper.AppendNote(row.Notes, "Ditolak", reason, TEXT_LENGTH);
            }
            row.Status = (int)decision;
            row.ApprovedBy = userId;
            row.ApprovedAt = now;
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        private static void SetCancel(EF.TBorrowRequest row, string? reason, int userId)
        {
            TransaksiHelper.ValidateStatus((BorrowRequestStatus)row.Status, "membatalkan peminjaman", BorrowRequestStatus.Pending, BorrowRequestStatus.Approved);
            if (row.PaidAmount > 0)
            {
                throw new ArgumentException(ExceptionMessage.BORROW_HAS_PAYMENT);
            }
            row.Notes = TransaksiHelper.AppendNote(row.Notes, "Batal", reason, TEXT_LENGTH);
            row.Status = (int)BorrowRequestStatus.Cancelled;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static void SetCheckout(EF.ModelContext context, EF.TBorrowRequest row, List<CheckoutLine>? lines, int userId)
        {
            TransaksiHelper.ValidateStatus((BorrowRequestStatus)row.Status, "mengambil aset peminjaman", BorrowRequestStatus.Approved);
            var items = context.TBorrowItems.Where(x => x.BorrowRequestId == row.Id).ToList();
            if (items.Count == 0)
            {
                throw new ArgumentException(ExceptionMessage.BORROW_ITEM_REQUIRED);
            }
            var lineMap = (lines ?? new()).ToDictionary(x => x.BorrowItemId);
            if (lineMap.Keys.Any(id => items.All(x => x.Id != id)))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Item Peminjaman"));
            }
            var now = DateTime.Now;
            var assets = new List<EF.MAsset>();
            // Ketersediaan dicek sebelum status berubah supaya item peminjaman ini belum dihitung keluar.
            foreach (var item in items)
            {
                var asset = Asset.GetAsset(context, item.AssetId);
                if (asset.Status == (int)AssetStatus.Maintenance)
                {
                    throw new ArgumentException(ExceptionMessage.ASSET_IN_USE);
                }
                lineMap.TryGetValue(item.Id, out var line);
                if (asset.TrackUnit == (int)UnitTracking.PerUnit)
                {
                    var codes = (line?.UnitCodes ?? new()).Select(x => x?.Trim().ToUpperInvariant()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                    if (codes.Count != item.Quantity)
                    {
                        throw new ArgumentException(string.Format(ExceptionMessage.BORROW_UNIT_COUNT_MISMATCH, asset.Name, item.Quantity));
                    }
                    foreach (var code in codes)
                    {
                        BorrowItemUnit.CreateRow(context, row, item, asset, InventarisHelper.GetUnitByCode(context, code, asset), now, userId);
                    }
                }
                else
                {
                    var available = InventarisHelper.GetAvailableQuantity(context, asset);
                    if (item.Quantity > available)
                    {
                        throw new ArgumentException(string.Format(ExceptionMessage.ASSET_QUANTITY_INSUFFICIENT, asset.Name, available));
                    }
                    item.ConditionOnBorrow = (int)(line?.Condition ?? (ItemCondition)asset.AssetCondition);
                    InventarisHelper.AddMovement(context, asset.Id, null, AssetMovementType.Borrow, item.Quantity, InventarisHelper.REF_BORROW, row.Id,
                        "Dipinjam " + row.RequestNumber, userId);
                }
                item.UpdDate = now;
                item.UpdBy = userId;
                assets.Add(asset);
            }
            row.Status = (int)BorrowRequestStatus.Borrowed;
            row.UpdDate = now;
            row.UpdBy = userId;
            foreach (var asset in assets)
            {
                InventarisHelper.RefreshAssetStatus(context, asset, userId);
            }
        }
        private static void SetWaive(EF.TBorrowRequest row, decimal amount, string? note, int userId)
        {
            TransaksiHelper.ValidateStatus((BorrowRequestStatus)row.Status, "memotong denda", BorrowRequestStatus.Borrowed, BorrowRequestStatus.Overdue, BorrowRequestStatus.Returned);
            if (amount < 0 || amount > row.TotalLateFee)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.BORROW_WAIVE_EXCEEDS, row.TotalLateFee));
            }
            row.LateFeeWaived = amount;
            row.WaiveNote = TransaksiHelper.Truncate(note, TEXT_LENGTH);
            row.PaymentStatus = (int)GetPaymentStatus(row.TotalRentalFee + row.TotalLateFee - row.LateFeeWaived, row.PaidAmount);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static decimal GetPaidTotal(EF.ModelContext context, int requestId, int? excludeTransactionId)
        {
            return context.TFinancialTransactions
                .Where(x => x.BorrowRequestId == requestId && x.Status == (int)TransactionStatus.Posted
                    && x.TransactionType == (int)CashFlowType.Income && x.Id != excludeTransactionId)
                .Sum(x => x.Amount);
        }
        // Kategori menentukan jenis pembayaran (sewa / sumbangan / denda).
        private static void SetPayment(EF.ModelContext context, EF.TBorrowRequest row, int accountId, int categoryId, decimal amount, DateOnly paidDate, int userId)
        {
            TransaksiHelper.ValidateStatus((BorrowRequestStatus)row.Status, "menerima pembayaran peminjaman",
                BorrowRequestStatus.Approved, BorrowRequestStatus.Borrowed, BorrowRequestStatus.Overdue, BorrowRequestStatus.Returned);
            TransaksiHelper.ValidatePositive(amount, "Nominal Pembayaran");
            var paid = GetPaidTotal(context, row.Id, null);
            var due = row.TotalRentalFee + row.TotalLateFee - row.LateFeeWaived;
            // Skema seikhlasnya tidak punya tagihan sewa, jadi sumbangan tidak dibatasi.
            if (row.RentalScheme != (int)RentalScheme.Seikhlasnya && amount > due - paid)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.BORROW_PAYMENT_EXCEEDS, due - paid));
            }
            FinancialTransaction.CreateRow(context, new FinancialTransaction.NewRow
            {
                AccountId = accountId,
                CategoryId = categoryId,
                TransactionType = CashFlowType.Income,
                Amount = amount,
                TransactionDate = paidDate,
                Description = "Peminjaman " + row.RequestNumber,
                BorrowRequestId = row.Id
            }, null, true, userId);
            SetPaid(row, paid + amount, userId);
        }
        private static void SetCancelPayment(EF.ModelContext context, EF.TBorrowRequest row, int transactionId, string? reason, int userId)
        {
            var trx = context.TFinancialTransactions.SingleOrDefault(x => x.Id == transactionId && x.BorrowRequestId == row.Id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pembayaran Peminjaman"));
            FinancialTransaction.VoidRow(context, trx, reason ?? "Pembayaran peminjaman dibatalkan", userId);
            SetPaid(row, GetPaidTotal(context, row.Id, trx.Id), userId);
        }
        private static void SetPaid(EF.TBorrowRequest row, decimal paid, int userId)
        {
            row.PaidAmount = paid;
            row.PaymentStatus = (int)GetPaymentStatus(row.TotalRentalFee + row.TotalLateFee - row.LateFeeWaived, paid);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        private static int SetOverdue(EF.ModelContext context, int userId)
        {
            var now = DateTime.Now;
            var rows = context.TBorrowRequests.Where(x => x.Status == (int)BorrowRequestStatus.Borrowed && x.ExpectedReturnDate < now).ToList();
            foreach (var row in rows)
            {
                row.Status = (int)BorrowRequestStatus.Overdue;
                row.UpdDate = now;
                row.UpdBy = userId;
            }
            return rows.Count;
        }
        #endregion
    }
}
