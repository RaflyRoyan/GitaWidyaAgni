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

namespace GWALib.Lib.Transaksi.Pengadaan
{
    /// <summary>
    /// Baris penerimaan barang (t_procurement_receipt_item), unik per (penerimaan, item). Dibuat bersama ProcurementReceipt dan tidak diubah
    /// terpisah. Jumlah tidak boleh melebihi sisa item yang belum diterima. Item aset (bukan Hilang) bisa langsung dibuat m_asset lewat
    /// NewRow.Asset: kategori, tanggal perolehan dan biaya (harga aktual / estimasi x jumlah) diisi dari pengadaan bila kosong.
    /// </summary>
    public partial class ProcurementReceiptItem : BaseLib
    {
        public class NewRow
        {
            public int ItemId { get; set; }
            public int Quantity { get; set; }
            public ItemCondition ItemCondition { get; set; } = ItemCondition.Baik;
            public string? Notes { get; set; }
            // Opsional untuk item aset: data aset yang dibuat (Quantity, sumber dan kategori diisi otomatis).
            public Asset.NewRow? Asset { get; set; }
        }

        public int Id { get; set; }
        public int ReceiptId { get; private set; }
        public int ItemId { get; private set; }
        public string ItemName { get; private set; } = null!;
        public int Quantity { get; private set; }
        public ItemCondition ItemCondition { get; private set; }
        public string? Notes { get; private set; }

        private static IQueryable<EF.TProcurementReceiptItem> _Querys
        {
            get
            {
                return DBClass.GetContext().TProcurementReceiptItems.Include(x => x.Item).AsQueryable();
            }
        }
        public ProcurementReceiptItem()
        {
        }
        public ProcurementReceiptItem(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ProcurementReceiptItem>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ProcurementReceiptItem(EF.TProcurementReceiptItem row)
        {
            SetData(row);
        }
        private void SetData(EF.TProcurementReceiptItem row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ProcurementReceiptItem>();
            ReceiptId = row.ReceiptId;
            ItemId = row.ItemId;
            ItemName = row.Item?.ItemName ?? string.Empty;
            Quantity = row.Quantity;
            ItemCondition = (ItemCondition)row.ItemCondition;
            Notes = row.Notes;
        }
    }
}
