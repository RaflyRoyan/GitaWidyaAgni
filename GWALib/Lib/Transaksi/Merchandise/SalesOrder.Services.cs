using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Anggota;
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

namespace GWALib.Lib.Transaksi.Merchandise
{
    public partial class SalesOrder
    {
        public const string NUMBER_DOCUMENT_TYPE = "DANUS";
        private const string NUMBER_FALLBACK_PREFIX = "DNS";
        private const int NOTES_LENGTH = 500;

        #region SYNC
        public List<SalesOrderItem> GetItems(bool refresh)
        {
            if (refresh || ItemList == null)
            {
                ItemList = SalesOrderItem.GetDataList(Id);
            }
            return ItemList;
        }
        public static List<SalesOrder> GetDataList(int? periodId = null, int? activityId = null, FulfillmentStatus? fulfillmentStatus = null,
            PaymentStatus? paymentStatus = null, int? buyerMemberId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, activityId, fulfillmentStatus, paymentStatus, buyerMemberId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return query.ToList().MappingData<SalesOrder, EF.TSalesOrder>();
        }
        // Pesanan, item dan pemotongan stok disimpan dalam satu transaksi DB.
        public static Response<SalesOrder> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                using var transaction = context.Database.BeginTransaction();
                var row = CreateRow(context, newrow, userId);
                context.SaveChanges();
                transaction.Commit();
                return new SalesOrder(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<SalesOrder>(msg));
        }
        // Hanya data pembeli, kegiatan, PIC dan catatan; item tetap.
        public Response<SalesOrder> Update(string user)
        {
            return Run(SetRow, user);
        }
        public Response<SalesOrder> SetReady(string user)
        {
            return Run((context, row, userId) => SetFulfillment(row, FulfillmentStatus.SiapDiambil, userId), user);
        }
        public Response<SalesOrder> Finish(string user)
        {
            return Run((context, row, userId) => SetFulfillment(row, FulfillmentStatus.Selesai, userId), user);
        }
        public Response<SalesOrder> Cancel(string? reason, string user)
        {
            return Run((context, row, userId) => SetCancel(context, row, reason, userId), user);
        }
        public Response<SalesOrder> ReceivePayment(int accountId, int categoryId, decimal amount, DateOnly paidDate, string user)
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
                return new SalesOrder(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SalesOrder>(msg));
        }
        public Response<SalesOrder> CancelPayment(int transactionId, string? reason, string user)
        {
            return Run((context, row, userId) => SetCancelPayment(context, row, transactionId, reason, userId), user);
        }
        #endregion

        #region ASYNC
        public async Task<List<SalesOrderItem>> GetItemsAsync(bool refresh)
        {
            if (refresh || ItemList == null)
            {
                ItemList = await SalesOrderItem.GetDataListAsync(Id);
            }
            return ItemList;
        }
        public static async Task<List<SalesOrder>> GetDataListAsync(int? periodId = null, int? activityId = null, FulfillmentStatus? fulfillmentStatus = null,
            PaymentStatus? paymentStatus = null, int? buyerMemberId = null, int startdata = -1, int jmldata = 50)
        {
            var query = FilterQuery(periodId, activityId, fulfillmentStatus, paymentStatus, buyerMemberId);

            if (startdata > -1)
            {
                query = query.Skip(startdata).Take(jmldata);
            }
            return await (await query.ToListAsync()).MappingDataAsync<SalesOrder, EF.TSalesOrder>();
        }
        public static async Task<Response<SalesOrder>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                await using var transaction = await context.Database.BeginTransactionAsync();
                var row = CreateRow(context, newrow, userId);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new SalesOrder(Reload(row.Id));
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<SalesOrder>(msg));
        }
        public Task<Response<SalesOrder>> UpdateAsync(string user)
        {
            return RunAsync(SetRow, user);
        }
        public Task<Response<SalesOrder>> SetReadyAsync(string user)
        {
            return RunAsync((context, row, userId) => SetFulfillment(row, FulfillmentStatus.SiapDiambil, userId), user);
        }
        public Task<Response<SalesOrder>> FinishAsync(string user)
        {
            return RunAsync((context, row, userId) => SetFulfillment(row, FulfillmentStatus.Selesai, userId), user);
        }
        public Task<Response<SalesOrder>> CancelAsync(string? reason, string user)
        {
            return RunAsync((context, row, userId) => SetCancel(context, row, reason, userId), user);
        }
        public async Task<Response<SalesOrder>> ReceivePaymentAsync(int accountId, int categoryId, decimal amount, DateOnly paidDate, string user)
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
                return new SalesOrder(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SalesOrder>(msg));
        }
        public Task<Response<SalesOrder>> CancelPaymentAsync(int transactionId, string? reason, string user)
        {
            return RunAsync((context, row, userId) => SetCancelPayment(context, row, transactionId, reason, userId), user);
        }
        #endregion

        #region HELPER
        private delegate void RowAction(EF.ModelContext context, EF.TSalesOrder row, int userId);

        private Response<SalesOrder> Run(RowAction action, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                action(context, row, userId);
                context.SaveChanges();
                return new SalesOrder(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SalesOrder>(msg));
        }
        private async Task<Response<SalesOrder>> RunAsync(RowAction action, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                action(context, row, userId);
                await context.SaveChangesAsync();
                return new SalesOrder(Reload(row.Id));
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<SalesOrder>(msg));
        }
        private static EF.TSalesOrder Reload(int id)
        {
            return _Querys.Single(x => x.Id == id);
        }
        private static IQueryable<EF.TSalesOrder> FilterQuery(int? periodId, int? activityId, FulfillmentStatus? fulfillmentStatus, PaymentStatus? paymentStatus, int? buyerMemberId)
        {
            return _Querys.Where(x => (periodId == null || x.PeriodId == periodId)
                    && (activityId == null || x.ActivityId == activityId)
                    && (fulfillmentStatus == null || x.FulfillmentStatus == (int)fulfillmentStatus)
                    && (paymentStatus == null || x.PaymentStatus == (int)paymentStatus)
                    && (buyerMemberId == null || x.BuyerMemberId == buyerMemberId))
                .OrderByDescending(x => x.OrderDate).ThenByDescending(x => x.Id);
        }
        private static EF.TSalesOrder GetRow(EF.ModelContext context, int id)
        {
            return context.TSalesOrders.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pesanan Penjualan"));
        }
        private static async Task<EF.TSalesOrder> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TSalesOrders.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pesanan Penjualan"));
        }
        private record Buyer(int? MemberId, string? Name, int? ActivityId, int? PicMemberId);

        // Pembeli anggota mengosongkan nama pembeli.
        private static Buyer ValidateData(EF.ModelContext context, int? buyerMemberId, string? buyerName, int? activityId, int? picMemberId)
        {
            buyerMemberId = buyerMemberId > 0 ? buyerMemberId : null;
            var name = buyerMemberId != null ? null : TransaksiHelper.Truncate(buyerName, 150);
            if (buyerMemberId != null)
            {
                Member.GetMember(context, buyerMemberId.Value);
            }
            else if (name == null)
            {
                throw new ArgumentException(ExceptionMessage.SALES_BUYER_REQUIRED);
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
            return new Buyer(buyerMemberId, name, activityId, picMemberId);
        }
        private static EF.TSalesOrder CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            var buyer = ValidateData(context, newrow.BuyerMemberId, newrow.BuyerName, newrow.ActivityId, newrow.PicMemberId);
            if (newrow.OrderDate == default)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Tanggal Pesanan"));
            }
            if (newrow.Items == null || newrow.Items.Count == 0)
            {
                throw new ArgumentException(ExceptionMessage.SALES_ITEM_REQUIRED);
            }
            var periodId = (newrow.PeriodId > 0 ? newrow.PeriodId : null) ?? FinancialTransaction.GetActivePeriodId(context);
            if (periodId != null)
            {
                ManagementPeriod.ValidateOpenPeriod(context, periodId.Value);
            }

            var id = SequenceUtility.GetNextId<EF.TSalesOrder>(context);
            var row = new EF.TSalesOrder
            {
                Id = id,
                OrderNumber = TransaksiHelper.GenerateNumber(context, NUMBER_DOCUMENT_TYPE, newrow.OrderDate.ToDateTime(TimeOnly.MinValue), NUMBER_FALLBACK_PREFIX, id, userId),
                PeriodId = periodId,
                ActivityId = buyer.ActivityId,
                BuyerMemberId = buyer.MemberId,
                BuyerName = buyer.Name,
                BuyerPhone = TransaksiHelper.ToNullable(newrow.BuyerPhone),
                OrderDate = newrow.OrderDate,
                PaidAmount = 0,
                FulfillmentStatus = (int)FulfillmentStatus.Dipesan,
                PicMemberId = buyer.PicMemberId,
                Notes = TransaksiHelper.Truncate(newrow.Notes, NOTES_LENGTH),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            context.TSalesOrders.Add(row);
            row.TotalAmount = newrow.Items.Sum(item => SalesOrderItem.CreateRow(context, row, item, userId).Subtotal);
            row.PaymentStatus = (int)GetPaymentStatus(row.TotalAmount, 0);
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private static PaymentStatus GetPaymentStatus(decimal total, decimal paid)
        {
            return paid >= total ? PaymentStatus.Lunas : paid > 0 ? PaymentStatus.Sebagian : PaymentStatus.BelumBayar;
        }
        private void SetRow(EF.ModelContext context, EF.TSalesOrder row, int userId)
        {
            TransaksiHelper.ValidateStatus((FulfillmentStatus)row.FulfillmentStatus, "mengubah pesanan", FulfillmentStatus.Dipesan, FulfillmentStatus.SiapDiambil);
            var buyer = ValidateData(context, BuyerMemberId, BuyerName, ActivityId, PicMemberId);
            row.BuyerMemberId = buyer.MemberId;
            row.BuyerName = buyer.Name;
            row.BuyerPhone = TransaksiHelper.ToNullable(BuyerPhone);
            row.ActivityId = buyer.ActivityId;
            row.PicMemberId = buyer.PicMemberId;
            row.Notes = TransaksiHelper.Truncate(Notes, NOTES_LENGTH);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        // SiapDiambil dari Dipesan; Selesai dari Dipesan / SiapDiambil dan harus Lunas.
        private static void SetFulfillment(EF.TSalesOrder row, FulfillmentStatus target, int userId)
        {
            if (target == FulfillmentStatus.SiapDiambil)
            {
                TransaksiHelper.ValidateStatus((FulfillmentStatus)row.FulfillmentStatus, "menandai pesanan siap diambil", FulfillmentStatus.Dipesan);
            }
            else
            {
                TransaksiHelper.ValidateStatus((FulfillmentStatus)row.FulfillmentStatus, "menyelesaikan pesanan", FulfillmentStatus.Dipesan, FulfillmentStatus.SiapDiambil);
                if (row.PaymentStatus != (int)PaymentStatus.Lunas)
                {
                    throw new ArgumentException(ExceptionMessage.SALES_NOT_PAID);
                }
            }
            row.FulfillmentStatus = (int)target;
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        // Stok yang terpotong (mutasi Terjual pesanan ini) dikembalikan lewat mutasi Retur.
        private static void SetCancel(EF.ModelContext context, EF.TSalesOrder row, string? reason, int userId)
        {
            TransaksiHelper.ValidateStatus((FulfillmentStatus)row.FulfillmentStatus, "membatalkan pesanan", FulfillmentStatus.Dipesan, FulfillmentStatus.SiapDiambil);
            if (row.PaidAmount > 0)
            {
                throw new ArgumentException(ExceptionMessage.SALES_HAS_PAYMENT);
            }
            var now = DateTime.Now;
            var sold = context.TProductStockMovements.Where(x => x.OrderId == row.Id).ToList()
                .GroupBy(x => new { x.ProductId, x.VariantId })
                .Select(g => new { g.Key.ProductId, g.Key.VariantId, Quantity = g.Sum(x => x.Quantity) })
                .Where(x => x.Quantity < 0)
                .ToList();
            foreach (var line in sold)
            {
                var product = context.MProducts.Single(x => x.Id == line.ProductId);
                var variant = line.VariantId == null ? null : context.MProductVariants.Single(x => x.Id == line.VariantId);
                ProductStockMovement.Apply(context, product, variant, StockMovementType.Retur, -line.Quantity, now, row.Id, null,
                    "Batal pesanan " + row.OrderNumber, userId);
            }
            row.Notes = TransaksiHelper.AppendNote(row.Notes, "Batal", reason, NOTES_LENGTH);
            row.FulfillmentStatus = (int)FulfillmentStatus.Batal;
            row.UpdDate = now;
            row.UpdBy = userId;
        }
        private static decimal GetPaidTotal(EF.ModelContext context, int orderId, int? excludeTransactionId)
        {
            return context.TFinancialTransactions
                .Where(x => x.SalesOrderId == orderId && x.Status == (int)TransactionStatus.Posted
                    && x.TransactionType == (int)CashFlowType.Income && x.Id != excludeTransactionId)
                .Sum(x => x.Amount);
        }
        private static void SetPayment(EF.ModelContext context, EF.TSalesOrder row, int accountId, int categoryId, decimal amount, DateOnly paidDate, int userId)
        {
            TransaksiHelper.ValidateStatus((FulfillmentStatus)row.FulfillmentStatus, "menerima pembayaran pesanan",
                FulfillmentStatus.Dipesan, FulfillmentStatus.SiapDiambil, FulfillmentStatus.Selesai);
            TransaksiHelper.ValidatePositive(amount, "Nominal Pembayaran");
            var paid = GetPaidTotal(context, row.Id, null);
            if (amount > row.TotalAmount - paid)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.SALES_PAYMENT_EXCEEDS, row.TotalAmount - paid));
            }
            FinancialTransaction.CreateRow(context, new FinancialTransaction.NewRow
            {
                AccountId = accountId,
                CategoryId = categoryId,
                PeriodId = FinancialTransaction.OpenPeriodOrDefault(context, row.PeriodId),
                TransactionType = CashFlowType.Income,
                Amount = amount,
                TransactionDate = paidDate,
                Description = "Penjualan " + row.OrderNumber,
                ActivityId = row.ActivityId,
                SalesOrderId = row.Id
            }, null, true, userId);
            SetPaid(row, paid + amount, userId);
        }
        private static void SetCancelPayment(EF.ModelContext context, EF.TSalesOrder row, int transactionId, string? reason, int userId)
        {
            TransaksiHelper.ValidateStatus((FulfillmentStatus)row.FulfillmentStatus, "membatalkan pembayaran pesanan", FulfillmentStatus.Dipesan, FulfillmentStatus.SiapDiambil);
            var trx = context.TFinancialTransactions.SingleOrDefault(x => x.Id == transactionId && x.SalesOrderId == row.Id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Pembayaran Pesanan"));
            FinancialTransaction.VoidRow(context, trx, reason ?? "Pembayaran pesanan dibatalkan", userId);
            SetPaid(row, GetPaidTotal(context, row.Id, trx.Id), userId);
        }
        private static void SetPaid(EF.TSalesOrder row, decimal paid, int userId)
        {
            row.PaidAmount = paid;
            row.PaymentStatus = (int)GetPaymentStatus(row.TotalAmount, paid);
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
        }
        #endregion
    }
}
