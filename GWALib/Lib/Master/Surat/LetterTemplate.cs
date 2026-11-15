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
    /// Template surat organisasi (m_letter_template): body HTML berplaceholder dan/atau file .docx. Nama unik.
    /// LetterType kode bebas huruf besar (UNDANGAN, PERMOHONAN, SK, ...). Query tidak menarik blob file (lihat <see cref="ListRow"/>);
    /// file diambil lewat GetFile() dan diganti lewat UpdateFile().
    /// </summary>
    public partial class LetterTemplate : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public string LetterType { get; set; } = null!;
            public string? Body { get; set; }
            // Opsional: file template .docx.
            public byte[]? FileData { get; set; }
            public string? FileName { get; set; }
        }
        public record FileContent(byte[] Data, string FileName, string ContentType);

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string LetterType { get; set; } = null!;
        public string? Body { get; set; }
        public bool HasFile { get; private set; }
        public int? FileSize { get; private set; }
        public string? FileName { get; private set; }
        public string? ContentType { get; private set; }
        public ActiveStatus IsActive { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        // Baris hasil query tanpa kolom file_data (blob): kolom template + rantai organisasi + ukuran file.
        private sealed class ListRow
        {
            public EF.MLetterTemplate Template { get; init; } = null!;
            public int? FileSize { get; init; }
        }

        EF.ModelContext _context = null!;
        private IQueryable<ListRow> _Query
        {
            get
            {
                return Project(_context.MLetterTemplates);
            }
        }
        private static IQueryable<ListRow> _Querys
        {
            get
            {
                return Project(DBClass.GetContext().MLetterTemplates);
            }
        }
        private static IQueryable<ListRow> Project(IQueryable<EF.MLetterTemplate> query)
        {
            return query.Select(x => new ListRow
            {
                Template = new EF.MLetterTemplate
                {
                    Id = x.Id,
                    Name = x.Name,
                    LetterType = x.LetterType,
                    Body = x.Body,
                    FileName = x.FileName,
                    ContentType = x.ContentType,
                    IsActive = x.IsActive,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy,
                    UpdDate = x.UpdDate,
                    UpdBy = x.UpdBy
                },
                FileSize = x.FileData == null ? null : x.FileData.Length
            });
        }
        public LetterTemplate()
        {
            _context = DBClass.GetContext();
        }
        public LetterTemplate(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<LetterTemplate>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Template.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal LetterTemplate(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Template.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal LetterTemplate(EF.MLetterTemplate row)
        {
            SetData(row, row.FileData?.Length);
        }
        private LetterTemplate(ListRow row)
        {
            SetData(row);
        }
        private void SetData(ListRow row)
        {
            SetData(row.Template, row.FileSize);
        }
        private void SetData(EF.MLetterTemplate row, int? fileSize)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<LetterTemplate>();
            Name = row.Name;
            LetterType = row.LetterType;
            Body = row.Body;
            HasFile = fileSize != null;
            FileSize = fileSize;
            FileName = row.FileName;
            ContentType = row.ContentType;
            IsActive = (ActiveStatus)row.IsActive;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
