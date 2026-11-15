using GWALib.General;
using GWAShared.Enums;
using GWAShared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWALib.Lib.Transaksi.Anggota
{
    /// <summary>
    /// Pendaftar open recruitment (t_recruitment_applicant). Pendaftaran tidak butuh login (Apply).
    /// Alur status: Submitted → Screening → Interview → Accepted / Rejected (boleh melompat maju, tidak bisa mundur).
    /// Accepted mengecek kuota dan membuat data anggota (m_member) bila pendaftar belum terhubung ke anggota.
    /// </summary>
    public partial class RecruitmentApplicant : BaseLib
    {
        public class NewRow
        {
            public int RecruitmentId { get; set; }
            public MemberCategory ApplicantCategory { get; set; } = MemberCategory.Mahasiswa;
            public string? Nim { get; set; }
            public string Fullname { get; set; } = null!;
            public string Email { get; set; } = null!;
            public string? Phone { get; set; }
            public string? ProgramStudi { get; set; }
            public short? Angkatan { get; set; }
            public string? Institution { get; set; }
            public string? Motivation { get; set; }
        }

        public int Id { get; private set; }
        public int RecruitmentId { get; private set; }
        public int? MemberId { get; private set; }
        public MemberCategory ApplicantCategory { get; private set; }
        public string? Nim { get; private set; }
        public string Fullname { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public string? Phone { get; private set; }
        public string? ProgramStudi { get; private set; }
        public short? Angkatan { get; private set; }
        public string? Institution { get; private set; }
        public string? Motivation { get; private set; }
        public ApplicantStatus Status { get; private set; }
        public int? ReviewedBy { get; private set; }
        public DateTime? ReviewedAt { get; private set; }
        public string? Notes { get; private set; }
        public DateTime InsDate { get; private set; }
        public int? InsBy { get; private set; }
        public DateTime? UpdDate { get; private set; }
        public int? UpdBy { get; private set; }

        private static IQueryable<EF.TRecruitmentApplicant> _Querys
        {
            get
            {
                return DBClass.GetContext().TRecruitmentApplicants.AsQueryable();
            }
        }
        public RecruitmentApplicant()
        {
        }
        public RecruitmentApplicant(string encKey)
        {
            var id = Convert.ToInt32(encKey.DecryptString<RecruitmentApplicant>());
            var row = _Querys.SingleOrDefault(x => x.Id == id);
            if (row != null)
            {
                SetData(row);
            }
        }
        internal RecruitmentApplicant(EF.TRecruitmentApplicant row)
        {
            SetData(row);
        }
        private void SetData(EF.TRecruitmentApplicant row)
        {
            Id = row.Id;
            EncDecrKey = Id.ToString().EncryptString<RecruitmentApplicant>();
            RecruitmentId = row.RecruitmentId;
            MemberId = row.MemberId;
            ApplicantCategory = (MemberCategory)row.ApplicantCategory;
            Nim = row.Nim;
            Fullname = row.Fullname;
            Email = row.Email;
            Phone = row.Phone;
            ProgramStudi = row.ProgramStudi;
            Angkatan = row.Angkatan;
            Institution = row.Institution;
            Motivation = row.Motivation;
            Status = (ApplicantStatus)row.Status;
            ReviewedBy = row.ReviewedBy;
            ReviewedAt = row.ReviewedAt;
            Notes = row.Notes;
            InsDate = row.InsDate;
            InsBy = row.InsBy;
            UpdDate = row.UpdDate;
            UpdBy = row.UpdBy;
        }
    }
}
