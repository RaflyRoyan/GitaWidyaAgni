using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Inventaris
{
    /// <summary>
    /// Aset yang dipinjam (t_borrow_item), unik per (peminjaman, aset). Tambah / ubah jumlah / hapus hanya saat peminjaman Pending.
    /// Tarif sewa / denda disalin dari aset; RentalFee = hari sewa x tarif x jumlah (hanya skema TarifTetap).
    /// Pengembalian aset cukup jumlah lewat ReturnQuantity (boleh bertahap, termasuk yang hilang); aset per unit lewat BorrowItemUnit.ReturnUnit.
    /// Denda: jumlah x hari terlambat x tarif denda (per unit: jumlah denda unit).
    /// </summary>
    public partial class BorrowItem : BaseLib
    {
        public class NewRow
        {
            public int BorrowRequestId { get; set; }
            public int AssetId { get; set; }
            public int Quantity { get; set; } = 1;
        }

        public int Id { get; set; }
        public int BorrowRequestId { get; private set; }
        public int AssetId { get; private set; }
        public string AssetName { get; private set; } = null!;
        public UnitTracking TrackUnit { get; private set; }
        public int Quantity { get; set; }
        public int ReturnedQuantity { get; private set; }
        public ItemCondition? ConditionOnBorrow { get; private set; }
        public ItemCondition? ConditionOnReturn { get; private set; }
        public DateTime? ReturnedAt { get; private set; }
        public decimal RentalFeePerDay { get; private set; }
        public int RentalDays { get; private set; }
        public decimal RentalFee { get; private set; }
        public decimal LateFeePerDay { get; private set; }
        public int LateDays { get; private set; }
        public decimal LateFee { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private List<BorrowItemUnit>? UnitList;

        private static IQueryable<EF.TBorrowItem> _Querys
        {
            get
            {
                return DBClass.GetContext().TBorrowItems.Include(x => x.Asset).AsQueryable();
            }
        }
        public BorrowItem()
        {
        }
        public BorrowItem(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<BorrowItem>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal BorrowItem(EF.TBorrowItem row)
        {
            SetData(row);
        }
        private void SetData(EF.TBorrowItem row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<BorrowItem>();
            BorrowRequestId = row.BorrowRequestId;
            AssetId = row.AssetId;
            AssetName = row.Asset?.Name ?? string.Empty;
            TrackUnit = (UnitTracking)(row.Asset?.TrackUnit ?? 0);
            Quantity = row.Quantity;
            ReturnedQuantity = row.ReturnedQuantity;
            ConditionOnBorrow = (ItemCondition?)row.ConditionOnBorrow;
            ConditionOnReturn = (ItemCondition?)row.ConditionOnReturn;
            ReturnedAt = row.ReturnedAt;
            RentalFeePerDay = row.RentalFeePerDay;
            RentalDays = row.RentalDays;
            RentalFee = row.RentalFee;
            LateFeePerDay = row.LateFeePerDay;
            LateDays = row.LateDays;
            LateFee = row.LateFee;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
