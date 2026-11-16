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
    /// Dokumen pendukung anggaran (t_budget_document), mis. RAB atau proposal. Maksimal 10 MB per file.
    /// File tidak ikut dimuat di daftar (GetFile). Boleh ditambah / dihapus pada status anggaran apa pun.
    /// </summary>
    public partial class BudgetDocument : BaseLib
    {
        public class NewRow
        {
            public int BudgetId { get; set; }
            public byte[] FileData { get; set; } = null!;
            public string FileName { get; set; } = null!;
            public string? FileType { get; set; }
        }
        public record FileContent(byte[] Data, string FileName, string? FileType);

        public int Id { get; set; }
        public int BudgetId { get; private set; }
        public string FileName { get; private set; } = null!;
        public string? FileType { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }

        // Kolom file (blob) tidak ikut di-select.
        private static IQueryable<EF.TBudgetDocument> _Querys
        {
            get
            {
                return DBClass.GetContext().TBudgetDocuments.Select(x => new EF.TBudgetDocument
                {
                    Id = x.Id,
                    BudgetId = x.BudgetId,
                    FileName = x.FileName,
                    FileType = x.FileType,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy
                });
            }
        }
        public BudgetDocument()
        {
        }
        public BudgetDocument(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<BudgetDocument>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal BudgetDocument(EF.TBudgetDocument row)
        {
            SetData(row);
        }
        private void SetData(EF.TBudgetDocument row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<BudgetDocument>();
            BudgetId = row.BudgetId;
            FileName = row.FileName;
            FileType = row.FileType;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
        }
    }
}
