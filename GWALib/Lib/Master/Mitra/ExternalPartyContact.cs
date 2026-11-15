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
    /// Kontak PIC mitra (m_external_party_contact). Maksimal satu kontak utama aktif per mitra: menjadikan kontak utama
    /// otomatis menurunkan kontak utama sebelumnya. Mitra disimpan sebagai ExternalPartyId + GetExternalParty() agar daftar kontak
    /// tidak ikut menarik blob logo mitra.
    /// </summary>
    public partial class ExternalPartyContact : BaseLib
    {
        public class NewRow
        {
            public int ExternalPartyId { get; set; }
            public string Name { get; set; } = null!;
            public string? Position { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public ContactRank IsPrimary { get; set; } = ContactRank.Tambahan;
            public string? Notes { get; set; }
        }

        public int Id { get; set; }
        public int ExternalPartyId { get; set; }
        public string Name { get; set; } = null!;
        public string? Position { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public ContactRank IsPrimary { get; set; }
        public ActiveStatus IsActive { get; set; }
        public string? Notes { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private ExternalParty? ExternalPartyData;

        EF.ModelContext _context = null!;
        private IQueryable<EF.MExternalPartyContact> _Query
        {
            get
            {
                return _context.MExternalPartyContacts.AsQueryable();
            }
        }
        private static IQueryable<EF.MExternalPartyContact> _Querys
        {
            get
            {
                return DBClass.GetContext().MExternalPartyContacts.AsQueryable();
            }
        }
        public ExternalPartyContact()
        {
            _context = DBClass.GetContext();
        }
        public ExternalPartyContact(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<ExternalPartyContact>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ExternalPartyContact(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal ExternalPartyContact(EF.MExternalPartyContact row)
        {
            SetData(row);
        }
        private void SetData(EF.MExternalPartyContact row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<ExternalPartyContact>();
            ExternalPartyId = row.ExternalPartyId;
            Name = row.Name;
            Position = row.Position;
            Phone = row.Phone;
            Email = row.Email;
            IsPrimary = (ContactRank)row.IsPrimary;
            IsActive = (ActiveStatus)row.IsActive;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
