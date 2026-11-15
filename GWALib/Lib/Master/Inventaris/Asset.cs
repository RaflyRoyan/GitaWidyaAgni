using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Inventaris
{
    /// <summary>
    /// Aset / inventaris organisasi (m_asset). Kode aset unik.
    /// Quantity hanya diisi saat dibuat (dicatat sebagai mutasi Masuk); Status Dipinjam / Perawatan diatur modul transaksi;
    /// Delete = dihapuskan (Disposed). ProcurementItemId / SponsorshipId diisi oleh modul pengadaan / sponsorship.
    /// TrackUnit PerUnit: setiap unit fisik punya kode / QR di m_asset_unit (lihat <see cref="AssetUnit"/>) dan Quantity = jumlah unit
    /// yang belum dihapus. TrackUnit dan UnitPrefix ditentukan saat dibuat dan tidak bisa diubah.
    /// </summary>
    public partial class Asset : BaseLib
    {
        public class NewRow
        {
            public int CategoryId { get; set; }
            public string AssetCode { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string? Brand { get; set; }
            public string? Specification { get; set; }
            public int Quantity { get; set; } = 1;
            public string Unit { get; set; } = "unit";
            public UnitTracking TrackUnit { get; set; } = UnitTracking.CukupJumlah;
            // Wajib untuk PerUnit, mis. HT -> kode unit HT001, HT002, ...
            public string? UnitPrefix { get; set; }
            public DateOnly? AcquisitionDate { get; set; }
            public decimal? AcquisitionCost { get; set; }
            public AcquisitionSource AcquisitionSource { get; set; } = AcquisitionSource.Beli;
            public string? Location { get; set; }
            public ItemCondition AssetCondition { get; set; } = ItemCondition.Baik;
            public decimal RentalFeePerDay { get; set; }
            public decimal LateFeePerDay { get; set; }
        }

        public int Id { get; set; }
        public AssetCategory Category { get; set; } = null!;
        public string AssetCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Brand { get; set; }
        public string? Specification { get; set; }
        // Hanya-baca: diubah lewat mutasi aset.
        public int Quantity { get; private set; }
        public string Unit { get; set; } = null!;
        // Hanya-baca: ditentukan saat aset dibuat.
        public UnitTracking TrackUnit { get; private set; }
        public string? UnitPrefix { get; private set; }
        public DateOnly? AcquisitionDate { get; set; }
        public decimal? AcquisitionCost { get; set; }
        public AcquisitionSource AcquisitionSource { get; set; }
        public int? ProcurementItemId { get; private set; }
        public int? SponsorshipId { get; private set; }
        public string? Location { get; set; }
        public ItemCondition AssetCondition { get; set; }
        // Tarif sewa / denda telat per hari per unit; 0 = gratis / tanpa denda.
        public decimal RentalFeePerDay { get; set; }
        public decimal LateFeePerDay { get; set; }
        // Hanya-baca: diatur peminjaman / perawatan / penghapusan.
        public AssetStatus Status { get; private set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private List<AssetPhoto.Info>? PhotoList;
        private List<AssetUnit>? UnitList;

        EF.ModelContext _context = null!;
        private IQueryable<EF.MAsset> _Query
        {
            get
            {
                return _context.MAssets
                    .Include(x => x.Category)
                    .AsQueryable();
            }
        }
        private static IQueryable<EF.MAsset> _Querys
        {
            get
            {
                return DBClass.GetContext().MAssets
                    .Include(x => x.Category)
                    .AsQueryable();
            }
        }
        public Asset()
        {
            _context = DBClass.GetContext();
        }
        public Asset(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<Asset>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Asset(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Asset(EF.MAsset row)
        {
            SetData(row);
        }
        private void SetData(EF.MAsset row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Asset>();
            Category = row.Category != null ? new AssetCategory(row.Category) : new AssetCategory(row.CategoryId);
            AssetCode = row.AssetCode;
            Name = row.Name;
            Brand = row.Brand;
            Specification = row.Specification;
            Quantity = row.Quantity;
            Unit = row.Unit;
            TrackUnit = (UnitTracking)row.TrackUnit;
            UnitPrefix = row.UnitPrefix;
            AcquisitionDate = row.AcquisitionDate;
            AcquisitionCost = row.AcquisitionCost;
            AcquisitionSource = (AcquisitionSource)row.AcquisitionSource;
            ProcurementItemId = row.ProcurementItemId;
            SponsorshipId = row.SponsorshipId;
            Location = row.Location;
            AssetCondition = (ItemCondition)row.AssetCondition;
            RentalFeePerDay = row.RentalFeePerDay;
            LateFeePerDay = row.LateFeePerDay;
            Status = (AssetStatus)row.Status;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
