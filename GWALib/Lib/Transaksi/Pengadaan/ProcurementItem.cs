using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Pengadaan
{
    /// <summary>
    /// Item usulan pengadaan (t_procurement_item). Item aset (IsAsset = Aset) wajib punya kategori aset; vendor (opsional) harus mitra aktif.
    /// Tambah / ubah / hapus hanya saat usulan Draft / Revisi dan menghitung ulang TotalEstimate.
    /// Harga aktual dan vendor bisa diisi setelah disetujui (SetActual). ReceivedQuantity hanya diubah oleh penerimaan.
    /// </summary>
    public partial class ProcurementItem : BaseLib
    {
        public class NewRow
        {
            public int RequestId { get; set; }
            public string ItemName { get; set; } = null!;
            public string? Specification { get; set; }
            public ProcurementItemKind IsAsset { get; set; } = ProcurementItemKind.HabisPakai;
            public int? AssetCategoryId { get; set; }
            public int Quantity { get; set; } = 1;
            public string Unit { get; set; } = "pcs";
            public decimal EstimatedUnitPrice { get; set; }
            public int? ExternalPartyId { get; set; }
            public string? Notes { get; set; }
        }

        public int Id { get; set; }
        public int RequestId { get; private set; }
        public string ItemName { get; set; } = null!;
        public string? Specification { get; set; }
        public ProcurementItemKind IsAsset { get; set; }
        public int? AssetCategoryId { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; } = null!;
        public decimal EstimatedUnitPrice { get; set; }
        public decimal EstimatedAmount => Quantity * EstimatedUnitPrice;
        // Hanya-baca: diisi lewat SetActual.
        public decimal? ActualUnitPrice { get; private set; }
        public int? ExternalPartyId { get; set; }
        public int ReceivedQuantity { get; private set; }
        public string? Notes { get; set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TProcurementItem> _Querys
        {
            get
            {
                return DBClass.GetContext().TProcurementItems.AsQueryable();
            }
        }
        public ProcurementItem()
        {
        }
        public ProcurementItem(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ProcurementItem>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ProcurementItem(EF.TProcurementItem row)
        {
            SetData(row);
        }
        private void SetData(EF.TProcurementItem row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ProcurementItem>();
            RequestId = row.RequestId;
            ItemName = row.ItemName;
            Specification = row.Specification;
            IsAsset = (ProcurementItemKind)row.IsAsset;
            AssetCategoryId = row.AssetCategoryId;
            Quantity = row.Quantity;
            Unit = row.Unit;
            EstimatedUnitPrice = row.EstimatedUnitPrice;
            ActualUnitPrice = row.ActualUnitPrice;
            ExternalPartyId = row.ExternalPartyId;
            ReceivedQuantity = row.ReceivedQuantity;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
