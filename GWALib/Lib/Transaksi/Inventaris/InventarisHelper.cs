using GWAShared.Enums;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Inventaris
{
    /// <summary>
    /// Pembantu status dan ketersediaan aset untuk peminjaman / perawatan. Perhitungan memakai data yang sudah di-track context (Local),
    /// sehingga perubahan dalam command yang sama (mis. status peminjaman baru Borrowed) ikut dihitung.
    /// </summary>
    internal static class InventarisHelper
    {
        internal const string REF_BORROW = "BORROW_REQUEST";
        internal const string REF_MAINTENANCE = "MAINTENANCE";
        internal const string REF_MANUAL = "MANUAL";

        private static readonly int[] OutStatuses = { (int)BorrowRequestStatus.Borrowed, (int)BorrowRequestStatus.Overdue };

        // Jumlah aset (cukup jumlah) yang sedang di luar: peminjaman Borrowed / Overdue, quantity - returned_quantity.
        internal static int GetOutstanding(EF.ModelContext context, int assetId)
        {
            context.TBorrowItems.Include(x => x.BorrowRequest)
                .Where(x => x.AssetId == assetId && (x.BorrowRequest.Status == (int)BorrowRequestStatus.Approved || OutStatuses.Contains(x.BorrowRequest.Status)))
                .Load();
            return context.TBorrowItems.Local
                .Where(x => x.AssetId == assetId && x.BorrowRequest != null && OutStatuses.Contains(x.BorrowRequest.Status))
                .Sum(x => x.Quantity - x.ReturnedQuantity);
        }
        internal static int GetAvailableQuantity(EF.ModelContext context, EF.MAsset asset)
        {
            return asset.Quantity - GetOutstanding(context, asset.Id);
        }
        // Status aset dari unit / jumlah yang tersedia; Disposed dan Maintenance tidak diubah di sini.
        internal static void RefreshAssetStatus(EF.ModelContext context, EF.MAsset asset, int userId)
        {
            if (asset.Status == (int)AssetStatus.Disposed || asset.Status == (int)AssetStatus.Maintenance)
            {
                return;
            }
            bool borrowedOut;
            if (asset.TrackUnit == (int)UnitTracking.PerUnit)
            {
                context.MAssetUnits.Where(x => x.AssetId == asset.Id).Load();
                var units = context.MAssetUnits.Local.Where(x => x.AssetId == asset.Id).ToList();
                borrowedOut = units.Any(x => x.Status == (int)AssetUnitStatus.Dipinjam) && !units.Any(x => x.Status == (int)AssetUnitStatus.Tersedia);
            }
            else
            {
                borrowedOut = GetOutstanding(context, asset.Id) > 0 && GetAvailableQuantity(context, asset) <= 0;
            }
            var status = (int)(borrowedOut ? AssetStatus.Borrowed : AssetStatus.Available);
            if (asset.Status != status)
            {
                asset.Status = status;
                asset.UpdDate = DateTime.Now;
                asset.UpdBy = userId;
            }
        }
        internal static EF.TAssetMovement AddMovement(EF.ModelContext context, int assetId, int? assetUnitId, AssetMovementType movementType, int quantity,
            string referenceType, int? referenceId, string? notes, int userId)
        {
            var movement = GWALib.Lib.Master.Inventaris.Asset.CreateMovement(context, assetId, assetUnitId, movementType, quantity, notes ?? string.Empty, userId);
            movement.ReferenceType = referenceType;
            movement.ReferenceId = referenceId;
            movement.Notes = TransaksiHelper.Truncate(notes, 255);
            context.TAssetMovements.Add(movement);
            return movement;
        }
        // Unit berdasarkan kode QR (huruf besar), harus milik aset yang disebut.
        internal static EF.MAssetUnit GetUnitByCode(EF.ModelContext context, string? unitCode, EF.MAsset asset)
        {
            var code = unitCode?.Trim().ToUpperInvariant() ?? string.Empty;
            var unit = context.MAssetUnits.SingleOrDefault(x => x.UnitCode == code)
                ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Unit Aset " + code));
            if (unit.AssetId != asset.Id)
            {
                throw new ArgumentException(string.Format(ExceptionMessage.ASSET_UNIT_MISMATCH, code, asset.Name));
            }
            return unit;
        }
        // Hari terlambat = MAX(0, CEIL(hari returnedAt - expectedReturn)).
        internal static int GetLateDays(DateTime returnedAt, DateTime expectedReturn)
        {
            var days = (returnedAt - expectedReturn).TotalDays;
            return days <= 0 ? 0 : (int)Math.Ceiling(days);
        }
    }
}
