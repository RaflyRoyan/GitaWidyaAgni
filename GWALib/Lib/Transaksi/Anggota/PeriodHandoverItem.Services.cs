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

namespace GWALib.Lib.Transaksi.Anggota
{
    public partial class PeriodHandoverItem
    {
        #region SYNC
        public static List<PeriodHandoverItem> GetDataList(int handoverId)
        {
            return FilterQuery(handoverId).ToList().MappingData<PeriodHandoverItem, EF.TPeriodHandoverItem>();
        }
        public static Response<PeriodHandoverItem> Save(NewRow newrow, string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TPeriodHandoverItems.Add(row);
                context.SaveChanges();
                return new PeriodHandoverItem(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<PeriodHandoverItem>(msg));
        }
        public Response<PeriodHandoverItem> Update(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                SetRow(context, row, userId);
                context.SaveChanges();
                return new PeriodHandoverItem(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodHandoverItem>(msg));
        }
        public Response<PeriodHandoverItem> Delete(string user)
        {
            return ServiceHandler.Handle(() =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = GetRow(context, Id);
                PeriodHandover.GetEditableHandover(context, row.HandoverId);
                context.TPeriodHandoverItems.Remove(row);
                context.SaveChanges();
                return new PeriodHandoverItem(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<PeriodHandoverItem>(msg));
        }
        #endregion

        #region ASYNC
        public static async Task<List<PeriodHandoverItem>> GetDataListAsync(int handoverId)
        {
            return await (await FilterQuery(handoverId).ToListAsync()).MappingDataAsync<PeriodHandoverItem, EF.TPeriodHandoverItem>();
        }
        public static async Task<Response<PeriodHandoverItem>> SaveAsync(NewRow newrow, string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = CreateRow(context, newrow, userId);
                context.TPeriodHandoverItems.Add(row);
                await context.SaveChangesAsync();
                return new PeriodHandoverItem(row);
            }, data => ResponseFactory.DataSaved(data), msg => ResponseFactory.DataSaveFailed<PeriodHandoverItem>(msg));
        }
        public async Task<Response<PeriodHandoverItem>> UpdateAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                var userId = ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                SetRow(context, row, userId);
                await context.SaveChangesAsync();
                return new PeriodHandoverItem(row);
            }, data => ResponseFactory.DataUpdated(data), msg => ResponseFactory.DataUpdateFailed<PeriodHandoverItem>(msg));
        }
        public async Task<Response<PeriodHandoverItem>> DeleteAsync(string user)
        {
            return await ServiceHandler.HandleAsync(async () =>
            {
                ValidateUtility.ValidateUser(user);
                using var context = DBClass.GetContext();
                var row = await GetRowAsync(context, Id);
                PeriodHandover.GetEditableHandover(context, row.HandoverId);
                context.TPeriodHandoverItems.Remove(row);
                await context.SaveChangesAsync();
                return new PeriodHandoverItem(row);
            }, data => ResponseFactory.DataDeleted(data), msg => ResponseFactory.DataDeleteFailed<PeriodHandoverItem>(msg));
        }
        #endregion

        #region HELPER
        private static IQueryable<EF.TPeriodHandoverItem> FilterQuery(int handoverId)
        {
            return _Querys.Where(x => x.HandoverId == handoverId).OrderBy(x => x.ItemType).ThenBy(x => x.Id);
        }
        private static EF.TPeriodHandoverItem GetRow(EF.ModelContext context, int id)
        {
            return context.TPeriodHandoverItems.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Item Serah Terima"));
        }
        private static async Task<EF.TPeriodHandoverItem> GetRowAsync(EF.ModelContext context, int id)
        {
            return await context.TPeriodHandoverItems.SingleOrDefaultAsync(x => x.Id == id)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Item Serah Terima"));
        }
        // RefId harus menunjuk data yang ada sesuai ItemType; Lainnya tidak memakai RefId.
        private static int? ValidateRef(EF.ModelContext context, HandoverItemType itemType, int? refId)
        {
            if (refId == null || refId <= 0 || itemType == HandoverItemType.Lainnya)
            {
                return null;
            }
            var id = refId.Value;
            var exists = itemType switch
            {
                HandoverItemType.Saldo => context.MFinancialAccounts.Any(x => x.Id == id),
                HandoverItemType.Aset => context.MAssets.Any(x => x.Id == id),
                HandoverItemType.Program => context.TPrograms.Any(x => x.Id == id),
                HandoverItemType.Piutang => context.TDuesBills.Any(x => x.Id == id),
                HandoverItemType.Hutang => context.TDebts.Any(x => x.Id == id),
                HandoverItemType.Pinjaman => context.TBorrowRequests.Any(x => x.Id == id),
                HandoverItemType.Dokumen => context.TDocuments.Any(x => x.Id == id),
                HandoverItemType.MouAktif => context.TSponsorshipMous.Any(x => x.Id == id),
                HandoverItemType.KewajibanSponsor => context.TSponsorshipBenefits.Any(x => x.Id == id),
                HandoverItemType.SponsorBerjalan => context.TSponsorships.Any(x => x.Id == id),
                HandoverItemType.PengadaanBerjalan => context.TProcurementRequests.Any(x => x.Id == id),
                _ => false
            };
            if (!exists)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Referensi " + itemType.GetDescription()));
            }
            return id;
        }
        private static void ValidateData(HandoverItemType itemType, string description, int? quantity, ItemCondition? itemCondition)
        {
            if (!Enum.IsDefined(itemType))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jenis Item"));
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_REQUIRED, "Keterangan Item"));
            }
            if (quantity != null && quantity < 0)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Jumlah"));
            }
            if (itemCondition != null && !Enum.IsDefined(itemCondition.Value))
            {
                throw new ArgumentException(string.Format(ExceptionMessage.FIELD_INVALID, "Kondisi"));
            }
        }
        // Tanpa SaveChanges; dipakai juga PeriodHandover.Save untuk membuat item bersama berita acaranya.
        internal static EF.TPeriodHandoverItem CreateRow(EF.ModelContext context, NewRow newrow, int userId)
        {
            PeriodHandover.GetEditableHandover(context, newrow.HandoverId);
            var description = newrow.Description?.Trim() ?? string.Empty;
            ValidateData(newrow.ItemType, description, newrow.Quantity, newrow.ItemCondition);
            var refId = ValidateRef(context, newrow.ItemType, newrow.RefId);

            var row = new EF.TPeriodHandoverItem
            {
                Id = SequenceUtility.GetNextId<EF.TPeriodHandoverItem>(context),
                HandoverId = newrow.HandoverId,
                ItemType = (int)newrow.ItemType,
                RefId = refId,
                Description = description,
                Amount = newrow.Amount,
                Quantity = newrow.Quantity,
                ItemCondition = newrow.ItemCondition != null ? (int)newrow.ItemCondition : null,
                Notes = string.IsNullOrWhiteSpace(newrow.Notes) ? null : newrow.Notes.Trim(),
                InsDate = DateTime.Now,
                InsBy = userId
            };
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
            return row;
        }
        private void SetRow(EF.ModelContext context, EF.TPeriodHandoverItem row, int userId)
        {
            PeriodHandover.GetEditableHandover(context, row.HandoverId);
            var description = Description?.Trim() ?? string.Empty;
            ValidateData(ItemType, description, Quantity, ItemCondition);
            var refId = ValidateRef(context, ItemType, RefId);

            row.ItemType = (int)ItemType;
            row.RefId = refId;
            row.Description = description;
            row.Amount = Amount;
            row.Quantity = Quantity;
            row.ItemCondition = ItemCondition != null ? (int)ItemCondition : null;
            row.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim();
            row.UpdDate = DateTime.Now;
            row.UpdBy = userId;
            GWAShared.Utilities.ValidateUtility.ValidateDAModel(row);
        }
        #endregion
    }
}
