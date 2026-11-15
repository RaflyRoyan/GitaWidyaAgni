using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Master.Anggota
{
    /// <summary>
    /// Profil alumni (m_alumni_profile): satu profil per anggota berstatus Alumni.
    /// </summary>
    public partial class AlumniProfile : BaseLib
    {
        public class NewRow
        {
            public int MemberId { get; set; }
            public short? GraduationYear { get; set; }
            public string? CurrentJob { get; set; }
            public string? Company { get; set; }
            public string? Industry { get; set; }
            public string? City { get; set; }
            public string? LinkedinUrl { get; set; }
            public ContactPermission IsContactable { get; set; } = ContactPermission.TidakBersedia;
            public string? Notes { get; set; }
        }

        public int Id { get; set; }
        public Member Member { get; set; } = null!;
        public short? GraduationYear { get; set; }
        public string? CurrentJob { get; set; }
        public string? Company { get; set; }
        public string? Industry { get; set; }
        public string? City { get; set; }
        public string? LinkedinUrl { get; set; }
        public ContactPermission IsContactable { get; set; }
        public string? Notes { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        EF.ModelContext _context = null!;
        private IQueryable<EF.MAlumniProfile> _Query
        {
            get
            {
                return _context.MAlumniProfiles
                    .Include(x => x.Member).ThenInclude(x => x.User)
                    .AsQueryable();
            }
        }
        private static IQueryable<EF.MAlumniProfile> _Querys
        {
            get
            {
                return DBClass.GetContext().MAlumniProfiles
                    .Include(x => x.Member).ThenInclude(x => x.User)
                    .AsQueryable();
            }
        }
        public AlumniProfile()
        {
            _context = DBClass.GetContext();
        }
        public AlumniProfile(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<AlumniProfile>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal AlumniProfile(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal AlumniProfile(EF.MAlumniProfile row)
        {
            SetData(row);
        }
        private void SetData(EF.MAlumniProfile row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<AlumniProfile>();
            Member = row.Member != null ? new Member(row.Member) : new Member(row.MemberId);
            GraduationYear = row.GraduationYear;
            CurrentJob = row.CurrentJob;
            Company = row.Company;
            Industry = row.Industry;
            City = row.City;
            LinkedinUrl = row.LinkedinUrl;
            IsContactable = (ContactPermission)row.IsContactable;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
