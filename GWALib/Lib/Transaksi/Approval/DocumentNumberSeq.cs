using GWALib.General;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Approval
{
    /// <summary>
    /// Nomor urut terakhir dokumen per (jenis dokumen, periode) (t_document_number_seq).
    /// Period key mengikuti reset_period format nomor: 2026 (tahunan), 2026-10 (bulanan), ALL (tidak pernah).
    /// Nomor diambil lewat GenerateNumber, yang menaikkan last_number secara atomik (LAST_INSERT_ID).
    /// </summary>
    public partial class DocumentNumberSeq : BaseLib
    {
        public int Id { get; private set; }
        public string DocumentType { get; private set; } = null!;
        public string PeriodKey { get; private set; } = null!;
        public int LastNumber { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TDocumentNumberSeq> _Querys
        {
            get
            {
                return DBClass.GetContext().TDocumentNumberSeqs.AsQueryable();
            }
        }
        public DocumentNumberSeq()
        {
        }
        public DocumentNumberSeq(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<DocumentNumberSeq>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal DocumentNumberSeq(int id)
        {
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal DocumentNumberSeq(EF.TDocumentNumberSeq row)
        {
            SetData(row);
        }
        private void SetData(EF.TDocumentNumberSeq row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<DocumentNumberSeq>();
            DocumentType = row.DocumentType;
            PeriodKey = row.PeriodKey;
            LastNumber = row.LastNumber;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
