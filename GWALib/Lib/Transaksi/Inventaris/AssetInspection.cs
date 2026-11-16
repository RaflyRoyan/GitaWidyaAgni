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
    /// Pemeriksaan kondisi aset / unit (t_asset_inspection). Inspeksi manual (Save) memperbarui kondisi: aset per unit wajib menyebut unit
    /// (kondisi unit; Hilang menjadikan unit Hilang), aset cukup jumlah memperbarui kondisi aset. Inspeksi pengembalian dibuat otomatis
    /// (BorrowItemId terisi) dan hanya mencatat. Hanya inspeksi manual yang bisa dihapus. Foto (maksimal 5 MB) tidak dimuat di daftar; GetPhoto mengembalikan null bila tidak ada.
    /// </summary>
    public partial class AssetInspection : BaseLib
    {
        public class NewRow
        {
            public int AssetId { get; set; }
            // Wajib untuk aset per unit.
            public int? AssetUnitId { get; set; }
            public ItemCondition ItemCondition { get; set; } = ItemCondition.Baik;
            // Kosong = sekarang.
            public DateTime? InspectedAt { get; set; }
            public string? Notes { get; set; }
            public byte[]? Photo { get; set; }
        }

        public int Id { get; set; }
        public int AssetId { get; private set; }
        public string AssetName { get; private set; } = null!;
        public int? AssetUnitId { get; private set; }
        public int? BorrowItemId { get; private set; }
        public int InspectedBy { get; private set; }
        public DateTime InspectedAt { get; private set; }
        public ItemCondition ItemCondition { get; private set; }
        public string? Notes { get; private set; }

        // Kolom foto (blob) tidak ikut di-select.
        private static IQueryable<EF.TAssetInspection> _Querys
        {
            get
            {
                return DBClass.GetContext().TAssetInspections.Select(x => new EF.TAssetInspection
                {
                    Id = x.Id,
                    AssetId = x.AssetId,
                    Asset = x.Asset,
                    AssetUnitId = x.AssetUnitId,
                    BorrowItemId = x.BorrowItemId,
                    InspectedBy = x.InspectedBy,
                    InspectedAt = x.InspectedAt,
                    ItemCondition = x.ItemCondition,
                    Notes = x.Notes
                });
            }
        }
        public AssetInspection()
        {
        }
        public AssetInspection(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<AssetInspection>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal AssetInspection(EF.TAssetInspection row)
        {
            SetData(row);
        }
        private void SetData(EF.TAssetInspection row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<AssetInspection>();
            AssetId = row.AssetId;
            AssetName = row.Asset?.Name ?? string.Empty;
            AssetUnitId = row.AssetUnitId;
            BorrowItemId = row.BorrowItemId;
            InspectedBy = row.InspectedBy;
            InspectedAt = row.InspectedAt;
            ItemCondition = (ItemCondition)row.ItemCondition;
            Notes = row.Notes;
        }
    }
}
