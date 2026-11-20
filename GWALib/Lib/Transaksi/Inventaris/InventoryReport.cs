using GWALib.General;
using GWAShared.Enums;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;

namespace GWALib.Lib.Transaksi.Inventaris
{
    /// <summary>
    /// Data laporan inventaris (read-only): aset yang belum dihapuskan per kategori dengan jumlah per kondisi, yang sedang dipinjam
    /// dan dalam perawatan, plus riwayat perawatan dalam rentang tanggal (biaya, status).
    /// Aset per unit dihitung dari unit yang tidak dihapus (Hilang = status unit Hilang); aset cukup jumlah memakai kondisi aset untuk seluruh jumlahnya.
    /// </summary>
    public static class InventoryReport
    {
        public record AssetRow(string Category, string Code, string Name, string? Brand, string Unit, bool PerUnit, int Quantity,
            int Baik, int RusakRingan, int RusakBerat, int Hilang, int Borrowed, int InMaintenance, string? Location, AssetStatus Status,
            DateOnly? AcquisitionDate, decimal? Cost);
        public record MaintenanceRow(DateOnly Date, string AssetCode, string Asset, string? UnitCode, string Description, string? Vendor,
            decimal Cost, MaintenanceStatus Status, DateTime? FinishedAt);
        public record Inventory(DateOnly From, DateOnly To, string? Category, List<AssetRow> Assets, List<MaintenanceRow> Maintenance)
        {
            public int Quantity => Assets.Sum(x => x.Quantity);
            public int Baik => Assets.Sum(x => x.Baik);
            public int RusakRingan => Assets.Sum(x => x.RusakRingan);
            public int RusakBerat => Assets.Sum(x => x.RusakBerat);
            public int Hilang => Assets.Sum(x => x.Hilang);
            public decimal Cost => Assets.Sum(x => x.Cost ?? 0);
            public decimal MaintenanceCost => Maintenance.Sum(x => x.Cost);
        }

        private const int MAX_DAYS = 3 * 366;
        private static readonly int[] OutStatuses = { (int)BorrowRequestStatus.Borrowed, (int)BorrowRequestStatus.Overdue };

        public static Inventory GetInventory(DateOnly from, DateOnly to, int? categoryId = null)
        {
            if (to < from || to.DayNumber - from.DayNumber > MAX_DAYS)
            {
                throw new ArgumentException(ExceptionMessage.DATE_RANGE_INVALID);
            }
            using var context = DBClass.GetContext();
            string? category = null;
            if (categoryId > 0)
            {
                category = context.MAssetCategories.Where(x => x.Id == categoryId).Select(x => x.Name).SingleOrDefault()
                    ?? throw new ArgumentException(string.Format(ExceptionMessage.DATA_X_NOT_FOUND, "Kategori aset"));
            }

            var assets = context.MAssets.AsNoTracking()
                .Where(x => x.Status != (int)AssetStatus.Disposed && (categoryId == null || categoryId <= 0 || x.CategoryId == categoryId))
                .Select(x => new
                {
                    x.Id, Category = x.Category.Name, x.AssetCode, x.Name, x.Brand, x.Unit, x.TrackUnit, x.Quantity, x.AssetCondition,
                    x.Location, x.Status, x.AcquisitionDate, x.AcquisitionCost
                }).ToList();
            var ids = assets.Select(x => x.Id).ToList();
            var units = context.MAssetUnits.AsNoTracking()
                .Where(x => ids.Contains(x.AssetId) && x.Status != (int)AssetUnitStatus.Dihapus)
                .GroupBy(x => new { x.AssetId, x.Status, x.UnitCondition })
                .Select(g => new { g.Key.AssetId, g.Key.Status, g.Key.UnitCondition, Count = g.Count() }).ToList();
            var borrowed = context.TBorrowItems.AsNoTracking()
                .Where(x => ids.Contains(x.AssetId) && OutStatuses.Contains(x.BorrowRequest.Status))
                .GroupBy(x => x.AssetId).Select(g => new { AssetId = g.Key, Count = g.Sum(x => x.Quantity - x.ReturnedQuantity) }).ToList();

            var rows = new List<AssetRow>();
            foreach (var a in assets.OrderBy(x => x.Category).ThenBy(x => x.AssetCode))
            {
                if (a.TrackUnit == (int)UnitTracking.PerUnit)
                {
                    var u = units.Where(x => x.AssetId == a.Id).ToList();
                    var kept = u.Where(x => x.Status != (int)AssetUnitStatus.Hilang).ToList();
                    rows.Add(new AssetRow(a.Category, a.AssetCode, a.Name, a.Brand, a.Unit, true, u.Sum(x => x.Count),
                        kept.Where(x => x.UnitCondition == (int)ItemCondition.Baik).Sum(x => x.Count),
                        kept.Where(x => x.UnitCondition == (int)ItemCondition.RusakRingan).Sum(x => x.Count),
                        kept.Where(x => x.UnitCondition == (int)ItemCondition.RusakBerat).Sum(x => x.Count),
                        u.Where(x => x.Status == (int)AssetUnitStatus.Hilang).Sum(x => x.Count),
                        u.Where(x => x.Status == (int)AssetUnitStatus.Dipinjam).Sum(x => x.Count),
                        u.Where(x => x.Status == (int)AssetUnitStatus.Perawatan).Sum(x => x.Count),
                        a.Location, (AssetStatus)a.Status, a.AcquisitionDate, a.AcquisitionCost));
                }
                else
                {
                    int Of(ItemCondition c) => a.AssetCondition == (int)c ? a.Quantity : 0;
                    rows.Add(new AssetRow(a.Category, a.AssetCode, a.Name, a.Brand, a.Unit, false, a.Quantity,
                        Of(ItemCondition.Baik), Of(ItemCondition.RusakRingan), Of(ItemCondition.RusakBerat), Of(ItemCondition.Hilang),
                        borrowed.Where(x => x.AssetId == a.Id).Sum(x => x.Count),
                        a.Status == (int)AssetStatus.Maintenance ? a.Quantity : 0,
                        a.Location, (AssetStatus)a.Status, a.AcquisitionDate, a.AcquisitionCost));
                }
            }

            var maintenance = context.TMaintenanceLogs.AsNoTracking()
                .Where(x => x.MaintenanceDate >= from && x.MaintenanceDate <= to
                    && (categoryId == null || categoryId <= 0 || x.Asset.CategoryId == categoryId))
                .OrderBy(x => x.MaintenanceDate).ThenBy(x => x.Id)
                .Select(x => new MaintenanceRow(x.MaintenanceDate, x.Asset.AssetCode, x.Asset.Name, x.AssetUnit == null ? null : x.AssetUnit.UnitCode,
                    x.Description, x.Vendor, x.Cost, (MaintenanceStatus)x.Status, x.FinishedAt))
                .ToList();
            return new Inventory(from, to, category, rows, maintenance);
        }
    }
}
