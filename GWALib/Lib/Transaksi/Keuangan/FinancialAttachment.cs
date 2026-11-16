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
    /// Bukti transaksi keuangan (t_financial_attachment): nota, kuitansi, foto. Maksimal 5 MB per file.
    /// File tidak ikut dimuat di daftar (GetFile). Tidak bisa ditambah / dihapus pada transaksi yang sudah Void.
    /// </summary>
    public partial class FinancialAttachment : BaseLib
    {
        public class NewRow
        {
            public int TransactionId { get; set; }
            public byte[] FileData { get; set; } = null!;
            public string FileName { get; set; } = null!;
            public string? FileType { get; set; }
        }
        public record FileContent(byte[] Data, string FileName, string? FileType);

        public int Id { get; set; }
        public int TransactionId { get; private set; }
        public string FileName { get; private set; } = null!;
        public string? FileType { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }

        // Kolom file (blob) tidak ikut di-select.
        private static IQueryable<EF.TFinancialAttachment> _Querys
        {
            get
            {
                return DBClass.GetContext().TFinancialAttachments.Select(x => new EF.TFinancialAttachment
                {
                    Id = x.Id,
                    TransactionId = x.TransactionId,
                    FileName = x.FileName,
                    FileType = x.FileType,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy
                });
            }
        }
        public FinancialAttachment()
        {
        }
        public FinancialAttachment(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<FinancialAttachment>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal FinancialAttachment(EF.TFinancialAttachment row)
        {
            SetData(row);
        }
        private void SetData(EF.TFinancialAttachment row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<FinancialAttachment>();
            TransactionId = row.TransactionId;
            FileName = row.FileName;
            FileType = row.FileType;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
        }
    }
}
