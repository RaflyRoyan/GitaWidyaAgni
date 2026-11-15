using GWALib.General;
using GWALib.Lib.Master.Akses;
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
    /// Data anggota (m_member), mahasiswa maupun umum. NIM wajib untuk mahasiswa; NIM dan nomor anggota (NIA) unik.
    /// User (akun login) opsional dan satu user hanya untuk satu anggota.
    /// </summary>
    public partial class Member : BaseLib
    {
        public class NewRow
        {
            public int? UserId { get; set; }
            public string? MemberNumber { get; set; }
            public MemberType MemberType { get; set; } = MemberType.Anggota;
            public DateOnly? JoinedAt { get; set; }
            public MemberCategory MemberCategory { get; set; } = MemberCategory.Mahasiswa;
            public string? Nim { get; set; }
            public string Fullname { get; set; } = null!;
            public Gender? Gender { get; set; }
            public DateOnly? BirthDate { get; set; }
            public string? Fakultas { get; set; }
            public string? ProgramStudi { get; set; }
            public short? Angkatan { get; set; }
            public string? Institution { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public string? Address { get; set; }
        }

        public int Id { get; set; }
        public UserAccount? User { get; set; }
        public string? MemberNumber { get; set; }
        public MemberType MemberType { get; set; }
        public DateOnly? JoinedAt { get; set; }
        public DateOnly? LeftAt { get; set; }
        public MemberCategory MemberCategory { get; set; }
        public string? Nim { get; set; }
        public string Fullname { get; set; } = null!;
        public Gender? Gender { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? Fakultas { get; set; }
        public string? ProgramStudi { get; set; }
        public short? Angkatan { get; set; }
        public string? Institution { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public MemberStatus Status { get; set; }
        public DateTime InsDate { get; set; }
        public int? InsBy { get; set; }
        public DateTime? UpdDate { get; set; }
        public int? UpdBy { get; set; }

        private List<MemberPhoto.Info>? PhotoList;
        private AlumniProfile? AlumniProfileData;
        private bool _isAlumniProfileLoaded;

        EF.ModelContext _context = null!;
        private IQueryable<EF.MMember> _Query
        {
            get
            {
                return _context.MMembers.Include(x => x.User).AsQueryable();
            }
        }
        private static IQueryable<EF.MMember> _Querys
        {
            get
            {
                return DBClass.GetContext().MMembers.Include(x => x.User).AsQueryable();
            }
        }
        public Member()
        {
            _context = DBClass.GetContext();
        }
        public Member(string encKey)
        {
            _context = DBClass.GetContext();
            var key = encKey.DecryptString<Member>();
            var id = Convert.ToInt32(key);
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Member(int id)
        {
            _context = DBClass.GetContext();
            var row = _Query.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal Member(EF.MMember row)
        {
            SetData(row);
        }
        private void SetData(EF.MMember row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<Member>();
            User = row.User != null ? new UserAccount(row.User) : row.UserId != null ? new UserAccount(row.UserId.Value) : null;
            MemberNumber = row.MemberNumber;
            MemberType = (MemberType)row.MemberType;
            JoinedAt = row.JoinedAt;
            LeftAt = row.LeftAt;
            MemberCategory = (MemberCategory)row.MemberCategory;
            Nim = row.Nim;
            Fullname = row.Fullname;
            Gender = row.Gender != null ? (Gender)row.Gender : null;
            BirthDate = row.BirthDate;
            Fakultas = row.Fakultas;
            ProgramStudi = row.ProgramStudi;
            Angkatan = row.Angkatan;
            Institution = row.Institution;
            Phone = row.Phone;
            Email = row.Email;
            Address = row.Address;
            Status = (MemberStatus)row.Status;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
