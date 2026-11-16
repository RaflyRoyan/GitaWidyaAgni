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

namespace GWALib.Lib.Transaksi.Inventaris
{
    /// <summary>
    /// Unit aset yang diambil / dikembalikan per peminjaman (t_borrow_item_unit), unik per (item, unit). Dibuat saat BorrowRequest.Checkout
    /// (unit harus Tersedia; status unit Dipinjam, mutasi Pinjam per unit). ReturnUnit memindai kode QR: Kembali / KembaliRusak / Hilang,
    /// unit kembali Tersedia (atau Hilang), kondisi unit diperbarui, denda unit = hari terlambat x tarif denda item, dan inspeksi dicatat.
    /// </summary>
    public partial class BorrowItemUnit : BaseLib
    {
        public int Id { get; set; }
        public int BorrowItemId { get; private set; }
        public int AssetUnitId { get; private set; }
        public string UnitCode { get; private set; } = null!;
        public DateTime TakenAt { get; private set; }
        public ItemCondition ConditionOut { get; private set; }
        public DateTime? ReturnedAt { get; private set; }
        public ItemCondition? ConditionIn { get; private set; }
        public UnitReturnStatus ReturnStatus { get; private set; }
        public int LateDays { get; private set; }
        public decimal LateFee { get; private set; }
        public string? Notes { get; private set; }

        private static IQueryable<EF.TBorrowItemUnit> _Querys
        {
            get
            {
                return DBClass.GetContext().TBorrowItemUnits.Include(x => x.AssetUnit).AsQueryable();
            }
        }
        public BorrowItemUnit()
        {
        }
        public BorrowItemUnit(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<BorrowItemUnit>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal BorrowItemUnit(EF.TBorrowItemUnit row)
        {
            SetData(row);
        }
        private void SetData(EF.TBorrowItemUnit row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<BorrowItemUnit>();
            BorrowItemId = row.BorrowItemId;
            AssetUnitId = row.AssetUnitId;
            UnitCode = row.AssetUnit?.UnitCode ?? string.Empty;
            TakenAt = row.TakenAt;
            ConditionOut = (ItemCondition)row.ConditionOut;
            ReturnedAt = row.ReturnedAt;
            ConditionIn = (ItemCondition?)row.ConditionIn;
            ReturnStatus = (UnitReturnStatus)row.ReturnStatus;
            LateDays = row.LateDays;
            LateFee = row.LateFee;
            Notes = row.Notes;
        }
    }
}
