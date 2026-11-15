using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Surat
{
    /// <summary>
    /// Kategori arsip dokumen organisasi (m_document_category), mis. SK, Proposal, LPJ, Notulen. Nama unik.
    /// </summary>
    public partial class DocumentCategory : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MDocumentCategory> _Query
        {
            get
            {
                return _context.MDocumentCategories
                    .AsQueryable();
            }
        }
        private static IQueryable<EF.MDocumentCategory> _Querys
        {
            get
            {
                return DBClass.GetContext().MDocumentCategories
                    .AsQueryable();
            }
        }
        public DocumentCategory()
        {
            _context = DBClass.GetContext();
        }
        public DocumentCategory(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<DocumentCategory>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal DocumentCategory(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal DocumentCategory(EF.MDocumentCategory row)
        {
            SetData(row);
        }
        private void SetData(EF.MDocumentCategory row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<DocumentCategory>();
            Name = row.Name;
            Description = row.Description;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
