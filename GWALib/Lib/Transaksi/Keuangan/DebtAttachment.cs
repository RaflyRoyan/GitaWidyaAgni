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
    /// Bukti hutang (t_debt_attachment): surat perjanjian, nota, faktur, foto. Maksimal 5 MB per file.
    /// File tidak ikut dimuat di daftar (GetFile). Boleh ditambah / dihapus pada status hutang apa pun.
    /// </summary>
    public partial class DebtAttachment : BaseLib
    {
        public class NewRow
        {
            public int DebtId { get; set; }
            public byte[] FileData { get; set; } = null!;
            public string FileName { get; set; } = null!;
            public string? FileType { get; set; }
        }
        public record FileContent(byte[] Data, string FileName, string? FileType);

        public int Id { get; set; }
        public int DebtId { get; private set; }
        public string FileName { get; private set; } = null!;
        public string? FileType { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }

        // Kolom file (blob) tidak ikut di-select.
        private static IQueryable<EF.TDebtAttachment> _Querys
        {
            get
            {
                return DBClass.GetContext().TDebtAttachments.Select(x => new EF.TDebtAttachment
                {
                    Id = x.Id,
                    DebtId = x.DebtId,
                    FileName = x.FileName,
                    FileType = x.FileType,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy
                });
            }
        }
        public DebtAttachment()
        {
        }
        public DebtAttachment(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<DebtAttachment>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal DebtAttachment(EF.TDebtAttachment row)
        {
            SetData(row);
        }
        private void SetData(EF.TDebtAttachment row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<DebtAttachment>();
            DebtId = row.DebtId;
            FileName = row.FileName;
            FileType = row.FileType;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
        }
    }
}
