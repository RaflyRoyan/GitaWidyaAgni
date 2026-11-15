using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Mitra
{
    /// <summary>
    /// Direktori mitra / vendor / sponsor milik organisasi (m_external_party), tetap ada saat pengurus berganti. Nama unik.
    /// Query tidak menarik blob logo (lihat <see cref="ListRow"/>); logo diambil lewat GetLogo() dan diganti lewat UpdateLogo().
    /// </summary>
    public partial class ExternalParty : BaseLib
    {
        public class NewRow
        {
            public string Name { get; set; } = null!;
            public ExternalPartyType PartyType { get; set; } = ExternalPartyType.Perusahaan;
            public string? Industry { get; set; }
            public string? Address { get; set; }
            public string? City { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public string? Website { get; set; }
            public string? Instagram { get; set; }
            public string? Notes { get; set; }
            // Opsional: isi file logo asli dari upload; di-resize/kompres oleh ImageUtility.
            public byte[]? Logo { get; set; }
            public string? LogoFileName { get; set; }
            // Opsional: kontak yang langsung dibuat bersama mitra (ExternalPartyId diisi otomatis).
            public List<ExternalPartyContact.NewRow>? Contacts { get; set; }
        }
        public record LogoData(byte[] Data, string FileName, string ContentType);

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ExternalPartyType PartyType { get; set; }
        public string? Industry { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Instagram { get; set; }
        public bool HasLogo { get; private set; }
        public int? LogoSize { get; private set; }
        public string? LogoFileName { get; private set; }
        public string? LogoContentType { get; private set; }
        public string? Notes { get; set; }
        public ExternalPartyStatus Status { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private List<ExternalPartyContact>? ContactList;

        // Baris hasil query tanpa kolom logo (blob): kolom mitra + ukuran logo.
        private sealed class ListRow
        {
            public EF.MExternalParty ExternalParty { get; init; } = null!;
            public int? LogoSize { get; init; }
        }

        EF.ModelContext _context = null!;
        private IQueryable<ListRow> _Query
        {
            get
            {
                return Project(_context.MExternalParties);
            }
        }
        private static IQueryable<ListRow> _Querys
        {
            get
            {
                return Project(DBClass.GetContext().MExternalParties);
            }
        }
        private static IQueryable<ListRow> Project(IQueryable<EF.MExternalParty> query)
        {
            return query.Select(x => new ListRow
            {
                ExternalParty = new EF.MExternalParty
                {
                    Id = x.Id,
                    Name = x.Name,
                    PartyType = x.PartyType,
                    Industry = x.Industry,
                    Address = x.Address,
                    City = x.City,
                    Phone = x.Phone,
                    Email = x.Email,
                    Website = x.Website,
                    Instagram = x.Instagram,
                    LogoFileName = x.LogoFileName,
                    LogoContentType = x.LogoContentType,
                    Notes = x.Notes,
                    Status = x.Status,
                    InsDate = x.InsDate,
                    InsBy = x.InsBy,
                    UpdDate = x.UpdDate,
                    UpdBy = x.UpdBy
                },
                LogoSize = x.Logo == null ? null : x.Logo.Length
            });
        }
        public ExternalParty()
        {
            _context = DBClass.GetContext();
        }
        public ExternalParty(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<ExternalParty>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.ExternalParty.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ExternalParty(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.ExternalParty.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ExternalParty(EF.MExternalParty row)
        {
            SetData(row, row.Logo?.Length);
        }
        private ExternalParty(ListRow row)
        {
            SetData(row);
        }
        private void SetData(ListRow row)
        {
            SetData(row.ExternalParty, row.LogoSize);
        }
        private void SetData(EF.MExternalParty row, int? logoSize)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ExternalParty>();
            Name = row.Name;
            PartyType = (ExternalPartyType)row.PartyType;
            Industry = row.Industry;
            Address = row.Address;
            City = row.City;
            Phone = row.Phone;
            Email = row.Email;
            Website = row.Website;
            Instagram = row.Instagram;
            HasLogo = logoSize != null;
            LogoSize = logoSize;
            LogoFileName = row.LogoFileName;
            LogoContentType = row.LogoContentType;
            Notes = row.Notes;
            Status = (ExternalPartyStatus)row.Status;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
