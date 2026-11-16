using GWALib.General;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Pengadaan
{
    /// <summary>
    /// Penerimaan barang pengadaan (t_procurement_receipt) beserta barisnya (ProcurementReceiptItem), disimpan dalam satu transaksi DB.
    /// Hanya saat usulan Dibeli / DiterimaSebagian; status usulan dihitung ulang (DiterimaSebagian / Selesai) dari jumlah diterima.
    /// Penerima kosong = anggota milik user login. Foto (maksimal 5 MB) tidak dimuat di daftar (GetPhoto / UpdatePhoto).
    /// Hapus mengurangi jumlah diterima, ditolak bila salah satu item-nya sudah dibuat aset.
    /// </summary>
    public partial class ProcurementReceipt : BaseLib
    {
        public class NewRow
        {
            public int RequestId { get; set; }
            public DateOnly ReceiptDate { get; set; }
            // Kosong = anggota milik user login.
            public int? ReceivedByMemberId { get; set; }
            public string? Notes { get; set; }
            public byte[]? Photo { get; set; }
            public string? PhotoFileName { get; set; }
            public string? PhotoContentType { get; set; }
            public List<ProcurementReceiptItem.NewRow> Items { get; set; } = new();
        }
        public record FileContent(byte[] Data, string FileName, string? ContentType);

        public int Id { get; set; }
        public int RequestId { get; private set; }
        public DateOnly ReceiptDate { get; set; }
        public int ReceivedByMemberId { get; set; }
        public string ReceivedByName { get; private set; } = null!;
        public string? Notes { get; set; }
        // Terisi = foto ada.
        public string? PhotoFileName { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private List<ProcurementReceiptItem>? ItemList;

        // Kolom foto (blob) tidak ikut di-select.
        private static IQueryable<EF.TProcurementReceipt> _Querys
        {
            get
            {
                return DBClass.GetContext().TProcurementReceipts.Select(x => new EF.TProcurementReceipt
                {
                    Id = x.Id,
                    RequestId = x.RequestId,
                    ReceiptDate = x.ReceiptDate,
                    ReceivedByMemberId = x.ReceivedByMemberId,
                    ReceivedByMember = x.ReceivedByMember,
                    Notes = x.Notes,
                    PhotoFileName = x.PhotoFileName,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy,
                    UpdDate = x.UpdDate,
                    UpdBy = x.UpdBy
                });
            }
        }
        public ProcurementReceipt()
        {
        }
        public ProcurementReceipt(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ProcurementReceipt>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ProcurementReceipt(EF.TProcurementReceipt row)
        {
            SetData(row);
        }
        private void SetData(EF.TProcurementReceipt row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ProcurementReceipt>();
            RequestId = row.RequestId;
            ReceiptDate = row.ReceiptDate;
            ReceivedByMemberId = row.ReceivedByMemberId;
            ReceivedByName = row.ReceivedByMember?.Fullname ?? string.Empty;
            Notes = row.Notes;
            PhotoFileName = row.PhotoFileName;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
