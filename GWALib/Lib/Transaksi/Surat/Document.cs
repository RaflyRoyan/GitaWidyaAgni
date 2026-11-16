using GWALib.General;
using GWALib.General.Utilities;
using GWALib.Lib.Master.Surat;
using GWAShared.Enums;
using GWAShared.Extensions;
using GWAShared.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Surat
{
    /// <summary>
    /// Arsip dokumen organisasi (t_document): AD/ART, SOP, notulen, dll. Kategori harus aktif. File wajib, maksimal 20 MB, dan tidak dimuat
    /// di daftar (GetFile / UpdateFile); FileSize diisi otomatis. Visibility Public / Member / Pengurus: GetDataList menerima daftar
    /// visibilitas yang boleh dilihat pemanggil (mis. landing page hanya Public). Hapus permanen.
    /// </summary>
    public partial class Document : BaseLib
    {
        public class NewRow
        {
            public int CategoryId { get; set; }
            public string Title { get; set; } = null!;
            public string? Description { get; set; }
            public byte[] FileData { get; set; } = null!;
            public string FileName { get; set; } = null!;
            public string? ContentType { get; set; }
            public DocumentVisibility Visibility { get; set; } = DocumentVisibility.Member;
        }
        public record FileContent(byte[] Data, string FileName, string? ContentType);

        public const int MAX_FILE_BYTES = 20 * 1024 * 1024;

        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; private set; } = null!;
        public int UploadedBy { get; private set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string FileName { get; private set; } = null!;
        public string? ContentType { get; private set; }
        public int? FileSize { get; private set; }
        public DocumentVisibility Visibility { get; set; }
        public DateTime InsDate { get; private set; }
        public DateTime? UpdDate { get; private set; }

        // Kolom file (blob) tidak ikut di-select.
        private static IQueryable<EF.TDocument> _Querys
        {
            get
            {
                return DBClass.GetContext().TDocuments.Select(x => new EF.TDocument
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    Category = x.Category,
                    UploadedBy = x.UploadedBy,
                    Title = x.Title,
                    Description = x.Description,
                    FileName = x.FileName,
                    ContentType = x.ContentType,
                    FileSize = x.FileSize,
                    Visibility = x.Visibility,
                    InsDate = x.InsDate,
                    UpdDate = x.UpdDate
                });
            }
        }
        public Document()
        {
        }
        public Document(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<Document>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Document(EF.TDocument row)
        {
            SetData(row);
        }
        private void SetData(EF.TDocument row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Document>();
            CategoryId = row.CategoryId;
            CategoryName = row.Category?.Name ?? string.Empty;
            UploadedBy = row.UploadedBy;
            Title = row.Title;
            Description = row.Description;
            FileName = row.FileName;
            ContentType = row.ContentType;
            FileSize = row.FileSize;
            Visibility = (DocumentVisibility)row.Visibility;
            InsDate = row.InsDate;
            UpdDate = row.UpdDate;
        }
    }
}
