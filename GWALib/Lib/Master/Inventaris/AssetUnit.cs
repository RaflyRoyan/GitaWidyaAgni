using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Inventaris
{
    /// <summary>
    /// Unit fisik aset yang dicatat per unit (m_asset_unit), dengan kode inventaris unik = UnitPrefix aset + nomor urut (mis. HT001)
    /// yang dicetak sebagai QR. Status Dipinjam / Perawatan / Hilang diatur modul transaksi; Delete = Dihapus.
    /// Menambah / menghapus unit ikut mengubah Quantity aset dan dicatat di t_asset_movement.
    /// </summary>
    public partial class AssetUnit : BaseLib
    {
        public class NewRow
        {
            public int AssetId { get; set; }
            public string? SerialNumber { get; set; }
            public ItemCondition UnitCondition { get; set; } = ItemCondition.Baik;
            public string? Notes { get; set; }
        }

        public int Id { get; set; }
        public int AssetId { get; private set; }
        // Hanya-baca: dibuat otomatis.
        public string UnitCode { get; private set; } = null!;
        public string? SerialNumber { get; set; }
        public ItemCondition UnitCondition { get; set; }
        // Hanya-baca: diatur peminjaman / perawatan / penghapusan.
        public AssetUnitStatus Status { get; private set; }
        public DateTime? LabelPrintedAt { get; private set; }
        public string? Notes { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private Asset? AssetData;

        EF.ModelContext _context = null!;
        private IQueryable<EF.MAssetUnit> _Query
        {
            get
            {
                return _context.MAssetUnits.AsQueryable();
            }
        }
        private static IQueryable<EF.MAssetUnit> _Querys
        {
            get
            {
                return DBClass.GetContext().MAssetUnits.AsQueryable();
            }
        }
        public AssetUnit()
        {
            _context = DBClass.GetContext();
        }
        public AssetUnit(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<AssetUnit>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal AssetUnit(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal AssetUnit(EF.MAssetUnit row)
        {
            SetData(row);
        }
        private void SetData(EF.MAssetUnit row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<AssetUnit>();
            AssetId = row.AssetId;
            UnitCode = row.UnitCode;
            SerialNumber = row.SerialNumber;
            UnitCondition = (ItemCondition)row.UnitCondition;
            Status = (AssetUnitStatus)row.Status;
            LabelPrintedAt = row.LabelPrintedAt;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
