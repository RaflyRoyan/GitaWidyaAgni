using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Inventaris;
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
    /// Riwayat mutasi aset (t_asset_movement). Sebagian besar dibuat otomatis (aset / unit baru, pinjam, kembali, hilang, penghapusan).
    /// Save manual hanya untuk aset cukup jumlah: In (menambah Quantity), Out (mengurangi, tidak melebihi yang tersedia), Adjustment
    /// (jumlah bertanda; Quantity tidak boleh di bawah yang sedang dipinjam). Aset per unit memakai AssetUnit. Tidak ada ubah / hapus.
    /// </summary>
    public partial class AssetMovement : BaseLib
    {
        public class NewRow
        {
            public int AssetId { get; set; }
            public AssetMovementType MovementType { get; set; } = AssetMovementType.In;
            // In / Out: positif; Adjustment: bertanda.
            public int Quantity { get; set; }
            public string? Notes { get; set; }
        }

        public int Id { get; set; }
        public int AssetId { get; private set; }
        public string AssetName { get; private set; } = null!;
        public int? AssetUnitId { get; private set; }
        public string? UnitCode { get; private set; }
        public AssetMovementType MovementType { get; private set; }
        public int Quantity { get; private set; }
        public DateTime MovementDate { get; private set; }
        public string? ReferenceType { get; private set; }
        public int? ReferenceId { get; private set; }
        public string? Notes { get; private set; }
        public int CreatedBy { get; private set; }

        private static IQueryable<EF.TAssetMovement> _Querys
        {
            get
            {
                return DBClass.GetContext().TAssetMovements.Include(x => x.Asset).Include(x => x.AssetUnit).AsQueryable();
            }
        }
        public AssetMovement()
        {
        }
        public AssetMovement(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<AssetMovement>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal AssetMovement(EF.TAssetMovement row)
        {
            SetData(row);
        }
        private void SetData(EF.TAssetMovement row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<AssetMovement>();
            AssetId = row.AssetId;
            AssetName = row.Asset?.Name ?? string.Empty;
            AssetUnitId = row.AssetUnitId;
            UnitCode = row.AssetUnit?.UnitCode;
            MovementType = (AssetMovementType)row.MovementType;
            Quantity = row.Quantity;
            MovementDate = row.MovementDate;
            ReferenceType = row.ReferenceType;
            ReferenceId = row.ReferenceId;
            Notes = row.Notes;
            CreatedBy = row.CreatedBy;
        }
    }
}
