using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Approval
{
    /// <summary>
    /// Format penomoran dokumen (m_document_number_format), mis. {seq}/{type}/{org}/{month_roman}/{year}.
    /// Satu format per jenis dokumen (uq_number_format); yang dipakai hanya format aktif (lihat GetEffective).
    /// </summary>
    public partial class DocumentNumberFormat : BaseLib
    {
        public class NewRow
        {
            public string DocumentType { get; set; } = null!;
            public string NumberFormat { get; set; } = null!;
            public int SeqPadding { get; set; } = 3;
            public NumberResetPeriod ResetPeriod { get; set; } = NumberResetPeriod.Tahunan;
            public string? Description { get; set; }
        }

        public int Id { get; set; }
        public string DocumentType { get; set; } = null!;
        public string NumberFormat { get; set; } = null!;
        public int SeqPadding { get; set; }
        public NumberResetPeriod ResetPeriod { get; set; }
        public string? Description { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MDocumentNumberFormat> _Query
        {
            get
            {
                return _context.MDocumentNumberFormats
                    .AsQueryable();
            }
        }
        private static IQueryable<EF.MDocumentNumberFormat> _Querys
        {
            get
            {
                return DBClass.GetContext().MDocumentNumberFormats
                    .AsQueryable();
            }
        }
        public DocumentNumberFormat()
        {
            _context = DBClass.GetContext();
        }
        public DocumentNumberFormat(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<DocumentNumberFormat>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal DocumentNumberFormat(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal DocumentNumberFormat(EF.MDocumentNumberFormat row)
        {
            SetData(row);
        }
        private void SetData(EF.MDocumentNumberFormat row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<DocumentNumberFormat>();
            DocumentType = row.DocumentType;
            NumberFormat = row.NumberFormat;
            SeqPadding = row.SeqPadding;
            ResetPeriod = (NumberResetPeriod)row.ResetPeriod;
            Description = row.Description;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
