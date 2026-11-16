using GWALib.General;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Keuangan
{
    /// <summary>
    /// Realisasi pemakaian dana pengajuan (t_funding_realization), mis. konsumsi rapat 20 orang. Hanya bisa diubah saat pengajuan Disbursed
    /// dan pertanggungjawaban masih Belum; setiap perubahan menghitung ulang RealizedAmount pengajuan.
    /// Nota (opsional, maksimal 5 MB) tidak ikut dimuat di daftar (GetReceipt / UpdateReceipt).
    /// </summary>
    public partial class FundingRealization : BaseLib
    {
        public class NewRow
        {
            public int FundingRequestId { get; set; }
            public DateOnly RealizationDate { get; set; }
            public string Description { get; set; } = null!;
            public decimal? Quantity { get; set; }
            public string? Unit { get; set; }
            public decimal Amount { get; set; }
            public string? Notes { get; set; }
            // Opsional: foto / scan nota.
            public byte[]? Receipt { get; set; }
            public string? ReceiptFileName { get; set; }
            public string? ReceiptContentType { get; set; }
        }
        public record FileContent(byte[] Data, string FileName, string? ContentType);

        public int Id { get; set; }
        public int FundingRequestId { get; private set; }
        public DateOnly RealizationDate { get; set; }
        public string Description { get; set; } = null!;
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
        // Terisi = nota ada.
        public string? ReceiptFileName { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        // Kolom nota (blob) tidak ikut di-select.
        private static IQueryable<EF.TFundingRealization> _Querys
        {
            get
            {
                return DBClass.GetContext().TFundingRealizations.Select(x => new EF.TFundingRealization
                {
                    Id = x.Id,
                    FundingRequestId = x.FundingRequestId,
                    RealizationDate = x.RealizationDate,
                    Description = x.Description,
                    Quantity = x.Quantity,
                    Unit = x.Unit,
                    Amount = x.Amount,
                    ReceiptFileName = x.ReceiptFileName,
                    Notes = x.Notes,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy,
                    UpdDate = x.UpdDate,
                    UpdBy = x.UpdBy
                });
            }
        }
        public FundingRealization()
        {
        }
        public FundingRealization(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<FundingRealization>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal FundingRealization(EF.TFundingRealization row)
        {
            SetData(row);
        }
        private void SetData(EF.TFundingRealization row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<FundingRealization>();
            FundingRequestId = row.FundingRequestId;
            RealizationDate = row.RealizationDate;
            Description = row.Description;
            Quantity = row.Quantity;
            Unit = row.Unit;
            Amount = row.Amount;
            Notes = row.Notes;
            ReceiptFileName = row.ReceiptFileName;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
