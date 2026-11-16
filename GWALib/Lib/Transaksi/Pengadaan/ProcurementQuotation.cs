using GWALib.General;
using GWAShared.Enums;
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
    /// Penawaran vendor untuk pengadaan (t_procurement_quotation). ItemId kosong = penawaran untuk seluruh usulan. Vendor: mitra aktif
    /// (ExternalPartyId) atau nama vendor bila belum terdaftar. Bisa diubah saat usulan Draft / Diajukan / Revisi / Disetujui.
    /// Select memilih satu penawaran per cakupan (item yang sama, atau seluruh usulan); penawaran item dengan mitra mengisi vendor item.
    /// File penawaran (maksimal 5 MB) tidak dimuat di daftar (GetFile / UpdateFile).
    /// </summary>
    public partial class ProcurementQuotation : BaseLib
    {
        public class NewRow
        {
            public int RequestId { get; set; }
            public int? ItemId { get; set; }
            public int? ExternalPartyId { get; set; }
            public string? VendorName { get; set; }
            public decimal QuotedAmount { get; set; }
            public string? Notes { get; set; }
            // Opsional: file penawaran.
            public byte[]? File { get; set; }
            public string? FileName { get; set; }
            public string? ContentType { get; set; }
        }
        public record FileContent(byte[] Data, string FileName, string? ContentType);

        public int Id { get; set; }
        public int RequestId { get; private set; }
        public int? ItemId { get; set; }
        public int? ExternalPartyId { get; set; }
        public string? ExternalPartyName { get; private set; }
        public string? VendorName { get; set; }
        public decimal QuotedAmount { get; set; }
        // Hanya-baca: diubah lewat Select.
        public QuotationSelection IsSelected { get; private set; }
        // Terisi = file ada.
        public string? FileName { get; private set; }
        public string? Notes { get; set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        // Kolom file (blob) tidak ikut di-select.
        private static IQueryable<EF.TProcurementQuotation> _Querys
        {
            get
            {
                return DBClass.GetContext().TProcurementQuotations.Select(x => new EF.TProcurementQuotation
                {
                    Id = x.Id,
                    RequestId = x.RequestId,
                    ItemId = x.ItemId,
                    ExternalPartyId = x.ExternalPartyId,
                    ExternalParty = x.ExternalParty,
                    VendorName = x.VendorName,
                    QuotedAmount = x.QuotedAmount,
                    FileName = x.FileName,
                    IsSelected = x.IsSelected,
                    Notes = x.Notes,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy,
                    UpdDate = x.UpdDate,
                    UpdBy = x.UpdBy
                });
            }
        }
        public ProcurementQuotation()
        {
        }
        public ProcurementQuotation(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<ProcurementQuotation>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ProcurementQuotation(EF.TProcurementQuotation row)
        {
            SetData(row);
        }
        private void SetData(EF.TProcurementQuotation row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ProcurementQuotation>();
            RequestId = row.RequestId;
            ItemId = row.ItemId;
            ExternalPartyId = row.ExternalPartyId;
            ExternalPartyName = row.ExternalParty?.Name;
            VendorName = row.VendorName;
            QuotedAmount = row.QuotedAmount;
            IsSelected = (QuotationSelection)row.IsSelected;
            FileName = row.FileName;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
